using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

#pragma warning disable 618 // Disable Obsolete warning, as we're testing an obsolete method here...

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage]
	public class TestUtilsTests
	{
		[TestMethod]
		public void ExpectExceptionFailsIfTheActionDoesntThrowAnException()
		{
			Assert.ThrowsException<AssertFailedException>(() => TestUtils.ExpectException<Exception>(() => Functions.EmptyAction()));
		}

		[TestMethod]
		public void ExpectExceptionRethrowsExceptionsThatAreNotFromTheSpecifiedType()
		{
			Assert.ThrowsException<FormatException>(() =>
				TestUtils.ExpectException<ArgumentNullException>(() => throw new FormatException()));
		}

		[TestMethod]
		public void ExpectExceptionReturnsTheCaughtExceptionIfItsFromTheSpecifiedType()
		{
			var thrownException = new FormatException("Test Exception");
			var caughtException = TestUtils.ExpectException<FormatException>(() => throw thrownException);
			Assert.AreEqual(thrownException, caughtException, "Caught Exception is not the same as the thrown exception");
		}

		[TestMethod]
		public void ExpectExceptionThrowsArgumentNullExceptionIfActionIsNull()
		{
			var caughtException = Assert.ThrowsException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(null));

			Assert.AreEqual("action", caughtException.ParamName);
		}

		[TestMethod]
		public void ExpectExceptionThrowsArgumentNullExceptionIfMessageOrArgsAreNull()
		{
			var caughtException = Assert.ThrowsException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(
					() => throw new FormatException(), null));

			Assert.AreEqual("message", caughtException.ParamName);

			caughtException = Assert.ThrowsException<ArgumentNullException>(
				() => TestUtils.ExpectException<FormatException>(
					() => throw new FormatException(), "Dummy message", null));

			Assert.AreEqual("messageArgs", caughtException.ParamName);

		}
	}
}
