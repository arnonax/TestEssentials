using OpenQA.Selenium;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
    /// <summary>
    /// Represents a HTML frame or iframe
    /// </summary>
    public class Frame : ElementsContainer, IDOMRoot
    {
        private readonly IWebDriver _webDriver;
        private readonly BrowserElement _frameElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class representing the frame element specified
        /// </summary>
        /// <param name="frameElement">The BrowserElement of the frame element</param>
        public Frame(BrowserElement frameElement)
            : base(frameElement.Description)
        {
            _frameElement = frameElement;
            var browser = _frameElement.DOMRoot.Browser;
            _webDriver = browser.GetWebDriver();
        }

        /// <inheritdoc />
        protected internal sealed override void Activate()
        {
            if (Browser.ActiveDOM == this)
                return;

            Logger.WriteLine("Switching to frame '{0}'", Description);
            _frameElement.Activate();
            SwitchToFrame();
            Browser.ActiveDOM = this;
        }

        void IDOMRoot.Activate()
        {
            Activate();
        }

        /// <summary>
        /// Returns the underlying <see cref="IWebDriver"/>
        /// </summary>
        /// <returns>The underlying <see cref="IWebDriver"/> object</returns>
        /// <remarks>
        /// Avoid calling <see cref="IWebDriver.SwitchTo"/> on the returned object. If you do, make sure to 
        /// Switch back to the same context before calling any method or property on any object derived from <see cref="ElementsContainer"/>. Failing to do so
        /// will probably cause the method or property to fail.
        /// </remarks>
        public IWebDriver GetWebDriver()
        {
            return _webDriver;
        }

        /// <summary>
        /// Returns the containing browser
        /// </summary>
        public Browser Browser
        {
            get { return _frameElement.DOMRoot.Browser; }
        }

        /// <summary>
        /// the root of the DOM that contains this object. This can be the browser, the containing frame or a window.
        /// </summary>
        public override IDOMRoot DOMRoot
        {
            get { return this; }
        }

        private void SwitchToFrame()
        {
            // TODO: check if GetWebElement() is needed here (and at all)
            _webDriver.SwitchTo().Frame(_frameElement.GetWebElement());
        }
    }
}