using System;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.Common.ExecutionContext;
using TestAutomationEssentials.MSTest;
using TestAutomationEssentials.UnitTests;

namespace TestAutomationEssentials.Selenium.UnitTests
{
    [TestClass, Ignore]
    public class BrowserWindowTests : SeleniumTestBase
    {
        private const string NewWindowTitle = "New Window";
        private const string FirstWindowTitle = @"First Window";

        [TestMethod]
        public void OpenWindowReturnsTheNewlyOpenedWindow()
        {
            using (var browser = OpenBrowserWithLinkToNewWindow())
            {
                var mainWindow = browser.MainWindow;

                var newWindow = ClickOpenWindow(browser);

                Assert.AreEqual(NewWindowTitle, newWindow.Title);
                Assert.AreEqual("First Window", mainWindow.Title);
            }
        }

        [TestMethod]
        public void OpenWindowThrowsTimeoutExceptionIfAWindowIsntOpenedAfterSpecifiedTimeout()
        {
            const string pageSource = @"
<html>
<body>
<button id='dummyButton'>Click me</button>
</body>
</html>";
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.Id("dummyButton"), "Dummy button");
                var startTime = DateTime.MinValue;
                var expectedTimeout = WaitTests.DefaultWaitTimeoutForUnitTests;
                TestUtils.ExpectException<TimeoutException>(() =>
                    browser.OpenWindow(() =>
                    {
                        startTime = DateTime.Now;
                        button.Click();

                    }, "non existent window", expectedTimeout)); // TODO: use WaitTests's constant after merge with master
                var endTime = DateTime.Now;
                WaitTests.AssertTimeoutWithinThreashold(startTime, endTime, expectedTimeout + WaitTests.DefaultWaitTimeoutForUnitTests, "OpenWindow");
            }
        }

        [TestMethod]
        public void OpenWindowChecksForNullArguments()
        {
            var driver = A.Fake<IWebDriver>();
            var browser = new Browser("dummy description", driver, TestExecutionScopesManager);
            ArgumentNullException ex;
            ex = TestUtils.ExpectException<ArgumentNullException>(
                () => browser.OpenWindow(null, null, TimeSpan.Zero));
            Assert.AreEqual("action", ex.ParamName);

            ex = TestUtils.ExpectException<ArgumentNullException>(
                () => browser.OpenWindow(() => { }, null, TimeSpan.Zero));
            Assert.AreEqual("windowDescription", ex.ParamName);
        }

        [TestMethod]
        public void CloseWindow()
        {
            using (var browser = OpenBrowserWithLinkToNewWindow())
            {
                var driver = browser.GetWebDriver();
                var newWindow = ClickOpenWindow(browser);

                Assert.AreEqual(2, driver.WindowHandles.Count, "2 Windows should be open after OpenWindow was called");

                newWindow.Close();
                Assert.AreEqual(1, driver.WindowHandles.Count, "1 Window should be open after disposing the IsolationScope");
            }
        }

        [TestMethod]
        public void WindowIsClosedOnCleanup()
        {
            var scopesManager = new TestExecutionScopesManager("Dummy test scope", Functions.EmptyAction<IIsolationScope>());
            using (var browser = OpenBrowserWithLinkToNewWindow(scopesManager))
            {
                var driver = browser.GetWebDriver();
                using (scopesManager.BeginIsolationScope("Window scope"))
                {

                    var link = browser.WaitForElement(By.Id("myLink"), "Link to other window");
                    browser.OpenWindow(() => link.Click(), "Other window");
                    Assert.AreEqual(2, driver.WindowHandles.Count, "2 Windows should be open after OpenWindow was called");
                }
                Assert.AreEqual(1, driver.WindowHandles.Count, "1 Window should be open after disposing the IsolationScope");
            }
        }

        [TestMethod]
        public void NoExceptionIsThrownOnCleanupIfBrowserIsDisposedWhenTheresAnOpenWindow()
        {
            using (TestExecutionScopesManager.BeginIsolationScope("Window scope"))
            {
                using (var browser = OpenBrowserWithLinkToNewWindow())
                {
                    var link = browser.WaitForElement(By.Id("myLink"), "Link to other window");
                    browser.OpenWindow(() => link.Click(), "Other window");
                }
            }
        }

        [TestMethod]
        public void BrowserPropertyReturnsTheInstaneOfTheBrowserUsedToOpenTheWindow()
        {
            using (var browser = OpenBrowserWithLinkToNewWindow())
            {
                var newWindow = ClickOpenWindow(browser);
                Assert.AreSame(browser, newWindow.Browser);
            }
        }

        [TestMethod]
        public void SettingUrlNavigatesToAppropriatePage()
        {
            var title = "Another web site";
            var anotherSite = CreatePageWithTitle(title);
            using (var browser = OpenBrowserWithLinkToNewWindow())
            {
                var newWindow = ClickOpenWindow(browser);
                newWindow.NavigateToUrl(anotherSite.AbsoluteUri);

                Assert.AreEqual(title, newWindow.Title);
            }
        }

        private static BrowserWindow ClickOpenWindow(Browser browser)
        {
            var link = browser.WaitForElement(By.Id("myLink"), "Link to other window");
            var newWindow = browser.OpenWindow(() => link.Click(), "Other window");
            return newWindow;
        }

        private Browser OpenBrowserWithLinkToNewWindow()
        {
            return OpenBrowserWithLinkToNewWindow(TestExecutionScopesManager);
        }

        private Browser OpenBrowserWithLinkToNewWindow(TestExecutionScopesManager scopeManager)
        {
            var otherPageUrl = CreatePageWithTitle(NewWindowTitle);

            var pageSource = @"
<html>
<head><title>" + FirstWindowTitle + @"</title></head>
<body>
<a id='myLink' target='_blank' href='" + otherPageUrl + @"'>Click here to open new window</a>
</body>
</html>
";

            var browser = OpenBrowserWithPage(pageSource, scopeManager);
            return browser;
        }

        private Uri CreatePageWithTitle(string windowTitle)
        {
            var otherPageSource = @"
<html>
<head><title>" + windowTitle + @"</title></head>
</html>";

            return CreatePage(otherPageSource);
        }
    }
}
