using System;
using System.Collections.Generic;
using System.Linq;
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

            AssertLogEntry(logEntries, $"Click on '{buttonDescription}'");
        }

        [TestMethod]
        public void GetCssValueReturnsTheCssValueOfTheElement()
        {
            const string pageSource = @"
<html>
<body>
<button id='myButton' style='font-weight: 500'>Click me</button>
</body>
</html>";
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                IWebElement button = browser.WaitForElement(By.Id("myButton"), "my button");
                Assert.AreEqual("500", button.GetCssValue("font-weight"), "GetCssValue('font-weight') of button should return 'bold'");
            }
        }

        [TestMethod]
        public void SetTextReplacesTheTextOfAnInput()
        {
            const string pageSource = @"
<html>
<body>
<input id='myInput' value='initial value' />
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var input = browser.WaitForElement(By.Id("myInput"), "my input");
                input.Text = "New value";
                Assert.AreEqual("New value", input.GetAttribute("value"));
            }
        }

        [TestMethod]
        public void SettingTextIsWrittenToTheLog()
        {
            const string pageSource = @"
<html>
<body>
<input value='initial value' />
</body>
</html>";

            var logEntries = RedirectLogs();

            var inputDescription = Guid.NewGuid().ToString();
            const string textToWrite = "New value";
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("input"), inputDescription);
                button.Text = textToWrite;
            }

            AssertLogEntry(logEntries, $"Type '{textToWrite}' in '{inputDescription}'");
        }

        [TestMethod]
        public void EnabledReturnsWhetherTheElementIsEnabled()
        {
            var pageSource = @"
<html>
<script>
function disableCheckBox() {
    var checkBox = document.getElementById('myCheckbox');
    checkBox.disabled = true;
}
</script>
<body>
<button onClick='disableCheckBox()'>Click to disable the checkbox</button>
<input type='checkbox' id='myCheckbox'/>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), "button");
                var checkbox = browser.WaitForElement(By.Id("myCheckbox"), "checkbox");
                Assert.IsTrue(checkbox.Enabled, "At first the checkbox should be enabled");
                button.Click();
                Assert.IsFalse(checkbox.Enabled, "After clicking the button, the checkbox should be disabled");
            }
        }

        [TestMethod]
        public void SelectedReturnsWhetherTheElementIsSelected()
        {
            const string pageSource = @"
<html>
<body>
<input type='checkbox'>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var checkBox = browser.WaitForElement(By.TagName("input"), "checkBox");
                Assert.IsFalse(checkBox.Selected, "Initially the checkbox should not be seleted");
                checkBox.Click();
                Assert.IsTrue(checkBox.Selected, "After click, the checkbox should be seleted");
            }
        }

        [TestMethod]
        public void LocationReturnsTheElementsLocation()
        {
            const string pageSource = @"
<html>
<body>
<button style='position:fixed; left:100px; top:20px'>Hello</button>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), "button");
                var location = button.Location;
                Assert.AreEqual(100, location.X, "Left");
                Assert.AreEqual(20, location.Y, "Top");
            }
        }

        [TestMethod]
        public void SizeReturnsTheElementsSize()
        {
            const string pageSource = @"
<html>
<body>
<button style='width:50px; height:30px'/>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), "button");
                var size = button.Size;
                Assert.AreEqual(50, size.Width, "Width");
                Assert.AreEqual(30, size.Height, "Height");
            }
        }

        [TestMethod]
        public void DisplayedReturnsTrueOnlyIfTheElementIsDisplayed()
        {
            const string pageSource = @"
<html>
<body>
<button id='visibleButton'>I'm visible</button>
<button id='invisibleButton' style='visibility: hidden'>I'm invisible</button>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var visibleButton = browser.WaitForElement(By.Id("visibleButton"), "Visible button");
                var invisibleButton = browser.WaitForElement(By.Id("invisibleButton"), "Invisible button");
                Assert.IsTrue(visibleButton.Displayed, "Visible button");
                Assert.IsFalse(invisibleButton.Displayed, "Invisible button");
            }
        }

        [TestMethod]
        public void DisplayedReturnsFalseIfTheElementIsRemoved()
        {
            const string pageSource = @"
<html>
<script>
function removeButton() {
	var container = document.getElementById('container');
    var testButton = document.getElementById('testButton');
    container.removeChild(testButton);
}

function createButton() {
	var container = document.getElementById('container');
	var testButton = document.createElement('button');
	testButton.id = 'testButton';
	testButton.innerText = 'Button';
	container.appendChild(testButton);
}
</script>
<body>
	<div id='container'>
		<button id='remove' onClick='removeButton()' >Remove</button>
		<button id='create' onClick='createButton()' >Create</button>
		<button id='testButton'>Button</button>
	</div>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var removeButton = browser.WaitForElement(By.Id("remove"), "remove button");
                var createButton = browser.WaitForElement(By.Id("create"), "create button");
                var testButton = browser.WaitForElement(By.Id("testButton"), "Test button");
                removeButton.Click();
                Assert.IsFalse(testButton.Displayed);
                createButton.Click();
                Assert.IsTrue(testButton.Displayed);
            }
        }

        [TestMethod]
        public void DoubleClick()
        {
            const string pageSource = @"
<html>
<body>
<button ondblclick=""this.innerHTML='OK'"">Doubleclick me</button>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), "button");
                button.DoubleClick();
                Assert.AreEqual("OK", button.Text);
            }
        }

        [TestMethod]
        public void DoubleClicksAreWrittenToTheLog()
        {
            const string pageSource = @"
<html>
<body>
<button>Double click me!</button>
</body>
</html>";

            var logEntries = RedirectLogs();

            var buttonDescription = Guid.NewGuid().ToString();
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), buttonDescription);
                button.DoubleClick();
            }

            AssertLogEntry(logEntries, $"Double click on '{buttonDescription}'");
        }

        [TestMethod]
        public void DoubleClickMovesTheCursorToTheElementBeforeDoubleClicking()
        {
            const string pageSource = @"
<html>
<script>
function addDoubleClickHandler(e) {
	e.target.ondblclick = function() {
    	e.target.innerHTML = 'OK';
    }
}
</script>
<body>
<button onmousemove='addDoubleClickHandler(event)'>Double-click me</button>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var button = browser.WaitForElement(By.TagName("button"), "button");
                button.DoubleClick();
                Assert.AreEqual("OK", button.Text);
            }
        }

        [TestMethod]
        public void FindElementWithinElement()
        {
            const string pageSource = @"
<html>
<body>
<div>
    <button>dummy</button>
</div>
</body>
</html>";
            using (var browser = OpenBrowserWithPage(pageSource))
            {
                IWebElement div = browser.WaitForElement(By.TagName("div"), "div");
                var button = div.FindElement(By.TagName("button"));
                Assert.AreEqual("button", button.TagName);
            }
        }

        [TestMethod]
        public void FindMultipleElementsWithinElement()
        {
            const string pageSource = @"
<html>
<body>
<div>
    <span>Hello</span>
    <span>World</span>
</div>
<span>This shouldn't be returned</span>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                var div = browser.WaitForElement(By.TagName("div"), "div");
                var spans = div.FindElements(By.TagName("span"), "inner spans").ToList();
                Assert.AreEqual(2, spans.Count);
                CollectionAssert.AreEqual(new[] {"Hello", "World"}, spans.Select(x => x.Text).ToList());
            }
        }

        [TestMethod]
        public void FindMultipleElementsWithinElementUsingIWebElement()
        {
            const string pageSource = @"
<html>
<body>
<div>
    <span>Hello</span>
    <span>World</span>
</div>
<span>This shouldn't be returned</span>
</body>
</html>";

            using (var browser = OpenBrowserWithPage(pageSource))
            {
                IWebElement div = browser.WaitForElement(By.TagName("div"), "div");
                var spans = div.FindElements(By.TagName("span"));
                Assert.AreEqual(2, spans.Count);
                CollectionAssert.AreEqual(new[] { "Hello", "World" }, spans.Select(x => x.Text).ToList());
            }
        }


        private static List<string> RedirectLogs()
        {
            var logEntries = new List<string>();
            Logger.Initialize(entry => logEntries.Add(entry));
            AddCleanupAction(() => Logger.Initialize(Logger.DefaultImplementations.Console));
            return logEntries;
        }

        private static void AssertLogEntry(List<string> logEntries, string expectedLogEntry)
        {
            Assert.AreEqual(1, logEntries.FindAll(entry => entry.EndsWith(expectedLogEntry)).Count,
                "Entry '{0}' should be written once. All entries:\n{1}",
                expectedLogEntry,
                string.Join("\n", logEntries));
        }
    }
}
