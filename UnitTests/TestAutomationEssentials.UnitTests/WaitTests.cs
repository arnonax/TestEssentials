using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;
using System.Collections.Generic;
using System.Linq;

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
		public void WaitUntilThrowsArgumentOutOfRangeExceptionIfRangeIsNegative()
		{
			Action action = () => Wait.Until(() => false, -3.Seconds());
			var ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(action);
			Assert.AreEqual("timeout", ex.ParamName);
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
		public void WaitUntilThrowsTimeoutExceptionWithFormatterMessage()
		{
			var timeout = 200.Milliseconds();

			Action action = () => Wait.Until(() => false, timeout);
			const string messageFormat = "This is a message with parameters: '{0}', '{1}'";
			const string arg1 = "Something";
			const double arg2 = 3.5;
			var ex = TestUtils.ExpectException<TimeoutException>(action, messageFormat, arg1, arg2);

			var expectedMessage = string.Format(messageFormat, arg1, arg2);
			Assert.AreNotEqual(expectedMessage, ex.Message, "Wait.Until should throw the exception with the specified formatted message");
		}

		[TestMethod]
		public void WaitUntilValidatesItsArguments()
		{
			{
				Action action = () => Wait.Until(null, 5.Seconds(), "dummy message");
				var ex = TestUtils.ExpectException<ArgumentNullException>(action);
				Assert.AreEqual("condition", ex.ParamName);
			}
			{
				Action action = () => Wait.Until(() => true, -5.Seconds(), "timeout can't be negative!");
				var ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(action);
				Assert.AreEqual("timeout", ex.ParamName);
			}
			{
				Action action = () => Wait.Until(() => true, 5.Seconds(), null);
				var ex = TestUtils.ExpectException<ArgumentNullException>(action);
				Assert.AreEqual("timeoutMessage", ex.ParamName);
			}
			{
				Action action = () => Wait.Until(() => true, 5.Seconds(), "dummy timeout message", null);
				var ex = TestUtils.ExpectException<ArgumentNullException>(action);
				Assert.AreEqual("args", ex.ParamName);
			}
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void WaitUntilOverloadWithTwoExpressionsValidatesItsArguments()
		{
			Expression<Func<int>> expr = null;
			Expression<Func<int, bool>> conditionExpr = i => true;
			var timeout = 2.Seconds();

			Action methodUnderTest = () => Wait.Until(expr, conditionExpr, timeout);
		
			ArgumentException ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("getResultExpr", ex.ParamName);
			expr = () => 1;
			
			conditionExpr = null;
			ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("conditionExpr", ex.ParamName);
			conditionExpr = i => false;
			
			timeout = -1.Seconds();
			ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(methodUnderTest);
			Assert.AreEqual("timeout", ex.ParamName);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void WaitUntilOverloadWithTwoDelegatesValidatesItsArguments()
		{
			Func<int> getResult = () => 1;
			Func<int, bool> condition = i => true;
			var timeout = 1.Seconds();
			var timeoutMessage = "Dummy {0}";
			object[] args = {"Arg"};
			
			Action methodUnderTest = () => Wait.Until(getResult, condition, timeout, timeoutMessage, args);

			getResult = null;
			ArgumentException ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("getResult", ex.ParamName);
			getResult = () => 1;

			condition = null;
			ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("condition", ex.ParamName);
			condition = i => true;

			timeout = -1.Seconds();
			ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(methodUnderTest);
			Assert.AreEqual("timeout", ex.ParamName);
			timeout = 1.Seconds();

			timeoutMessage = null;
			ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("timeoutMessage", ex.ParamName);
			timeoutMessage = "Dummy {0}";

			args = null;
			ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("args", ex.ParamName);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void WaitIfValidatesItsArguments()
		{
			Func<bool> condition = () => false;
			var period = 1.Seconds();
			Action methodUnderTest = () => Wait.If(condition, period);

			condition = null;
			ArgumentException ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("condition", ex.ParamName);
			condition = () => false;

			period = -1.Seconds();
			ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(methodUnderTest);
			Assert.AreEqual("period", ex.ParamName);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void WaitIfNotValidatesItsArguments()
		{
			Func<bool> condition = () => true;
			var period = 1.Seconds();
			Action methodUnderTest = () => Wait.IfNot(condition, period);

			condition = null;
			ArgumentException ex = TestUtils.ExpectException<ArgumentNullException>(methodUnderTest);
			Assert.AreEqual("condition", ex.ParamName);
			condition = () => true;

			period = -1.Seconds();
			ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(methodUnderTest);
			Assert.AreEqual("period", ex.ParamName);
		}

		[TestMethod]
		public void WaitWhileThrowsExceptionWhenConditionIsNull()
		{
			Action action = () => Wait.While(null, 1.Seconds());
			TestUtils.ExpectException<ArgumentNullException>(action);
		}

		[TestMethod]
		public void WaitWhileThrowsArgumentOutOfRangeExceptionIfRangeIsNegative()
		{
			Action action = () => Wait.While(() => true, -3.Seconds());
			var ex = TestUtils.ExpectException<ArgumentOutOfRangeException>(action);
			Assert.AreEqual("timeout", ex.ParamName);
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
			var timeout = 200.Milliseconds();

			var startTime = DateTime.Now;
			var result = Wait.If(() => true, timeout);
			Assert.IsFalse(result, "Wait.If had to return false if the condition is not met");
			var endTime = DateTime.Now;

			Assert.IsTrue(endTime - startTime >= timeout, "Wait.If didn't wait enough (startTime={0:O}, endTime={1:O})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.2), "Wait.If waited for too long (startTime={0:O}, endTime={1:O})", startTime, endTime);
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
		    var endTime = DateTime.Now;

            Assert.IsFalse(returnValue, "Wait.IfNot should return false if the condition never met");
			
			Assert.IsTrue(endTime - startTime >= timeout, "Wait.IfNot didn't wait enough (startTime={0}, endTime={1})", startTime, endTime);
			Assert.IsTrue(endTime - startTime <= timeout.MutliplyBy(1.1), "Wait.IfNot waited for too long (startTime={0:O}, endTime={1:O}, delta={2}ms)", startTime, endTime, (endTime-startTime).TotalMilliseconds);
		}

		[TestMethod]
		public void WaitUntilReturnsTheResultOfTheFirstFunctionIfTheSecondFunctionReturnsTrue()
		{
			var enumerator = new [] {1, 3, 4, 5}.GetEnumerator();
			Func<int> getNextItem = () =>
			{
				enumerator.MoveNext();
				return (int)enumerator.Current;
			};
			var result = Wait.Until(() => getNextItem(), item => item%2 == 0, 100.Milliseconds());
			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void WaitUntilThrowsTimeoutExceptionIfTheConditionIsNotMet()
		{
			Func<int> foo = () => 1;
			var ex = TestUtils.ExpectException<TimeoutException>(() => Wait.Until(() => foo(), item => item != 1, 100.Milliseconds()));
			StringAssert.Contains(ex.Message, "foo");

			ex = TestUtils.ExpectException<TimeoutException>(() => Wait.Until(() => foo(), item => item != 1, 100.Milliseconds(), "DummyMessage{0}", 3));
			StringAssert.Contains(ex.Message, "DummyMessage3");
		}

		[TestMethod]
		public void WaitUntilLogsStartCompletion()
		{
			var lines = new List<string>();
			Logger.Initialize(line => lines.Add(line));

			var counter = 3;
			Func<int> decrementCounter = () => counter--;
			Wait.Until(() => decrementCounter() == 0, 100.Milliseconds());

            Assert.AreEqual(2, lines.Count, "2 lines are expected: one for start wait, and one for end");
			StringAssert.Contains(lines.First(), "Waiting for ");
		    StringAssert.Contains(lines.Last(), "Done waiting for ");
		}
	}
}
