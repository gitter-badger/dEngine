// DateTime.cs - dEngine
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
	/// A structure for holding a date/time.
	/// </summary>
	public struct DateTime : IDataType, IEquatable<DateTime>
	{
		private System.DateTime _dateTime;

        /// <summary>
        /// The number of ticks in the date.
        /// </summary>
	    public long ticks => _dateTime.Ticks;

	    /// <summary>
		/// Creates a date from miliseconds.
		/// </summary>
		public DateTime(double miliseconds)
		{
			_dateTime = new System.DateTime((long)miliseconds * 10000);
		}

		/// <summary>
		/// Creates a date from miliseconds.
		/// </summary>
		public DateTime(string dateString)
		{
			_dateTime = System.DateTime.Parse(dateString);
		}

		/// <summary>
		/// Creates a date from the given date/time paramaters.
		/// </summary>
		public DateTime(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds = 0)
		{
			_dateTime = new System.DateTime(year, month, day, hours, minutes, seconds, milliseconds);
		}

		/// <summary>
		/// Creates a date from miliseconds.
		/// </summary>
		public static DateTime @new(double miliseconds)
		{
			return new DateTime(miliseconds);
		}

		/// <summary>
		/// Creates a date from miliseconds.
		/// </summary>
		public static DateTime @new(string dateString)
		{
			return new DateTime(dateString);
		}

		/// <summary>
		/// Creates a date from the given date/time paramaters.
		/// </summary>
		public static DateTime @new(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds = 0)
		{
			return new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
		}

		private DateTime(System.DateTime dateTime)
		{
			_dateTime = dateTime;
		}

		/// <summary>
		/// Gets the day of the month.
		/// </summary>
		public int day => _dateTime.Day;

		/// <summary>
		/// Gets the day of the week.
		/// </summary>
		public int dayOfWeek => (int)_dateTime.DayOfWeek;

		/// <summary>
		/// Gets the day of the year.
		/// </summary>
		public int dayOfYear => _dateTime.DayOfYear;

		/// <summary>
		/// Gets the year.
		/// </summary>
		public int year => _dateTime.Year;

		/// <summary>
		/// Gets the millisecond.
		/// </summary>
		public int millisecond => _dateTime.Millisecond;

		/// <summary>
		/// Gets the minute.
		/// </summary>
		public int minute => _dateTime.Minute;

		/// <summary>
		/// Gets the month.
		/// </summary>
		public int month => _dateTime.Month;

		/// <summary>
		/// Gets the second.
		/// </summary>
		public int second => _dateTime.Second;

		/// <summary>
		/// Gets the time as a <see cref="TimeSpan" />.
		/// </summary>
		public TimeSpan time => (TimeSpan)_dateTime.TimeOfDay;

		/// <summary>
		/// Determines whether the date is in the UTC or local timezone.
		/// </summary>
		public bool isUtc => _dateTime.Kind == DateTimeKind.Utc;

		/// <summary>
		/// Gets the date converted to UTC-relative.
		/// </summary>
		public DateTime utc => (DateTime)_dateTime.ToUniversalTime();

		/// <summary>
		/// Returns the UTC offset of the local timezone.
		/// </summary>
		public int getTimezoneOffset()
		{
			return TimeZone.CurrentTimeZone.GetUtcOffset(_dateTime).Minutes;
		}

		/// <summary>
		/// Returns a formatted string representation of the DateTime.
		/// </summary>
		public string format(string formatString)
		{
			return _dateTime.ToString(formatString);
		}

		/// <summary>
		/// Returns the current time with added years.
		/// </summary>
		public DateTime addYears(int years)
		{
			return (DateTime)_dateTime.AddYears(years);
		}

		/// <summary>
		/// Returns the current time with added months.
		/// </summary>
		public DateTime addMonths(int months)
		{
			return (DateTime)_dateTime.AddMonths(months);
		}

		/// <summary>
		/// Returns the current time with added days.
		/// </summary>
		public DateTime addDays(double days)
		{
			return (DateTime)_dateTime.AddDays(days);
		}

		/// <summary>
		/// Returns the current time with added hours.
		/// </summary>
		public DateTime addHours(double hours)
		{
			return (DateTime)_dateTime.AddHours(hours);
		}

		/// <summary>
		/// Returns the current time with added minutes.
		/// </summary>
		public DateTime addMinutes(double hours)
		{
			return (DateTime)_dateTime.AddMinutes(hours);
		}

		/// <summary>
		/// Returns the current time with added seconds.
		/// </summary>
		public DateTime addSeconds(double seconds)
		{
			return (DateTime)_dateTime.AddSeconds(seconds);
		}

		/// <summary>
		/// Returns the current time with added milliseconds.
		/// </summary>
		public DateTime addMilliseconds(double milliseconds)
		{
			return (DateTime)_dateTime.AddMilliseconds(milliseconds);
		}

		/// <summary>
		/// Determines if the time is within the daylight saving time for the local timezone.
		/// </summary>
		public bool isDaylightSavingTime()
		{
			return _dateTime.IsDaylightSavingTime();
		}

		/// <summary>
		/// Returns the current date.
		/// </summary>
		public static DateTime Today()
		{
			return (DateTime)System.DateTime.Today;
		}

		/// <summary>
		/// Returns the current date and time.
		/// </summary>
		public static DateTime Now()
		{
			return (DateTime)System.DateTime.Now;
		}

		/// <summary>
		/// Returns the current date and time in UTC.
		/// </summary>
		public static DateTime UtcNow()
		{
			return (DateTime)System.DateTime.UtcNow;
        }

        /// <summary>
        /// Returns the Unix epoch date in UTC.
        /// </summary>
        public static DateTime UnixEpoch()
        {
            return new DateTime {_dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) };
        }

        /// <summary>
        /// Returns the number of seconds since the Unix epoch.
        /// </summary>
        /// <returns></returns>
        public static double UnixTimestamp()
        {
            return (UtcNow() - UnixEpoch()).totalSeconds;
        }

        /// <summary />
        public bool Equals(DateTime other)
		{
			return _dateTime.Equals(other._dateTime);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is DateTime && Equals((DateTime)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _dateTime.GetHashCode();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _dateTime.ToString("yy-mm-dd hh:mm:ss");
		}

		/// <summary />
		public static explicit operator DateTime(System.DateTime dateTime)
		{
			return new DateTime(dateTime);
		}

		/// <summary>
		/// Subtracts two dates.
		/// </summary>
		public static TimeSpan operator -(DateTime left, DateTime right)
		{
			return (TimeSpan)(left._dateTime - right._dateTime);
		}

		/// <summary>
		/// Subtracts a <see cref="TimeSpan" /> from the date.
		/// </summary>
		public static DateTime operator -(DateTime left, TimeSpan right)
		{
			return (DateTime)(left._dateTime - right._timeSpan);
		}

		/// <summary>
		/// Adds a <see cref="TimeSpan" /> to the date.
		/// </summary>
		public static DateTime operator +(DateTime left, TimeSpan right)
		{
			return (DateTime)(left._dateTime + right._timeSpan);
		}

		/// <summary>
		/// Determines if the left date is greater than the right date.
		/// </summary>
		public static bool operator >(DateTime left, DateTime right)
		{
			return left._dateTime > right._dateTime;
		}

		/// <summary>
		/// Determines if the left date is greater than or to the right date.
		/// </summary>
		public static bool operator >=(DateTime left, DateTime right)
		{
			return left._dateTime > right._dateTime;
		}

		/// <summary>
		/// Determines if the left date is lesser than the right date.
		/// </summary>
		public static bool operator <(DateTime left, DateTime right)
		{
			return left._dateTime > right._dateTime;
		}

		/// <summary>
		/// Determines if the left date is lesser than or equal to the right date.
		/// </summary>
		public static bool operator <=(DateTime left, DateTime right)
		{
			return left._dateTime > right._dateTime;
		}

		/// <summary>
		/// Determines if the left timespan is lesser than or equal to the right timespan.
		/// </summary>
		public static bool operator ==(DateTime left, DateTime right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines if the left timespan is lesser than or equal to the right timespan.
		/// </summary>
		public static bool operator !=(DateTime left, DateTime right)
		{
			return !left.Equals(right);
		}

        /// <summary/>
	    public void Load(BinaryReader reader)
	    {
            _dateTime = new System.DateTime(reader.ReadInt64());
	    }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(_dateTime.Ticks);
        }
	}
}