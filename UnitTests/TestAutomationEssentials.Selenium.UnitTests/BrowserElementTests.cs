using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;

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

        [TestMethod]
        public void SendKeysAppendsText()
        {
            const string pageSource = @"
<html>
<body>
<input id=""my-input"" value=""Hello, ""/>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                IWebElement webElement = browser.WaitForElement(By.Id("my-input"), "my input");
                Assert.AreEqual("Hello, ", webElement.GetAttribute("value"));
                webElement.SendKeys("world!");
                Assert.AreEqual("Hello, world!", webElement.GetAttribute("value"));
            }
        }

        [TestMethod]
        public void SubmitOnBrowserElementSubmitsTheForm()
        {
            const string pageSource = @"
<html>
<body>
<form>
<input name='myInput'/>
<button action='submit' >Submit</button>
</form>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var webDriver = browser.GetWebDriver();
                var initialUrl = webDriver.Url;

                IWebElement input = browser.WaitForElement(By.Name("myInput"), "my input");
                input.SendKeys("dummyValue");
                input.Submit();

                Assert.AreEqual($"{initialUrl}?myInput=dummyValue", webDriver.Url);
            }
        }

        [TestMethod]
        public void ClicksAreWrittenToTheLog()
        {
            const string pageSource = @"
<html>
<body>
<button>Click me!</button>
</body>
</html>";

            var logEntries = RedirectLogs();

            var buttonDescription = Guid.NewGuid().ToString();
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), buttonDescription);
                button.Click();
            }

            var expectedLogEntry = $"Click on '{buttonDescription}'";
            Assert.AreEqual(1, logEntries.FindAll(entry => entry.EndsWith(expectedLogEntry)).Count,
                "Entry '{0}' should be written once. All entries:\n{1}", 
                expectedLogEntry,
                string.Join("\n", logEntries));
        }

        private static List<string> RedirectLogs()
        {
            var logEntries = new List<string>();
            Logger.Initialize(entry => logEntries.Add(entry));
            AddCleanupAction(() => Logger.Initialize(Logger.DefaultImplementations.Console));
            return logEntries;
        }
    }
}
