// TimeSpan.cs - dEngine
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
using System.IO;


namespace dEngine
{
	/// <summary>
	/// A type which represents a time span.
	/// </summary>
	public struct TimeSpan : IDataType, IEquatable<TimeSpan>
	{
		internal System.TimeSpan _timeSpan;

		/// <summary>
		/// The number of ticks.
		/// </summary>
		[InstMember(1)]
		public long ticks
		{
			get { return _timeSpan.Ticks; }
			internal set { _timeSpan = new System.TimeSpan(value); }
		}

		/// <summary>
		/// Gets the number of days.
		/// </summary>
		public int days => _timeSpan.Days;

		/// <summary>
		/// Gets the number of hours.
		/// </summary>
		public int hours => _timeSpan.Hours;

		/// <summary>
		/// Gets the number of minutes.
		/// </summary>
		public int minutes => _timeSpan.Minutes;

		/// <summary>
		/// Gets the number of seconds.
		/// </summary>
		public int seconds => _timeSpan.Seconds;

		/// <summary>
		/// Gets the number of milliseconds.
		/// </summary>
		public int milliseconds => _timeSpan.Milliseconds;

		/// <summary>
		/// Gets the total number of days.
		/// </summary>
		public double totalDays => _timeSpan.TotalDays;

		/// <summary>
		/// Gets the total number of hours.
		/// </summary>
		public double totalHours => _timeSpan.TotalHours;

		/// <summary>
		/// Gets the total number of minutes.
		/// </summary>
		public double totalMinutes => _timeSpan.TotalMinutes;

		/// <summary>
		/// Gets the total number of seconds.
		/// </summary>
		public double totalSeconds => _timeSpan.TotalSeconds;

		/// <summary>
		/// Gets the total number of milliseconds.
		/// </summary>
		public double totalMilliseconds => _timeSpan.TotalMilliseconds;

		/// <summary>
		/// A zeroed timespan.
		/// </summary>
		public static TimeSpan Zero = new TimeSpan(0L);

		/// <summary />
		public TimeSpan(long ticks)
		{
			_timeSpan = new System.TimeSpan(ticks);
		}

		/// <summary />
		public TimeSpan(string str)
		{
			_timeSpan = System.DateTime.Parse(str).TimeOfDay;
		}

		/// <summary />
		public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds = 0)
		{
			_timeSpan = new System.TimeSpan(days, hours, minutes, seconds, milliseconds);
		}

		/// <summary />
		public TimeSpan(int hours, int minutes, int seconds)
		{
			_timeSpan = new System.TimeSpan(hours, minutes, seconds);
		}

		/// <summary />
		public static TimeSpan @new(long ticks)
		{
			return new TimeSpan(ticks);
		}

		/// <summary />
		public static TimeSpan @new(string str)
		{
			return new TimeSpan(str);
		}

		/// <summary />
		public static TimeSpan @new(int days, int hours, int minutes, int seconds, int milliseconds = 0)
		{
			return new TimeSpan(days, hours, minutes, seconds, milliseconds);
		}

		/// <summary />
		public static TimeSpan @new(int hours, int minutes, int seconds)
		{
			return new TimeSpan(hours, minutes, seconds);
		}

		/// <summary>
		/// Returns an absolute timespan.
		/// </summary>
		public TimeSpan duration()
		{
			return (TimeSpan)_timeSpan.Duration();
		}

		/// <summary />
		public static explicit operator TimeSpan(System.TimeSpan timeSpan)
		{
			return new TimeSpan {_timeSpan = timeSpan};
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _timeSpan.ToString();
		}

		/// <summary>
		/// Adds two timespans.
		/// </summary>
		public static TimeSpan operator +(TimeSpan left, TimeSpan right)
		{
			return (TimeSpan)(left._timeSpan + right._timeSpan);
		}

		/// <summary>
		/// Subtracts two timespans.
		/// </summary>
		public static TimeSpan operator -(TimeSpan left, TimeSpan right)
		{
			return (TimeSpan)(left._timeSpan - right._timeSpan);
		}

		/// <summary>
		/// Scales the <see cref="TimeSpan" />.
		/// </summary>
		public static TimeSpan operator *(TimeSpan left, float scale)
		{
			return new TimeSpan(left.ticks * (long)scale);
		}

		/// <summary>
		/// Determines if the left timespan is greater than the right timespan.
		/// </summary>
		public static bool operator >(TimeSpan left, TimeSpan right)
		{
			return left._timeSpan > right._timeSpan;
		}

		/// <summary>
		/// Determines if the left timespan is greater than or equal to the right timespan.
		/// </summary>
		public static bool operator >=(TimeSpan left, TimeSpan right)
		{
			return left._timeSpan > right._timeSpan;
		}

		/// <summary>
		/// Determines if the left timespan is lesser than the right timespan.
		/// </summary>
		public static bool operator <(TimeSpan left, TimeSpan right)
		{
			return left._timeSpan < right._timeSpan;
		}

		/// <summary>
		/// Determines if the left timespan is lesser than or equal to the right timespan.
		/// </summary>
		public static bool operator <=(TimeSpan left, TimeSpan right)
		{
			return left._timeSpan <= right._timeSpan;
		}

		/// <summary>
		/// Determines if the left timespan is lesser than or equal to the right timespan.
		/// </summary>
		public static bool operator ==(TimeSpan left, TimeSpan right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines if the left timespan is lesser than or equal to the right timespan.
		/// </summary>
		public static bool operator !=(TimeSpan left, TimeSpan right)
		{
			return !left.Equals(right);
		}

		/// <summary />
		public bool Equals(TimeSpan other)
		{
			return _timeSpan.Equals(other._timeSpan);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TimeSpan && Equals((TimeSpan)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _timeSpan.GetHashCode();
		}

		/// <summary>
		/// Returns a timespan from the given days.
		/// </summary>
		public static TimeSpan FromDays(double days)
		{
			return (TimeSpan)System.TimeSpan.FromDays(days);
		}

		/// <summary>
		/// Returns a timespan from the given hours.
		/// </summary>
		public static TimeSpan FromHours(double hours)
		{
			return (TimeSpan)System.TimeSpan.FromHours(hours);
		}

		/// <summary>
		/// Returns a timespan from the given minutes.
		/// </summary>
		public static TimeSpan FromMinutes(double minutes)
		{
			return (TimeSpan)System.TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Returns a timespan from the given seconds.
		/// </summary>
		public static TimeSpan FromSeconds(double seconds)
		{
			return (TimeSpan)System.TimeSpan.FromSeconds(seconds);
		}

		/// <summary>
		/// Returns a timespan from the given milliseconds.
		/// </summary>
		public static TimeSpan FromMilliseconds(double milliseconds)
		{
			return (TimeSpan)System.TimeSpan.FromMilliseconds(milliseconds);
		}

		/// <summary>
		/// Returns a <see cref="System.TimeSpan" />.
		/// </summary>
		public static explicit operator System.TimeSpan(TimeSpan time)
		{
			return time._timeSpan;
		}

        /// <summary/>
        public void Load(BinaryReader reader)
	    {
            _timeSpan = new System.TimeSpan(reader.ReadInt64());
	    }

        /// <summary/>
	    public void Save(BinaryWriter writer)
	    {
            writer.Write(_timeSpan.Ticks);
	    }
	}
}