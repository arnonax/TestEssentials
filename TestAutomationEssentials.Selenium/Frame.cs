using OpenQA.Selenium;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
    public class Frame : ElementsContainer, IDOMRoot
    {
        private readonly IWebDriver _webDriver;
        private readonly BrowserElement _frameElement;

        public Frame(BrowserElement frameElement)
            : base(frameElement.Description)
        {
            _frameElement = frameElement;
            var browser = _frameElement.DOMRoot.Browser;
            _webDriver = browser.GetWebDriver();
        }

        protected internal override void Activate()
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

        public IWebDriver GetWebDriver()
        {
            return _webDriver;
        }

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

        protected virtual void SwitchToFrame()
        {
            // TODO: check if GetWebElement() is needed here (and at all)
            _webDriver.SwitchTo().Frame(_frameElement.GetWebElement());
        }
    }
}