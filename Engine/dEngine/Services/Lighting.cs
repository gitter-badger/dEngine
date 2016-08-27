// Lighting.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using NLog;

using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace dEngine.Services
{
	/// <summary>
	/// The Lighting service controls environmental variables.
	/// </summary>
	/// <seealso cref="Skybox" />
	[TypeId(49), ExplorerOrder(4)]
	public class Lighting : Service
	{
		internal static ILogger InternalLogger = LogService.GetLogger();

		internal static readonly object LightSync = new object();

		internal static ConstantBuffer<LightingConstantData> LightingConstantBuffer =
			new ConstantBuffer<LightingConstantData>();

		private readonly Sky _defaultSkybox;

		// stars fade out @ : 310 - 390
		// stars fade in @ : 1050 - 1130
		private readonly List<MinutePair<Colour>> _timeColours = new List<MinutePair<Colour>>(12)
		{
			// night
			new MinutePair<Colour>(0, new Colour(0, 0, 0), new Colour(0, 0, 0)),
			new MinutePair<Colour>(180, new Colour(0, 0, 0), new Colour(0, 0, 0)),

			// blue
			new MinutePair<Colour>(250, new Colour(0.01f, 0.01f, 0.01f), new Colour(0.2f, 0.2f, 0.2f)),
			// orange top
			new MinutePair<Colour>(330, new Colour(0.2f, 0.15f, 0.01f), new Colour(0.4f, 0.3f, 0.3f)),
			// purple bottom
			new MinutePair<Colour>(360, new Colour(0.2f, 0.15f, 0.01f), new Colour(0.3f, 0.2f, 0.3f)),
			// green top
			new MinutePair<Colour>(370, new Colour(0.46f, 0.43f, 0.34f), new Colour(0.53f, 0.46f, 0.53f)),

			// day
			new MinutePair<Colour>(390, new Colour(1.0f, 1.0f, 1.0f), new Colour(1.0f, 1.0f, 1.0f)),
			new MinutePair<Colour>(1050, new Colour(1.0f, 1.0f, 1.0f), new Colour(1.0f, 1.0f, 1.0f)),

			// orange
			new MinutePair<Colour>(1080, new Colour(0.4f, 0.2f, 0.05f), new Colour(0.4f, 0.3f, 0.2f)),

			// purple peroid
			new MinutePair<Colour>(1100, new Colour(0.0f, 0.0f, 0.0f), new Colour(0.3f, 0.2f, 0.3f)),
			new MinutePair<Colour>(1200, new Colour(0.0f, 0.0f, 0.0f), new Colour(0.3f, 0.2f, 0.3f)),

			// night
			new MinutePair<Colour>(1260, new Colour(0.0f, 0.0f, 0.0f), new Colour(0.0f, 0.0f, 0.0f))
		};

		private readonly List<MinutePair<float>> _timeStarAlpha = new List<MinutePair<float>>(4)
		{
			new MinutePair<float>(310, 1),
			new MinutePair<float>(390, 0),
			new MinutePair<float>(1100, 0),
			new MinutePair<float>(1200, 1)
		};

		private Colour _ambientColour;
		private float _latitude;
		private float _longitude;
		private Sky _skybox;
		private Colour _sunColour;
		private Vector3 _sunVector;

		private TimeSpan _timeOfDay;
		internal ConstantBuffer<SkyboxConstantData> SkyboxConstantBuffer;

		/// <inheritdoc />
		public Lighting()
		{
            LightingChanged = new Signal<bool>(this);

            SkyboxConstantBuffer = new ConstantBuffer<SkyboxConstantData>();
			SunColour = SunColour;
			SkyColourShiftUpper = SkyColourShiftUpper;
			SkyColourShiftLower = SkyColourShiftLower;
			//OutdoorAmbient = new Colour(0.7f, 0.7f, 0.7f);
			//AmbientColour = Colour.Black;
			SkyLightIntensity = 1.0f;
			IsLowerHemisphereBlack = true;
			SunColour = Colour.White;
			TimeOfDay = new TimeSpan(10, 0, 0);

			Latitude = 56.243350f;
			Longitude = -2.636719f;

			_defaultSkybox = new Sky();
			Skybox = _defaultSkybox;
		}

		internal float StarAlpha
		{
			get { return _starAlpha; }
			private set
			{
				_starAlpha = value;
				SkyboxConstantBuffer.Data.StarAlpha = value;
			}
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<Lighting>();
		}

		internal static void AddPointLight(PointLight light)
		{
		}

		internal static void RemovePointLight(PointLight light)
		{
		}

		internal static void AddSpotLight(PointLight pointLight)
		{
			throw new NotImplementedException();
		}

		internal static void RemoveSpotLight(SpotLight spotLight)
		{
			//throw new NotImplementedException();
		}

		private class MinutePair<TIndex>
		{
			public MinutePair(int minutes, TIndex upper, TIndex lower = default(TIndex))
			{
				Minutes = minutes;
				Upper = upper;
				Lower = lower;
			}

			public TIndex Upper { get; }
			public TIndex Lower { get; }
			public int Minutes { get; }
		}

		internal struct SkyboxConstantData : IConstantBufferData
		{
			public Colour SkyBlendColourUpper;
			public Colour SkyBlendColourLower;
			public float StarAlpha;

			public void WriteTo(DeviceContext context, ref Buffer buffer)
			{
				DataStream stream;
				context.MapSubresource(buffer, MapMode.WriteDiscard, 0, out stream);
				{
					stream.Write(this);
				}
				context.UnmapSubresource(buffer, 0);
			}
		}

        /// <summary>
        /// Fired when a property specific to <see cref="Lighting"/> is changed.
        /// </summary>
	    public readonly Signal<bool> LightingChanged;

        [StructLayout(LayoutKind.Explicit, Pack = 16, Size = 64)]
		internal struct LightingConstantData : IConstantBufferData
		{
			[FieldOffset(0)] public PointLightData SunLight;
			[FieldOffset(32)] public Colour AmbientColour;
			[FieldOffset(48)] public Colour OutdoorAmbient;
		}

		#region Properties

		/// <summary>
		/// The colour of ambient lighting.
		/// </summary>
		[InstMember(1), EditorVisible("Appearance"), Obsolete]
		public Colour AmbientColour
		{
			get { return _ambientColour; }
			set
			{
				_ambientColour = value;
				UpdateSun();
                LightingChanged.Fire();
                NotifyChanged(nameof(AmbientColour));
			}
		}

		/// <summary>
		/// The colour of the sunlight.
		/// </summary>
		[InstMember(2), EditorVisible("Appearance")]
		public Colour SunColour
		{
			get { return _sunColour; }
			set
			{
				_sunColour = value;
				UpdateSun();
                LightingChanged.Fire();
                NotifyChanged(nameof(SunColour));
			}
		}

		/// <summary>
		/// The time of day.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public TimeSpan TimeOfDay
		{
			get { return _timeOfDay; }
			set
			{
				if (value == _timeOfDay) return;
				_timeOfDay = value;
				UpdateSun();
				UpdateTime();
                LightingChanged.Fire();
                NotifyChanged();
			}
		}

		/// <summary>
		/// The latitude of the map for sun direction calculations.
		/// </summary>
		[InstMember(4), EditorVisible("Data")]
		public float Latitude
		{
			get { return _latitude; }
			set
			{
				_latitude = value;
                LightingChanged.Fire();
                NotifyChanged(nameof(Latitude));
				UpdateSun();
			}
		}

		/// <summary>
		/// The longitude of the map for sun direction calculations.
		/// </summary>
		[InstMember(5), EditorVisible("Data")]
		public float Longitude
		{
			get { return _longitude; }
			set
			{
				_longitude = value;
                LightingChanged.Fire();
                NotifyChanged(nameof(Longitude));
				UpdateSun();
			}
		}

		private Colour _outdoorAmbient;

		private float _skyLightIntensity;

		private bool _isLowerHemisphereBlack;


		/// <summary>
		/// The darkest outdoor colour.
		/// </summary>
		[InstMember(6), EditorVisible("Appearance"), Obsolete]
		public Colour OutdoorAmbient
		{
			get { return _outdoorAmbient; }
			set
			{
				if (value == _outdoorAmbient)
					return;
				_outdoorAmbient = value;
				UpdateSun();
                LightingChanged.Fire();
                NotifyChanged(nameof(OutdoorAmbient));
			}
		}

		[InstMember(7), EditorVisible("Appearance")]
		public float SkyLightIntensity
		{
			get { return _skyLightIntensity; }
			set
			{
				if (value == _skyLightIntensity)
					return;

				_skyLightIntensity = value;
                LightingChanged.Fire();
                NotifyChanged(nameof(SkyLightIntensity));
			}
		}

		[InstMember(8), EditorVisible("Appearance")]
		public bool IsLowerHemisphereBlack
		{
			get { return _isLowerHemisphereBlack; }
			set
			{
				if (value == _isLowerHemisphereBlack)
					return;

				_isLowerHemisphereBlack = value;
                LightingChanged.Fire();
                NotifyChanged(nameof(IsLowerHemisphereBlack));
			}
		}

		/// <summary>
		/// A unit vector of the direction the sun is facing.
		/// </summary>
		public Vector3 SunVector
		{
			get { return _sunVector; }
			private set
			{
				if (value == _sunVector)
					return;

				_sunVector = value;

                LightingChanged.Fire();
                NotifyChanged(nameof(SunVector));
			}
		}

		/// <summary>
		/// The current skybox.
		/// </summary>
		[EditorVisible("Data")]
		public Sky Skybox
		{
			get { return _skybox; }
			set
			{
				var skyboxRO = Game.Workspace.RenderObjectProvider.SkyboxRO;

				if (_skybox != null)
					skyboxRO.Clear();

				if (value == null)
					value = _defaultSkybox;

				_skybox = value;

                LightingChanged.Fire(true);
                skyboxRO.Add(value);
				value.UpdateRenderData();
			}
		}

		private Colour _skyColourShiftUpper;
		private Colour _skyColourShiftLower;
		private float _starAlpha;

		#endregion

		#region Methods

		/// <summary>
		/// Sets the time to the given amount of minutes past 12:00:00.
		/// </summary>
		/// <param name="minutes">The number of minutes past midnight.</param>
		public void SetMinutesAfterMidnight(double minutes)
		{
			_timeOfDay = TimeSpan.FromMinutes(minutes);
			UpdateSun();
			UpdateTime();
			NotifyChanged(nameof(TimeOfDay));
		}

		/// <summary>
		/// Returns the number of minutes past 12:00:00.
		/// </summary>
		public double GetMinutesAfterMidnight()
		{
			return _timeOfDay.totalMinutes;
		}

		internal Colour SkyColourShiftUpper
		{
			get { return _skyColourShiftUpper; }
			set
			{
				_skyColourShiftUpper = value;
				SkyboxConstantBuffer.Data.SkyBlendColourUpper = value;
			}
		}

		internal Colour SkyColourShiftLower
		{
			get { return _skyColourShiftLower; }
			set
			{
				_skyColourShiftLower = value;
				SkyboxConstantBuffer.Data.SkyBlendColourLower = value;
			}
		}

	    internal static SharpDX.Vector3 LightDirection;

	    private void UpdateTime()
		{
			var colourPair = GetPairDelta(_timeColours);
			SkyColourShiftUpper = colourPair.Item1.Upper.lerp(colourPair.Item2.Upper, colourPair.Item3);
			SkyColourShiftLower = colourPair.Item1.Lower.lerp(colourPair.Item2.Lower, colourPair.Item3);

			var starAlphaPair = GetPairDelta(_timeStarAlpha);
			StarAlpha = Mathf.Lerp(starAlphaPair.Item1.Upper, starAlphaPair.Item2.Upper, starAlphaPair.Item3);
		}

		private void UpdateSun()
		{
			var sunAngles = CalculateSunPosition(DateTime.Today() + _timeOfDay, _longitude, _latitude);
			var sunCF = CFrame.FromAxisAngle(Vector3.Up, (float)sunAngles.Azimuth) *
						CFrame.FromAxisAngle(Vector3.Right, (float)sunAngles.Altitude);

			var sunVector = sunCF.forward;
			SunVector = sunVector;

			LightingConstantBuffer.Data.AmbientColour = _ambientColour;
			LightingConstantBuffer.Data.OutdoorAmbient = _outdoorAmbient;
			LightingConstantBuffer.Data.SunLight = new PointLightData
			{
				Position = sunVector,
				Colour = SunColour
			};
			lock (Renderer.Locker)
			{
				LightingConstantBuffer.Update(ref Renderer.Context);
			}

            Shadows.Direction = new SharpDX.Vector3(-sunVector.x, sunVector.y, -sunVector.z);
        }

		private Tuple<MinutePair<T>, MinutePair<T>, float> GetPairDelta<T>(IList<MinutePair<T>> list)
		{
			var minutes = GetMinutesAfterMidnight();

			for (var i = 0; i < list.Count; i++)
			{
				var isLast = i == list.Count - 1;
				var current = list[i];
				var next = list[isLast ? 0 : i + 1];

				if (minutes >= current.Minutes && minutes < next.Minutes || isLast)
				{
					var delta = ((float)minutes - current.Minutes) / (next.Minutes - current.Minutes);
					return new Tuple<MinutePair<T>, MinutePair<T>, float>(current, next, delta);
				}
			}

			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Returns the Position of the sun at the given time and location in altitude and azimuth.
		/// </summary>
		private static SunCoords CalculateSunPosition(DateTime dateTime, double latitude, double longitude)
		{
			// Convert to UTC  
			dateTime = dateTime.utc;

			// Number of days from J2000.0.  
			var julianDate = 367 * dateTime.year -
							 (int)(7.0 / 4.0 * (dateTime.year +
												(int)((dateTime.month + 9.0) / 12.0))) +
							 (int)(275.0 * dateTime.month / 9.0) +
							 dateTime.day - 730531.5;

			var julianCenturies = julianDate / 36525.0;

			// Sidereal Time  
			var siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

			var siderealTimeUT = siderealTimeHours +
								 366.2422 / 365.2422 * dateTime.time.totalHours;

			var siderealTime = siderealTimeUT * 15 + longitude;

			// Refine to number of days (fractional) to specific time.  
			julianDate += dateTime.time.totalHours / 24.0;
			julianCenturies = julianDate / 36525.0;

			// Solar Coordinates  
			var meanLongitude = CorrectAngle(Mathf.Deg2Rad *
											 (280.466 + 36000.77 * julianCenturies));

			var meanAnomaly = CorrectAngle(Mathf.Deg2Rad *
										   (357.529 + 35999.05 * julianCenturies));

			var equationOfCenter = Mathf.Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
													Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

			var elipticalLongitude =
				CorrectAngle(meanLongitude + equationOfCenter);

			var obliquity = (23.439 - 0.013 * julianCenturies) * Mathf.Deg2Rad;

			// Right Ascension  
			var rightAscension = Math.Atan2(
				Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
				Math.Cos(elipticalLongitude));

			var declination = Math.Asin(
				Math.Sin(rightAscension) * Math.Sin(obliquity));

			// Horizontal Coordinates  
			var hourAngle = CorrectAngle(siderealTime * Mathf.Deg2Rad) - rightAscension;

			if (hourAngle > Math.PI)
			{
				hourAngle -= 2 * Math.PI;
			}

			var altitude = Math.Asin(Math.Sin(latitude * Mathf.Deg2Rad) *
									 Math.Sin(declination) + Math.Cos(latitude * Mathf.Deg2Rad) *
									 Math.Cos(declination) * Math.Cos(hourAngle));

			// Nominator and denominator for calculating Azimuth  
			// angle. Needed to test which quadrant the angle is }  
			var aziNom = -Math.Sin(hourAngle);
			var aziDenom =
				Math.Tan(declination) * Math.Cos(latitude * Mathf.Deg2Rad) -
				Math.Sin(latitude * Mathf.Deg2Rad) * Math.Cos(hourAngle);

			var azimuth = Math.Atan(aziNom / aziDenom);

			if (aziDenom < 0) // In 2nd or 3rd quadrant  
			{
				azimuth += Math.PI;
			}
			else if (aziNom < 0) // In 4th quadrant  
			{
				azimuth += 2 * Math.PI;
			}

			return new SunCoords(altitude, azimuth);
		}

		private class SunCoords
		{
			public SunCoords(double altitude, double azimuth)
			{
				Altitude = altitude;
				Azimuth = azimuth;
			}

			public double Altitude { get; }
			public double Azimuth { get; }
		}

		private static double CorrectAngle(double angleInRadians)
		{
			if (angleInRadians < 0)
			{
				return 2 * Math.PI - Math.Abs(angleInRadians) % (2 * Math.PI);
			}
			if (angleInRadians > 2 * Math.PI)
			{
				return angleInRadians % (2 * Math.PI);
			}
			return angleInRadians;
		}

		#endregion
	}
}