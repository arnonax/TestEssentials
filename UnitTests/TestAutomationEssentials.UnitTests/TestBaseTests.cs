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
    }
}