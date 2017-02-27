using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace TestAutomationEssentials.Selenium.UnitTests
{
    [TestClass]
    public class BrowserElementTests : SeleniumTestBase
    {
        [TestMethod]
        public void ClearClearsTheTextInATextBox()
        {
            const string pageSource = @"
<html>
<body>
<input id=""my-input"" value=""some value""/>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                IWebElement webElement = browser.WaitForElement(By.Id("my-input"), "my input");
                Assert.AreEqual("some value", webElement.GetAttribute("value"));
                webElement.Clear();
                Assert.AreEqual(string.Empty, webElement.GetAttribute("value"));
            }
        }
    }
}
