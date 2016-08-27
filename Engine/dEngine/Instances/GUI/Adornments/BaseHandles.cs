// BaseHandles.cs - dEngine
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
using System.Collections.Concurrent;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;
using dEngine.Utility;

namespace dEngine.Instances
{
	/// <summary>
	/// An abstract class for drawing handles around the <see cref="PVAdornment.Adornee" />.
	/// </summary>
	[TypeId(164)]
	public abstract class BaseHandles : PVAdornment
	{
		/*
		private PVInstance _lastAdornee;

		private void OnHandlesChanged(string propertyName)
		{
			if (propertyName != nameof(Adornee))
				return;

			if (_lastAdornee != null)
			{
				_lastAdornee.Changed.Event -= OnPropertyOfAdorneeChanged;
			}

			if (Adornee != null)
			{
				Adornee.Changed.Event += OnPropertyOfAdorneeChanged;
			}

			_lastAdornee = Adornee;
			UpdateHandle();
		}

		/// <summary>
		/// Called when a property of the current Adornee is changed.
		/// </summary>
		private void OnPropertyOfAdorneeChanged(string propertyName)
		{
			if (propertyName == nameof(Adornee.PVCFrame) || propertyName == nameof(Adornee.PVSize))
			{
				UpdateHandle();
			}
		}
		*/

		/// <summary>
		/// Determines whether any handles are blocking the mouse.
		/// </summary>
		internal static ConcurrentDictionary<BaseHandles, byte> HeldHandles { get; set; } =
			new ConcurrentDictionary<BaseHandles, byte>();

		internal static bool IsMouseOverHandle => HeldHandles.Count > 0;

		protected override void OnAncestryChanged(Instance child, Instance parent)
		{
			base.OnAncestryChanged(child, parent);

			if (child == this)
			{
				ChangeRenderObject();
			}
		}


        internal void GetDistanceToNormals(float rel, float mul, ref CFrame adorneeCFrame, ref Vector3 adorneeSize,
            ref float[] distances)
        {
            var adorneePos = adorneeCFrame.p;
            var camPos = ((ICameraUser)this).Camera.CFrame.p;

            for (int i = 0; i < 6; i++)
            {
                float dist;

                var id = (NormalId)i;
                switch (id)
                {
                    case NormalId.Left:
                        dist = (camPos - (adorneePos + adorneeCFrame.left * adorneeSize.X / 2)).mag2;
                        break;
                    case NormalId.Right:
                        dist = (camPos - (adorneePos + adorneeCFrame.right * adorneeSize.X / 2)).mag2;
                        break;
                    case NormalId.Top:
                        dist = (camPos - (adorneePos + adorneeCFrame.up * adorneeSize.Y / 2)).mag2;
                        break;
                    case NormalId.Bottom:
                        dist = (camPos - (adorneePos + adorneeCFrame.down * adorneeSize.Y / 2)).mag2;
                        break;
                    case NormalId.Front:
                        dist = (camPos - (adorneePos + adorneeCFrame.forward * adorneeSize.Z / 2)).mag2;
                        break;
                    case NormalId.Back:
                        dist = (camPos - (adorneePos + adorneeCFrame.backward * adorneeSize.Z / 2)).mag2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                distances[i] = Math.Max(1, Mathf.Sqrt(dist) / 10 * 0.6f);
            }
        }

        protected virtual void ChangeRenderObject()
		{
		}
	}
}