using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAutomationEssentials.UnitTests
{
    [TestClass]
    public class TestBaseTests : CommonTestBaseTests
    {
        protected override ITestClass CreateTestClass(string dllName, TestContext testContext)
        {
            return new TestClassWrapper(dllName, testContext);
        }

        // The following test fails in MsTestV2 due to the following issue: https://github.com/Microsoft/testfx/issues/265
        [TestMethod]
        public void WhenATestFailsAndAlsoACleanupActionFailsThenTheTestFailureIsReported()
        {
            var outputFileName = Path.GetFullPath("Output.txt");
            File.Delete(outputFileName);

            var testClass = CreateTestClass(
                GetLinePragma() +
                @"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

[TestClass]
public class TestClass1 : TestBase
{
	[TestMethod]
	public void TestMethod1()
	{
		AddCleanupAction(() => { throw new Exception(""CleanupAction failure...""); });
		throw new Exception(""TestMethod failure..."");
	}
}
");
            var testResults = testClass.Execute();
            Assert.AreEqual(1, testResults.FailedTests, "Failed");

            StringAssert.Contains(testResults.UnitTestResults[0].ErrorMessage, "TestMethod failure...");
        }
    }
}