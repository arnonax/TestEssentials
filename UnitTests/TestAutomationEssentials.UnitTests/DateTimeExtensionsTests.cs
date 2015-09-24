using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class DateTimeExtensionsTests
	{
		[TestMethod]
		public void _2MinutesAre120000Milliseconds()
		{
			Assert.AreEqual(120000, 2.MinutesAsMilliseconds());
		}

		[TestMethod]
		public void _2SecondsAre2000Milliseconds()
		{
			Assert.AreEqual(2000, 2.SecondsAsMilliseconds());
		}

		[TestMethod]
		public void MillisecondsAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromMilliseconds(1234), 1234.Milliseconds());
		}

		[TestMethod]
		public void SecondsAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromSeconds(1234), 1234.Seconds());
		}

		[TestMethod]
		public void MinutesAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromMinutes(1234), 1234.Minutes());
		}

		[TestMethod]
		public void HoursAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromHours(1234), 1234.Hours());
		}

		[TestMethod]
		public void TimeSpanToSpokenString()
		{
			Assert.AreEqual("0 seconds", TimeSpan.Zero.ToSpokenString());
			Assert.AreEqual("0.001 seconds", 1.Milliseconds().ToSpokenString());
			Assert.AreEqual("1 second", 1.Seconds().ToSpokenString());
			Assert.AreEqual("1.001 seconds", (1.Seconds() + 1.Milliseconds()).ToSpokenString());
			Assert.AreEqual("2 seconds", 2.Seconds().ToSpokenString());
			Assert.AreEqual("1 minute", 1.Minutes().ToSpokenString());
			Assert.AreEqual("1:01 minutes", (1.Minutes() + 1.Seconds()).ToSpokenString());
			Assert.AreEqual("1:01 minutes", (1.Minutes() + 1.Seconds() + 1.Milliseconds()).ToSpokenString());
			Assert.AreEqual("2 minutes", 2.Minutes().ToSpokenString());
			Assert.AreEqual("1 hour", 1.Hours().ToSpokenString());
			Assert.AreEqual("1:01 hours", (1.Hours() + 1.Minutes() + 1.Seconds() + 1.Milliseconds()).ToSpokenString());
		}

		[TestMethod]
		public void AbosulteReturnsTheSameTimeSpanIfPositive()
		{
			var original = 2.Seconds();
			Assert.AreEqual(original, original.Absolute(), "Absolute of positive should be positive");
		}

		[TestMethod]
		public void AbsoluteReturnsThePositiveTimeSpanIfGivenNegative()
		{
			var original = -2.Seconds();
			Assert.AreEqual(2.Seconds(), original.Absolute(), "Absolute of negative should be positive");
		}
	}
}
