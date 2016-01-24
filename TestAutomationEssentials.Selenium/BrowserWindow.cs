using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
	public class BrowserWindow : ElementsContainer, IDOMRoot
	{
		private readonly Browser _browser;
		private readonly string _windowHandle;

		public BrowserWindow(Browser browser, string windowHandle, string description)
			: base(description)
		{
			_browser = browser;
			_windowHandle = windowHandle;
		}

		protected internal override void Activate()
		{
			if (_browser.ActiveDOM == this)
				return;

			Logger.WriteLine("Switching to window '{0}'", Description);
			_browser.GetWebDriver().SwitchTo().Window(_windowHandle);
			_browser.ActiveDOM = this;
		}

		public override IDOMRoot DOMRoot
		{
			get { return this; }
		}

		public Browser Browser
		{
			get { return _browser; }
		}

		public string Title
		{
			get
			{
				Activate();
				return Browser.GetWebDriver().Title;
			}
		}

		public string Url
		{
			get
			{
				Activate();
				return Browser.GetWebDriver().Url;
			}
			set
			{
				Activate();
				Browser.GetWebDriver().Url = value;
			}
		}

		public void Close()
		{
			var webDriver = _browser.GetWebDriver();

			using (Logger.StartSection("Closing '{0}' Window, with id={1} ({2})", Description, _windowHandle.GetHashCode(), _windowHandle))
			{
				if (!webDriver.WindowHandles.Contains(_windowHandle))
				{
					Logger.WriteLine("Window '{0}' is already closed", Description);
					return;
				}

				Activate();
				webDriver.Close();
				Logger.WriteLine("Window '{0}' closed", Description);
			}
		}

		void IDOMRoot.Activate()
		{
			Activate();
		}

		public void NavigateToUrl(string url)
		{
			Logger.WriteLine("Navigating to '{0}' on '{1}' window", url, Description);
			Activate();
			Browser.GetWebDriver().Navigate().GoToUrl(url);
		}
	}
}