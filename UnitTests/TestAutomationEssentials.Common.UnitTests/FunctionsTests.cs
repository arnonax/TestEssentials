using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Common.UnitTests
{
	[TestClass]
	public class FunctionsTests
	{
		[TestMethod]
		public void NegateFlipsTheResultOfABooleanFunction()
		{
			Func<bool> func = () => true;
			Assert.IsFalse(func.Negate()());

			func = () => false;
			Assert.IsTrue(func.Negate()());
		}

		[TestMethod]
		public void NegateThrowsArgumentNullExceptionIfFuncIsNull()
		{
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => Functions.Negate(null));
			Assert.AreEqual("func", ex.ParamName);
		}
	}
}
