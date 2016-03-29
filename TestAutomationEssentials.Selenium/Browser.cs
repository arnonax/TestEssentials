using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Selenium
{
	public class Browser : ElementsContainer, IDOMRoot
	{
		protected readonly IWebDriver WebDriver;
		private bool _isDisposed;
		internal IDOMRoot ActiveDOM;

	    public Browser(string description, IWebDriver webDriver) : base(description)
		{
			WebDriver = webDriver;
			var mainWindowHandle = WebDriver.CurrentWindowHandle;
			MainWindow = new BrowserWindow(this, mainWindowHandle, "Main window");
			ActiveDOM = MainWindow;
		}

		public string Url
		{
			get
			{
				CheckDisposed();
				return WebDriver.Url;
			}
		}

		public BrowserWindow MainWindow { get; private set; }

		public override IDOMRoot DOMRoot
		{
			get
			{
				CheckDisposed();
				return this;
			}
		}

		Browser IDOMRoot.Browser
		{
			get { return this; }
		}

		public void NavigateToUrl(string url)
		{
			CheckDisposed();

			MainWindow.NavigateToUrl(url);
		}

		public IWebDriver GetWebDriver()
		{
			CheckDisposed();
			return WebDriver;
		}

		public void Dispose()
		{
			if (!_isDisposed)
				WebDriver.Quit();

			_isDisposed = true;
		}

		public IEnumerable<BrowserElement> WaitForElements(By @by, string description, int seconds = 30)
		{
			CheckDisposed();
			return
				WebDriver.FindElements(@by)
					.Select((x, i) => new BrowserElement(this, @by, matches => matches.ElementAt(i), String.Format("{0}[{1}]", description, i)));
		}

		public TimeSpan GetTimeout(int seconds = 60)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		public BrowserWindow OpenWindow(Action action, string windowDescription)
		{
			CheckDisposed();
            Activate();
			var webDriver = GetWebDriver();
			var existingHandles = webDriver.WindowHandles;
			action();

			var newWindowHandle = Wait.Until(() => webDriver.WindowHandles.Except(existingHandles).SingleOrDefault(),
				handle => handle != null,
				60.Seconds(), "Window '{0}' wasn't opened for 60 seconds", windowDescription);

			Logger.WriteLine("Opened window '{0}' with id={1} ({2})", windowDescription, newWindowHandle.GetHashCode(), newWindowHandle);

			var newWindow = new BrowserWindow(this, newWindowHandle, windowDescription);
			TestBase.AddCleanupAction(() => newWindow.Close());

			return newWindow;
		}

		protected void CheckDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException("Browser object has been disposed");
		}

		protected internal override void Activate()
		{
			MainWindow.Activate();
		}

		void IDOMRoot.Activate()
		{
			CheckDisposed();
			Activate();
		}
	}
}