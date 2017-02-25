using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Selenium.UnitTests
{
    [TestClass]
    public class BrowserTests : TestBase
    {
        [TestMethod]
        public void ConstructorThrowsArgumentNullExceptionsIfNullsArePassed()
        {
            var ex = TestUtils.ExpectException<ArgumentNullException>(() => new Browser(null, null));
            Assert.AreEqual("description", ex.ParamName);

            ex = TestUtils.ExpectException<ArgumentNullException>(() => new Browser("dummy", null));
            Assert.AreEqual("webDriver", ex.ParamName);
        }

        class BrowserDerived : Browser
        {
            public BrowserDerived(IWebDriver webDriver) 
                : base("dummy description", webDriver)
            {
                var webDriverFromBase = WebDriver;
                Assert.AreSame(webDriverFromBase, webDriver);
            }
        }

        [TestMethod]
        public void DerivedClassCanAccessUnderlyingWebDriver()
        {
            var driver = A.Fake<IWebDriver>();
            new BrowserDerived(driver);
        }

        [TestMethod]
        public void OtherClassesCanAccessUnderlyingWebDriver()
        {
            var driver = A.Fake<IWebDriver>();
            var browser = new Browser("dummy description", driver);
            Assert.AreSame(driver, browser.GetWebDriver());
        }

        [TestMethod]
        public void CanRetrieveDescriptionSpecifiedInConstructor()
        {
            var driver = A.Fake<IWebDriver>();
            var description = "dummy description";
            var browser = new Browser(description, driver);
            Assert.AreEqual(description, browser.Description);
        }

        [TestMethod]
        public void DisposingTheBrowserQuitsWebDriver()
        {
            var driverMock = new Fake<IWebDriver>();
            var driver = driverMock.FakedObject;
            using (new Browser("dummy description", driver))
            {
            }
            driverMock.CallsTo(x => x.Quit()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void NavigateToUrlNavigatesToTheSpecifiedUrl()
        {
            var dummyPageUrl = GetUrlForFile("dummyPage.html");
            var driver = new ChromeDriver();
            using (var browser = new Browser("", driver))
            {
                browser.NavigateToUrl(dummyPageUrl);
                Assert.AreEqual(new Uri(dummyPageUrl).AbsoluteUri, new Uri(driver.Url).AbsoluteUri);
            }
        }

        [TestMethod]
        public void WaitForElementFindsAnExistingElement()
        {
            const string pageSource = @"
<html>
<body>
<button id=""myButtonId"">My button</button>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                const string description = "dummy button description";
                var button = browser.WaitForElement(By.Id("myButtonId"), description);
                Assert.AreEqual("My button", button.Text);
                Assert.AreEqual(description, button.Description);
            }
        }

        [TestMethod]
        public void WaitForElementWaitsForAnElementToBeCreated()
        {
            const string pageSource = @"
<html>
<script>
    function createElementWithDelay() {
        window.setTimeout(function() {
            var newSpan = document.createElement(""span"");
                newSpan.id = ""mySpan"";
                newSpan.innerText = ""Hello"";
                document.body.appendChild(newSpan);
            },
            100);

    }
</script>
<body>
<button id=""myButtonId"" onclick='createElementWithDelay()'>My button</button>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.Id("myButtonId"), "button");
                button.Click();
                var input = browser.WaitForElement(By.Id("mySpan"), "span", 1);
                Assert.AreEqual("Hello", input.Text);
            }
        }

        [TestMethod]
        public void WaitForElementThrowsTimeoutExceptionIfElementDoesNotExist()
        {
            const string pageSource = @"
<html>
<body>
<button id=""myButtonId"">My button</button>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.Id("myButtonId"), "button");
                button.Click();
                
                // ReSharper disable once AccessToDisposedClosure
                var ex = TestUtils.ExpectException<TimeoutException>(() => browser.WaitForElement(By.Id("mySpan"), "span", 1));

                Logger.WriteLine("Actual exception:");
                Logger.WriteLine(ex.ToString());

                StringAssert.Contains(ex.Message, $"'{browser.Description}'");
                StringAssert.Contains(ex.Message, "'span'");
                StringAssert.Contains(ex.Message, By.Id("mySpan").ToString());
                StringAssert.Contains(ex.Message, "'1' seconds");
            }
        }

        [TestMethod]
        public void ImproveMessageOfStaleElementReferenceException()
        {
            const string pageSource = @"
<html>
<script>
function removeSpan() {
    var span = document.getElementById(""mySpan"");
    document.body.removeChild(span);
}
</script>
<body>
    <button id=""myButton"" onclick=""removeSpan()"">Click me</button>
    <span id=""mySpan"">Some text</span>
</body>
</html
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.Id("myButton"), "button");
                var span = browser.WaitForElement(By.Id("mySpan"), "span");
                button.Click();
                
                // ReSharper disable once AccessToDisposedClosure
                var ex = TestUtils.ExpectException<StaleElementReferenceException>(
                    () => span.Click());

                Logger.WriteLine("Actual exception:");
                Logger.WriteLine(ex);
                
                StringAssert.Contains(ex.Message, "'span'");
            }
        }

        [TestMethod]
        public void FindElementsCanFindMultipleMatchingElements()
        {
            const string pageSource = @"
<html>
<body>
<button class=""myClass"">My button</button>
<input class=""myClass""/>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                const string description = "dummy description";
                var elements = browser.FindElements(By.ClassName("myClass"), description).ToList();

                Assert.AreEqual(2, elements.Count, "Number of returned elements");
                var button = elements.Find(x => ((IWebElement) x).TagName == "button");
                var descriptionPattern = new Regex(description + @"\[\d+\]");
                StringAssert.Matches(button.Description, descriptionPattern);
                var input = elements.Find(x => ((IWebElement) x).TagName == "input");
                StringAssert.Matches(input.Description, descriptionPattern);
                Assert.AreNotEqual(button.Description, input.Description);
            }
        }

        [TestMethod]
        public void FindElementsReturnEmptyListIfNotMatchingElementsExist()
        {
            const string pageSource = @"
<html>
<body>
<button id=""myButtonId"">My button</button>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var matchingElements = browser.FindElements(By.Id("non-existent"), "Non-existent");
                Assert.IsTrue(matchingElements.IsEmpty());
            }
        }

        [TestMethod]
        public void ElementAppearsReturnsTrueOnlyIfElementIsVisible()
        {
            const string pageSource = @"
<html>
<body>
<button id=""visible-element"">Visible</button>
<button hidden id=""invisible-element"">
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                Assert.IsFalse(browser.ElementAppears(By.Id("non-existent")), "Non existent element should not appear");
                Assert.IsFalse(browser.ElementAppears(By.Id("invisible-element")), "Hidden element should not appear");
                Assert.IsTrue(browser.ElementAppears(By.Id("visible-element")), "Visible element should appear");
            }
        }

        private Browser OpenBrowserWithPage(string pageSource)
        {
            var filename = Path.GetTempFileName();
            File.Move(filename, filename += ".html");
            File.WriteAllText(filename, pageSource);
            TestContext.AddResultFile(filename);
            var driver = new ChromeDriver();
            var browser = new Browser("test browser", driver);
            browser.NavigateToUrl(new Uri(filename).AbsoluteUri);
            return browser;
        }

        private static string GetUrlForFile(string filename)
        {
            return $"file:///{Directory.GetCurrentDirectory()}/{filename}";
        }

    }
}
