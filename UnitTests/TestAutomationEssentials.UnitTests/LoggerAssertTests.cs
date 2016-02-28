using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class LoggerAssertTests
	{
		private readonly List<string> _lines = new List<string>();

		public LoggerAssertTests()
		{
			Logger.Initialize(str => _lines.Add(str));
		}

		[TestMethod]
		public void AreEqualWritesTheMessageOnSuccess()
		{		
			LoggerAssert.AreEqual(4, 4, "Dummy message {0}", "arg");

			Assert.AreEqual(1, _lines.Count);
			StringAssert.Contains(_lines[0], "Dummy message arg", "formatted message should appear in logged message");
		}

		[TestMethod]
		public void AreEqualThrowsAssertFailedExceptionAndLogsTheMessageOnFailure()
		{
			Action failingAssertion = () => LoggerAssert.AreEqual(3, 4, "Dummy message {0}", "arg");
			var ex = TestUtils.ExpectException<AssertFailedException>(failingAssertion);
			StringAssert.Contains(ex.Message, "Dummy message arg");

			Assert.AreEqual(1, _lines.Count);
			StringAssert.Contains(_lines[0], "Dummy message arg", "formatted message should appear in logged message");
			StringAssert.Contains(_lines[0], "3", "expected value should appear in logged message");
			StringAssert.Contains(_lines[0], "4", "actual value should appear in logged message");
		}

		[TestMethod]
		public void AreEqualsValidatesThatMessageAndArgsAreNotNull()
		{
			{
				var ex = TestUtils.ExpectException<ArgumentNullException>(() => LoggerAssert.AreEqual(3, 4, null, "a"));
				Assert.AreEqual("expectationMessage", ex.ParamName);
			}

			{
				var ex = TestUtils.ExpectException<ArgumentNullException>(() => LoggerAssert.AreEqual(3, 4, "Message", null));
				Assert.AreEqual("args", ex.ParamName);
			}
		}
	}
}
