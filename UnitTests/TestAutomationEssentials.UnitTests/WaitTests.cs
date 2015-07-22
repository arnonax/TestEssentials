using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage]
	public class WaitTests
	{
		[TestMethod]
		public void WaitUntilThrowsExceptionWhenConditionIsNull()
		{
			Action action = () => Wait.Until(null, 1.Seconds());
			TestUtils.ExpectException<ArgumentNullException>(action);
		}

		[TestMethod]
		public void WaitUntilWaitsUntilTheConditionIsMet()
		{
			var values = new[] {false, false, true, false};
			var i = 0;
			Func<bool> func = () =>
			{
				var result = values[i];
				i++;
				return result;
			};
			
			Wait.Until(() => func(), 3.Seconds());

			Assert.AreEqual(3, i, "Wait.Until continued to evaluate the expression after it already returned true");
		}

		[TestMethod]
		public void WaitUntilThrowsTimeoutExceptionIfConditionIsNotMetDuringTheDesignatedPeriod()
		{
			var timeout = 200.Milliseconds();

			var startTime = DateTime.Now;
			Action action = () => Wait.Until(() => false, timeout);
			TestUtils.ExpectException<TimeoutException>(action);
			var endTime = DateTime.Now;

			Assert.IsTrue(endTime - startTime >= timeout, "Wait.Until didn't wait enough (startTime={0}, endTime={1})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.1), "Wait.Until waited for too long (startTime={0}, endTime={1})", startTime, endTime);
		}

		[TestMethod]
		public void WaitWhileThrowsExceptionWhenConditionIsNull()
		{
			Action action = () => Wait.While(null, 1.Seconds());
			TestUtils.ExpectException<ArgumentNullException>(action);
		}

		[TestMethod]
		public void WaitWhileWaitsUntilTheConditionIsNotMet()
		{
			var values = new[] { true, true, false, true};
			var i = 0;
			Func<bool> func = () =>
			{
				var result = values[i];
				i++;
				return result;
			};

			Wait.While(() => func(), 3.Seconds());

			Assert.AreEqual(3, i, "Wait.While continued to evaluate the expression after it already returned false");
		}

		[TestMethod]
		public void WaitWhileThrowsTimeoutExceptionIfConditionIsMetDuringTheEntireDesignatedPeriod()
		{
			var timeout = 100.Milliseconds();

			var startTime = DateTime.Now;
			Action action = () => Wait.While(() => true, timeout);
			TestUtils.ExpectException<TimeoutException>(action);
			var endTime = DateTime.Now;

			Assert.IsTrue(endTime - startTime >= timeout, "Wait.While didn't wait enough (startTime={0}, endTime={1})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.1), "Wait.While waited for too long (startTime={0}, endTime={1})", startTime, endTime);
		}

		[TestMethod]
		public void WaitIfWaitsUntilTheConditionIsNotMet()
		{
			var values = new[] { true, true, false, true };
			var i = 0;
			Func<bool> func = () =>
			{
				var result = values[i];
				i++;
				return result;
			};

			var returnValue = Wait.If(() => func(), 3.Seconds());
			Assert.IsTrue(returnValue, "Wait.If should return true if the condition is met");

			Assert.AreEqual(3, i, "Wait.If continued to evaluate the expression after it already returned false");
		}

		[TestMethod]
		public void WaitIfReturnsFalseIfConditionIsMetDuringTheEntireDesignatedPeriod()
		{
			var timeout = 100.Milliseconds();

			var startTime = DateTime.Now;
			var result = Wait.If(() => true, timeout);
			Assert.IsFalse(result, "Wait.If had to return false if the condition is not met");
			var endTime = DateTime.Now;

			Assert.IsTrue(endTime - startTime >= timeout, "Wait.If didn't wait enough (startTime={0}, endTime={1})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.1), "Wait.If waited for too long (startTime={0}, endTime={1})", startTime, endTime);
		}

		public void WaitIfNotWaitsUntilTheConditionIsMet()
		{
			var values = new[] { false, false, true, false };
			var i = 0;
			Func<bool> func = () =>
			{
				var result = values[i];
				i++;
				return result;
			};

			var returnValue = Wait.IfNot(() => func(), 3.Seconds());
			Assert.IsTrue(returnValue, "Wait.IfNot should return true if the condition is met");

			Assert.AreEqual(3, i, "Wait.Until continued to evaluate the expression after it already returned true");
		}

		[TestMethod]
		public void WaitIfNotReturnsFalseIfConditionIsNotMetDuringTheDesignatedPeriod()
		{
			var timeout = 100.Milliseconds();

			var startTime = DateTime.Now;
			var returnValue = Wait.IfNot(() => false, timeout);
			Assert.IsFalse(returnValue, "Wait.IfNot should return false if the condition never met");
			var endTime = DateTime.Now;

			Assert.IsTrue(endTime - startTime >= timeout, "Wait.IfNot didn't wait enough (startTime={0}, endTime={1})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.1), "Wait.IfNot waited for too long (startTime={0}, endTime={1})", startTime, endTime);
		}
	}
}
