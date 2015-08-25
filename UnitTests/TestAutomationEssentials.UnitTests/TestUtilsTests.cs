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

		[TestMethod]
		public void ExpectExceptionThrowsArgumentNullExceptionIfActionIsNull()
		{
			var caughtException = TestUtils.ExpectException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(null));

			Assert.AreEqual("action", caughtException.ParamName);
		}

		[TestMethod]
		public void ExpectExceptionThrowsArgumentNullExceptionIfMessageOrArgsAreNull()
		{
			var caughtException = TestUtils.ExpectException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(
					() =>
					{
						throw new FormatException();
					}, null));

			Assert.AreEqual("message", caughtException.ParamName);

			caughtException = TestUtils.ExpectException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(
					() =>
					{
						throw new FormatException();
					}, "Dummy message", null));

			Assert.AreEqual("messageArgs", caughtException.ParamName);

		}
	}
}
