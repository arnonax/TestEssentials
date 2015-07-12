using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage]
	public class TestUtilsTests
	{
		[TestMethod]
		public void ExpectExceptionFailsIfTheActionDoesntThrowAnException()
		{
			TestUtils.ExpectException<AssertFailedException>(() => TestUtils.ExpectException<Exception>(() => Functions.EmptyAction()));
		}

		[TestMethod]
		public void ExpectExceptionRethrowsExceptionsThatAreNotFromTheSpecifiedType()
		{
			TestUtils.ExpectException<FormatException>(() =>
				TestUtils.ExpectException<ArgumentNullException>(() => { throw new FormatException(); }));
		}

		[TestMethod]
		public void ExpectExceptionReturnsTheCaughtExceptionIfItsFromTheSpecifiedType()
		{
			var thrownException = new FormatException("Test Exception");
			var caughtException = TestUtils.ExpectException<FormatException>(() => { throw thrownException; });
			Assert.AreEqual(thrownException, caughtException, "Caught Exception is not the same as the thrown exception");
		}
	}
}
