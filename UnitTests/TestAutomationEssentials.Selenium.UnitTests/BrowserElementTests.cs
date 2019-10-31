using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.Extensions;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.UnitTests;

namespace TestAutomationEssentials.Selenium.UnitTests
{
    // TODO: change the more trivial tests to unit tests using mocks.
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
        //}

        //[TestMethod]
        //public void SendKeysAppendsText()
        //{
            const string pageSource1 = @"
<html>
<body>
<input id=""my-input"" value=""Hello, ""/>
</body>
</html>
";

            using (var browser = OpenBrowserWithPage(pageSource1))
            {
                IWebElement webElement = browser.WaitForElement(By.Id("my-input"), "my input");
                Assert.AreEqual("Hello, ", webElement.GetAttribute("value"));
                webElement.SendKeys("world!");
                Assert.AreEqual("Hello, world!", webElement.GetAttribute("value"));
            }
        //}

        //[TestMethod]
        //public void SubmitOnBrowserElementSubmitsTheForm()
        //{
            const string pageSource2 = @"
        <html>
        <head>
        <script>
        function writeQueryString() {
        	document.getElementById('result').innerHTML = window.location.search;
        }
        </script>
        </head>
        <body onload='writeQueryString()'>
        <form>
        <input name='myInput'/>
        <button action='submit' >Submit</button>
        </form>
        <span id='result'/>
        </body>
        </html>";

            using (var browser = OpenBrowserWithPage(pageSource2))
            {
                IWebElement input = browser.WaitForElement(By.Name("myInput"), "my input");
                input.SendKeys("dummyValue");
                input.Submit();
                var result = browser.WaitForElement(By.Id("result"), "Result");

                Assert.AreEqual("?myInput=dummyValue", result.Text);
            }
        //}

        //[TestMethod]
        //public void ClicksAreWrittenToTheLog()
        //{
            const string pageSource3 = @"
<html>
<body>
<button>Click me!</button>
</body>
</html>";

            var logEntries = RedirectLogs();

            var buttonDescription = Guid.NewGuid().ToString();
            using (var browser = OpenBrowserWithPage(pageSource3))
            {
                var button = browser.WaitForElement(By.TagName("button"), buttonDescription);
                button.Click();
            }

            AssertLogEntry(logEntries, $"Click on '{buttonDescription}'");
        }

//        [TestMethod]
//        public void GetPropertyReturnsTheValueOfAProperty()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<span>Dummy text</span>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                IWebElement span = browser.WaitForElement(By.TagName("span"), "span");
//                Assert.AreEqual("Dummy text", span.GetProperty("innerText"));
//            }
//        }

//        [TestMethod]
//        public void GetCssValueReturnsTheCssValueOfTheElement()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<button id='myButton' style='font-weight: 500'>Click me</button>
//</body>
//</html>";
//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                IWebElement button = browser.WaitForElement(By.Id("myButton"), "my button");
//                Assert.AreEqual("500", button.GetCssValue("font-weight"), "GetCssValue('font-weight') of button should return 'bold'");
//            }
//        }

//        [TestMethod]
//        public void SetTextReplacesTheTextOfAnInput()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<input id='myInput' value='initial value' />
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var input = browser.WaitForElement(By.Id("myInput"), "my input");
//                input.Text = "New value";
//                Assert.AreEqual("New value", input.GetAttribute("value"));
//            }
//        }

//        [TestMethod]
//        public void SettingTextIsWrittenToTheLog()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<input value='initial value' />
//</body>
//</html>";

//            var logEntries = RedirectLogs();

//            var inputDescription = Guid.NewGuid().ToString();
//            const string textToWrite = "New value";
//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var button = browser.WaitForElement(By.TagName("input"), inputDescription);
//                button.Text = textToWrite;
//            }

//            AssertLogEntry(logEntries, $"Type '{textToWrite}' in '{inputDescription}'");
//        }

//        [TestMethod]
//        public void EnabledReturnsWhetherTheElementIsEnabled()
//        {
//            var pageSource = @"
//<html>
//<script>
//function disableCheckBox() {
//    var checkBox = document.getElementById('myCheckbox');
//    checkBox.disabled = true;
//}
//</script>
//<body>
//<button onClick='disableCheckBox()'>Click to disable the checkbox</button>
//<input type='checkbox' id='myCheckbox'/>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var button = browser.WaitForElement(By.TagName("button"), "button");
//                var checkbox = browser.WaitForElement(By.Id("myCheckbox"), "checkbox");
//                Assert.IsTrue(checkbox.Enabled, "At first the checkbox should be enabled");
//                button.Click();
//                Assert.IsFalse(checkbox.Enabled, "After clicking the button, the checkbox should be disabled");
//            }
//        }

//        [TestMethod]
//        public void SelectedReturnsWhetherTheElementIsSelected()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<input type='checkbox'>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var checkBox = browser.WaitForElement(By.TagName("input"), "checkBox");
//                Assert.IsFalse(checkBox.Selected, "Initially the checkbox should not be seleted");
//                checkBox.Click();
//                Assert.IsTrue(checkBox.Selected, "After click, the checkbox should be seleted");
//            }
//        }

//        [TestMethod]
//        public void LocationReturnsTheElementsLocation()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<button style='position:fixed; left:100px; top:20px'>Hello</button>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var button = browser.WaitForElement(By.TagName("button"), "button");
//                var location = button.Location;
//                Assert.AreEqual(100, location.X, "Left");
//                Assert.AreEqual(20, location.Y, "Top");
//            }
//        }

//        [TestMethod]
//        public void SizeReturnsTheElementsSize()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<button style='width:50px; height:30px'/>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var button = browser.WaitForElement(By.TagName("button"), "button");
//                var size = button.Size;
//                Assert.AreEqual(50, size.Width, "Width");
//                Assert.AreEqual(30, size.Height, "Height");
//            }
//        }

//        [TestMethod]
//        public void DisplayedReturnsTrueOnlyIfTheElementIsDisplayed()
//        {
//            const string pageSource = @"
//<html>
//<body>
//<button id='visibleButton'>I'm visible</button>
//<button id='invisibleButton'>I'm invisible</button>
//</body>
//</html>";

//            using (var browser = OpenBrowserWithPage(pageSource))
//            {
//                var visibleButton = browser.WaitForElement(By.Id("visibleButton"), "Visible button");
//                var invisibleButton = browser.WaitForElement(By.Id("invisibleButton"), "Invisible button");
//                browser.GetWebDriver()
//                    .ExecuteJavaScript("document.getElementById('invisibleButton').style.visibility = 'hidden'");
//                Assert.IsTrue(visibleButton.Displayed, "Visible button");
//                Assert.IsFalse(invisibleButton.Displayed, "Invisible button");
//            }
//        }

        //        [TestMethod]
        //        public void DisplayedReturnsFalseIfTheElementIsRemoved()
        //        {
        //            const string pageSource = @"
        //<html>
        //<script>
        //function removeButton() {
        //	var container = document.getElementById('container');
        //    var testButton = document.getElementById('testButton');
        //    container.removeChild(testButton);
        //}

        //function createButton() {
        //	var container = document.getElementById('container');
        //	var testButton = document.createElement('button');
        //	testButton.id = 'testButton';
        //	testButton.innerText = 'Button';
        //	container.appendChild(testButton);
        //}
        //</script>
        //<body>
        //	<div id='container'>
        //		<button id='remove' onClick='removeButton()' >Remove</button>
        //		<button id='create' onClick='createButton()' >Create</button>
        //		<button id='testButton'>Button</button>
        //	</div>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var removeButton = browser.WaitForElement(By.Id("remove"), "remove button");
        //                var createButton = browser.WaitForElement(By.Id("create"), "create button");
        //                var testButton = browser.WaitForElement(By.Id("testButton"), "Test button");
        //                removeButton.Click();
        //                Assert.IsFalse(testButton.Displayed);
        //                createButton.Click();
        //                Assert.IsTrue(testButton.Displayed);
        //            }
        //        }

        //        [TestMethod]
        //        public void DoubleClick()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<button ondblclick=""this.innerHTML='OK'"">Doubleclick me</button>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var button = browser.WaitForElement(By.TagName("button"), "button");
        //                button.DoubleClick();
        //                Assert.AreEqual("OK", button.Text);
        //            }
        //        }

        //        [TestMethod]
        //        public void DoubleClicksAreWrittenToTheLog()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<button>Double click me!</button>
        //</body>
        //</html>";

        //            var logEntries = RedirectLogs();

        //            var buttonDescription = Guid.NewGuid().ToString();
        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var button = browser.WaitForElement(By.TagName("button"), buttonDescription);
        //                button.DoubleClick();
        //            }

        //            AssertLogEntry(logEntries, $"Double click on '{buttonDescription}'");
        //        }

        //        [TestMethod]
        //        public void DoubleClickMovesTheCursorToTheElementBeforeDoubleClicking()
        //        {
        //            const string pageSource = @"
        //<html>
        //<script>
        //function addDoubleClickHandler(e) {
        //	e.target.ondblclick = function() {
        //    	e.target.innerHTML = 'OK';
        //    }
        //}
        //</script>
        //<body>
        //<button onmousemove='addDoubleClickHandler(event)'>Double-click me</button>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var button = browser.WaitForElement(By.TagName("button"), "button");
        //                button.DoubleClick();
        //                Assert.AreEqual("OK", button.Text);
        //            }
        //        }

        //        [TestMethod]
        //        public void FindElementWithinElement()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<div>
        //    <button>dummy</button>
        //</div>
        //</body>
        //</html>";
        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                IWebElement div = browser.WaitForElement(By.TagName("div"), "div");
        //                var button = div.FindElement(By.TagName("button"));
        //                Assert.AreEqual("button", button.TagName);
        //            }
        //        }

        //        [TestMethod]
        //        public void FindMultipleElementsWithinElement()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<div>
        //    <span>Hello</span>
        //    <span>World</span>
        //</div>
        //<span>This shouldn't be returned</span>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var div = browser.WaitForElement(By.TagName("div"), "div");
        //                var spans = div.FindElements(By.TagName("span"), "inner spans").ToList();
        //                Assert.AreEqual(2, spans.Count);
        //                CollectionAssert.AreEqual(new[] {"Hello", "World"}, spans.Select(x => x.Text).ToList());
        //            }
        //        }

        //        [TestMethod]
        //        public void FindMultipleElementsWithinElementUsingIWebElement()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<div>
        //    <span>Hello</span>
        //    <span>World</span>
        //</div>
        //<span>This shouldn't be returned</span>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                IWebElement div = browser.WaitForElement(By.TagName("div"), "div");
        //                var spans = div.FindElements(By.TagName("span"));
        //                Assert.AreEqual(2, spans.Count);
        //                CollectionAssert.AreEqual(new[] { "Hello", "World" }, spans.Select(x => x.Text).ToList());
        //            }
        //        }

        //        [TestMethod]
        //        public void HoverOverAnElementTriggersItsMouseOverEvent()
        //        {
        //            const string pageSource = @"
        //<html>
        //<script>
        //function updateWorld() {
        //    var el = document.getElementById('world');
        //    el.innerHTML = 'World';
        //}
        //</script>
        //<body>
        //<span id='hello' onmousemove='updateWorld()'>Hello</span>
        //<span id='world' />
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var hello = browser.WaitForElement(By.Id("hello"), "hello");
        //                var world = browser.WaitForElement(By.Id("world"), "world");
        //                Assert.AreEqual(string.Empty, world.Text);
        //                hello.Hover();
        //                Assert.AreEqual("World", world.Text);
        //            }
        //        }

        //        [TestMethod]
        //        public void HoversAreWrittenToTheLog()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<button>Hover over me</button>
        //</body>
        //</html>";

        //            var logEntries = RedirectLogs();

        //            var buttonDescription = Guid.NewGuid().ToString();
        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var button = browser.WaitForElement(By.TagName("button"), buttonDescription);
        //                button.Hover();
        //            }

        //            AssertLogEntry(logEntries, $"Move to '{buttonDescription}'");
        //        }

        //        [TestMethod]
        //        public void GetParentReturnsTheParentElement()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<div id='parent'>
        //<span id='child'>Dummy</span>
        //</div>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var child = browser.WaitForElement(By.Id("child"), "child");
        //                IWebElement parent = child.GetParent("parent");
        //                Assert.AreEqual("div", parent.TagName);
        //            }
        //        }

        //        [Ignore] // This is not supported yet at GeckoDriver. See: https://stackoverflow.com/questions/42197200/selenium-actions-movetoelement-org-openqa-selenium-unsupportedcommandexception
        //        [TestMethod]
        //        public void DragAndDrop()
        //        {
        //            const string pageSource = @"
        //<!DOCTYPE HTML>
        //<html>
        //<head>
        //<script>
        //function allowDrop(ev) {
        //    ev.preventDefault();
        //}

        //function drag(ev) {
        //    ev.dataTransfer.setData('dummy', null);
        //}

        //function drop(ev) {
        //    ev.preventDefault();
        //    ev.target.innerText = 'Success!';
        //}
        //</script>
        //</head>
        //<body>

        //<span id='draggable' draggable='true' ondragstart='drag(event)'>Drag me</span>
        //<span id='droppable' ondrop='drop(event)' ondragover='allowDrop(event)'>Drop on me</span>

        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var draggable = browser.WaitForElement(By.Id("draggable"), "draggable");
        //                var droppable = browser.WaitForElement(By.Id("droppable"), "droppable");

        //#pragma warning disable 618 // Obsolete
        //                draggable.DragAndDrop(droppable);
        //#pragma warning restore 618

        //                Assert.AreEqual("Success!", droppable.Text);
        //            }
        //        }

        //        [TestMethod]
        //        public void WaitToDisappearWaitsForTheElementToDisappear()
        //        {
        //            // It seems that Firefox times are very inaccurate, so we expect 400ms+/-200ms (See: https://stackoverflow.com/a/16753220)
        //            var expectedTimeout = 400.Milliseconds();
        //            var pageSource = @"
        //<html>
        //<script>
        //function hideSoon() {
        //    var el = document.getElementById('myBtn');
        //    window.setTimeout(function() {
        //        el.style.display = 'none';
        //    }, " + expectedTimeout.TotalMilliseconds + @");
        //}
        //</script>
        //<body>
        //<button id='myBtn' onclick='hideSoon()'>Hide me</button>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                var btn = browser.WaitForElement(By.Id("myBtn"), "button");

        //                btn.Click();
        //                var startTime = DateTime.Now;
        //                btn.WaitToDisappear(1.Seconds());
        //                var endTime = DateTime.Now;

        //                Assert.IsFalse(btn.Displayed, "Button should have disappeared");
        //                var threshold = 200.Milliseconds();
        //                WaitTests.AssertTimeoutWithinThreashold(startTime, endTime, threshold, "WaitForDisappear");
        //            }
        //        }

        //        [TestMethod]
        //        public void BrowserElementImplementsIWrapsElement()
        //        {
        //            const string pageSource = @"
        //<html>
        //<body>
        //<span>Hello</span>
        //</body>
        //</html>";

        //            using (var browser = OpenBrowserWithPage(pageSource))
        //            {
        //                IWrapsElement span = browser.WaitForElement(By.TagName("span"), "span");
        //                var expectedType = browser.GetWebDriver().FindElement(By.TagName("span")).GetType();
        //                Assert.IsInstanceOfType(span.WrappedElement, expectedType);
        //                Assert.AreEqual("Hello", span.WrappedElement.Text);
        //            }
        //        }

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
