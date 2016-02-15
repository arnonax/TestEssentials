using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class AssertsAggregatorTests
	{
		[TestMethod]
		public void EmptyAssertAggregatorPassesTheTest()
		{
			using (new AssertsAggregator("dummy aggregator"))
			{
			}
		}

		[TestMethod]
		public void IfAllAssertsPassNoExceptionIsThrown()
		{
			using (var asserts = new AssertsAggregator("dummy"))
			{
				asserts.AreEqual(3, () => 3, "3 always equal 3");
				asserts.AreEqual(4, () => 4, "4 always equal 4");
				asserts.IsTrue(() => true, "true is always true...");
			}
		}

		[TestMethod]
		public void ConstructorValidatesThatDescriptionIsNotNull()
		{
			const string nullDescription = null;
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => new AssertsAggregator(nullDescription));
			Assert.AreEqual("description", ex.ParamName, "Parameter name");
		}

		[TestMethod]
		public void AssertsAggregatorStartsASectionInLogger()
		{
			var lines = new List<string>();
			Logger.Initialize(line => lines.Add(line));

			const string description = "dummy description";
			using (new AssertsAggregator(description)) { }
			Assert.AreEqual(1, lines.Count, "1 line should be written");
			StringAssert.Contains(lines.Content(), "Verifying: " + description);
		}

		[TestMethod]
		public void AssertsAggregatorThrowsAssertFailedExceptionOnlyAfterAllAssertionsHaveBeenValidated()
		{
			var lineAfterAssertIsExecuted = false;
			Action test = () =>
			{
				using (var asserts = new AssertsAggregator("dummy"))
				{
					asserts.AreEqual(2, () => 3, "2 does not equal 3");
					lineAfterAssertIsExecuted = true;
				}
			};

			TestUtils.ExpectException<AssertFailedException>(test);
			Assert.IsTrue(lineAfterAssertIsExecuted, "Line after assert should have been executed");
		}

		[TestMethod]
		public void AssertsAggregatorThrowsAssertFailedExceptionEvenIfTheFailureOccursWhenTryingToGetTheActualResult()
		{
			Func<int> getDividionByZeroResult = () =>
			{
				var x = 0;
				return 1 / x;
			};

			Action test = () =>
			{
				using (var asserts = new AssertsAggregator("dummy"))
				{
					asserts.AreEqual(1, () => getDividionByZeroResult(), "Dividion by zero should be 1. Really?!");
				}
			};

			TestUtils.ExpectException<AssertFailedException>(test);
		}

		[TestMethod]
		public void AssertAggregatorCanHaveBooleanAsserts()
		{
			using (var asserts = new AssertsAggregator("dummy"))
			{
				asserts.IsTrue(() => true, "true is always true...");
			}

			Action negativeTest = () =>
			{
				using (var asserts = new AssertsAggregator("dummy"))
				{
					asserts.IsTrue(() => false, "false is never true...");
				}
			};

			TestUtils.ExpectException<AssertFailedException>(negativeTest);
		}
	}
}
