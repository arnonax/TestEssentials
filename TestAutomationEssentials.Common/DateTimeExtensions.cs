using System;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Provides extension methods to DateTime, TimeSpan and other methods related to these types
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Returns the absolute timespan that corresponds to the given one
		/// </summary>
		/// <param name="timeSpan">The original timespan</param>
		/// <returns>If the given time span is positive, the method returns the same value; otherwise it returns the opposite (i.e. positive) time span that has the same absolute value as the given one</returns>
		public static TimeSpan Absolute(this TimeSpan timeSpan)
		{
			return timeSpan < TimeSpan.Zero ? timeSpan.Negate() : timeSpan;
		}

		/// <summary>
		/// Calculates the number of milliseconds in the specified number of minutes
		/// </summary>
		/// <param name="minutes">The number of minutes to convert to milliseconds</param>
		/// <returns>The result of the calculation</returns>
		public static int MinutesAsMilliseconds(this int minutes)
		{
			return (minutes*60).SecondsAsMilliseconds();
		}

		/// <summary>
		/// Calculates the number of milliseconds in the specified number of seconds
		/// </summary>
		/// <param name="seconds">The number of seconds to convert to milliseconds</param>
		/// <returns>The result of the calculation</returns>
		public static int SecondsAsMilliseconds(this int seconds)
		{
			return seconds*1000;
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of milliseconds
		/// </summary>
		/// <param name="milliseconds">The number of milliseconds</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of milliseconds</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Millisconds();
		/// </example>
		public static TimeSpan Milliseconds(this int milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of seconds
		/// </summary>
		/// <param name="seconds">The number of seconds</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of seconds</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Seconds();
		/// </example>
		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of minutes
		/// </summary>
		/// <param name="minutes">The number of minutes</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of minutes</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Minutes();
		/// </example>
		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of hours
		/// </summary>
		/// <param name="hours">The number of hours</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of hours</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Hours();
		/// </example>
		public static TimeSpan Hours(this int hours)
		{
			return TimeSpan.FromHours(hours);
		}

		/// <summary>
		/// Multiplies the specified TimeSpan by the specified multiplier
		/// </summary>
		/// <param name="multiplicand">The <b>TimeSpan</b> struct to multiply</param>
		/// <param name="multiplier">The multiplier</param>
		/// <returns>A <b>TimeSpan</b> representing the calculated multiplication</returns>
		/// <example>
		/// Assert.AreEquals(6.Minutes(), 2.Minutes().MultiplyBy(3));
		/// </example>
		public static TimeSpan MutliplyBy(this TimeSpan multiplicand, double multiplier)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
		}

		/// <summary>
		/// Returns a string that represent the most significant portions of the specified <b>TimeSpan</b>
		/// </summary>
		/// <param name="timeSpan">The <b>TimeSpan</b> to convert to string</param>
		/// <returns>The string that contains the significant portions of the TimeSpan</returns>
		public static string ToSpokenString(this TimeSpan timeSpan)
		{
			if (timeSpan == 1.Minutes())
				return "1 minute";

			if (timeSpan == 1.Hours())
				return "1 hour";

			if (timeSpan > 1.Hours())
				return Format("hours", timeSpan.Hours, timeSpan.Minutes);

			if (timeSpan > 1.Minutes())
				return Format("minutes", timeSpan.Minutes, timeSpan.Seconds);

			var seconds = timeSpan.TotalSeconds;
			
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return seconds == 1.0 ? "1 second" : seconds + " seconds";
		}

		private static string Format(string uom, int mainValue, int subValue)
		{
			if (subValue == 0)
				return mainValue + " " + uom;

			return String.Format("{0}:{1:00} " + uom, mainValue, subValue);
		}
	}
}
