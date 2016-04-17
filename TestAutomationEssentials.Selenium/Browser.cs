using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Selenium
{
	/// <summary>
	/// Represents an instance of a browser
	/// </summary>
	public class Browser : ElementsContainer, IDOMRoot
	{
		/// <summary>
		/// Provides access to the underlying <see cref="IWebDriver"/>
		/// </summary>
		protected readonly IWebDriver WebDriver;
		private bool _isDisposed;
		internal IDOMRoot ActiveDOM;

		/// <summary>
		/// Initializes the instance of the object using the specified description and <see cref="IWebDriver"/>
		/// </summary>
		/// <param name="description">The description of the browser. This is used for logging</param>
		/// <param name="webDriver">The WebDriver instance that is used to communicate with the browser</param>
		/// <exception cref="ArgumentNullException">one of the arguments are null</exception>
		public Browser(string description, IWebDriver webDriver) : base(description)
		{
			if (description == null)
				throw new ArgumentNullException("description");

			if (webDriver == null)
				throw new ArgumentNullException("webDriver");

			WebDriver = webDriver;
			var mainWindowHandle = WebDriver.CurrentWindowHandle;
			MainWindow = new BrowserWindow(this, mainWindowHandle, "Main window");
			ActiveDOM = MainWindow;
		}

		/// <summary>
		/// Returns the browser window that was activew when the browser was opened
		/// </summary>
		public BrowserWindow MainWindow { get; private set; }

		internal override IDOMRoot DOMRoot
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

		/// <summary>
		/// Navigates the main window to the specified url
		/// </summary>
		/// <param name="url">The url to navigate to</param>
		public void NavigateToUrl(string url)
		{
			CheckDisposed();

			MainWindow.NavigateToUrl(url);
		}

		/// <summary>
		/// Returns the underlying IWebDriver object
		/// </summary>
		/// <returns></returns>
		public IWebDriver GetWebDriver()
		{
			CheckDisposed();
			return WebDriver;
		}

		/// <summary>
		/// Closes the Selenium driver
		/// </summary>
		public void Dispose()
		{
			if (!_isDisposed)
				WebDriver.Quit();

			_isDisposed = true;
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