using System;
using System.Linq;
using JetBrains.Annotations;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.Common.ExecutionContext;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Selenium
{
	/// <summary>
	/// Represents an instance of a browser
	/// </summary>
	public class Browser : ElementsContainer, IDisposable, IDOMRoot
	{
		/// <summary>
		/// Provides access to the underlying <see cref="IWebDriver"/>
		/// </summary>
		protected readonly IWebDriver WebDriver;
		//internal IDOMRoot ActiveDOM;
	    //private readonly TestExecutionScopesManager _testExecutionScopesManager;

        /// <summary>
        /// Initializes the instance of the object using the specified description and <see cref="IWebDriver"/>
        /// </summary>
        /// <param name="description">The description of the browser. This is used for logging</param>
        /// <param name="webDriver">The WebDriver instance that is used to communicate with the browser</param>
        /// <exception cref="ArgumentNullException">one of the arguments are null</exception>
        /// <remarks>
        /// This overload is provided only for backward compatibility and it works only with MSTest V1.
        /// </remarks>
        [Obsolete("Always use the overload that accepts TestExecutionScopesManager.")]
        public Browser(string description, IWebDriver webDriver) : 
            this(description, webDriver, TestBase.TestExecutionScopesManager)
		{
		}

        /// <summary>
        /// Initializes the instance of the object using the specified description and <see cref="IWebDriver"/>
        /// </summary>
        /// <param name="description">The description of the browser. This is used for logging</param>
        /// <param name="webDriver">The WebDriver instance that is used to communicate with the browser</param>
        /// <param name="testExecutionScopesManager">The test execution scope manager of your tests (See remarks)</param>
        /// <exception cref="ArgumentNullException">one of the arguments are null</exception>
        /// <remarks>
        /// The <paramref name="testExecutionScopesManager"/> is used to automatically close any windows that 
        /// are opened using <see cref="OpenWindow(System.Action,string)"/>, at the end of the current test or active scope.
        /// <br/>
        /// If you're using TestAutomationEssentials.MSTest or TestAutomationEssentials.MSTestV2, simply pass
        /// <see cref="TestExecutionScopesManager"/>. Otherwise, create an instance of <see cref="TestExecutionScopesManager"/>
        /// and pass it.
        /// </remarks>
        public Browser(string description, IWebDriver webDriver, TestExecutionScopesManager testExecutionScopesManager) 
            : base(description)
	    {
	        if (webDriver == null)
	            throw new ArgumentNullException("webDriver");
            //if (testExecutionScopesManager == null)
                //throw new ArgumentNullException("testExecutionScopesManager");

			WebDriver = webDriver;
			var mainWindowHandle = WebDriver.CurrentWindowHandle;
			MainWindow = new BrowserWindow(this, mainWindowHandle/*, "Main window"*/);
			//ActiveDOM = MainWindow;
	        //_testExecutionScopesManager = testExecutionScopesManager;
	    }

        /// <summary>
        /// Returns the browser window that was activew when the browser was opened
        /// </summary>
        public BrowserWindow MainWindow { get; private set; }

        /// <summary>
        /// Always returns itself
        /// </summary>
        public override IDOMRoot DOMRoot
        {
            get
            {
                //CheckDisposed();
                return this;
            }
        }

        Browser IDOMRoot.Browser
        {
            get { return this; }
        }

	    internal bool IsDisposed { get; private set; }

	    /// <summary>
        /// Navigates the main window to the specified url
        /// </summary>
        /// <param name="url">The url to navigate to</param>
        public void NavigateToUrl(string url)
        {
            //CheckDisposed();

            MainWindow.NavigateToUrl(url);
        }

        /// <summary>
        /// Returns the underlying IWebDriver object
        /// </summary>
        /// <returns></returns>
        public IWebDriver GetWebDriver()
        {
            //CheckDisposed();
            return WebDriver;
        }

        /// <summary>
        /// Closes the Selenium driver
        /// </summary>
        public void Dispose()
        {
            //if (!_isDisposed)
                WebDriver.Quit();

            IsDisposed = true;
        }

	    /// <summary>
	    /// Invokes a delegate that causes a new window to open, and return an object representing the new window
	    /// </summary>
	    /// <param name="action">The delegate that should cause a new window to open</param>
	    /// <param name="windowDescription">A description that will identify the window in the log</param>
	    /// <returns>The <see cref="BrowserWindow"/> object that represent the newly opened window</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="action"/> or <paramref name="windowDescription"/> are null</exception>
	    /// <exception cref="TimeoutException">A new window wasn't opened for 60 seconds after the delegate completed</exception>
	    /// <remarks>
	    /// When the current <see cref="IIsolationScope"/> ends, the window is automatically closed
	    /// </remarks>
	    /// <example>
	    /// <code>
	    /// var openNewWindowButton = myBrowser.WaitForElement(By.Id("openNewWindowButtonId"), "Open new window button");
	    /// var newWindow = myBrowser.OpenWindow(() => openNewButton.Click(), "New window");
	    /// Assert.AreEqual("New window Title", newWindow.Title);
	    /// </code>
	    /// </example>
	    public BrowserWindow OpenWindow(Action action, string windowDescription)
	    {
	        return OpenWindow(action, windowDescription, 1.Minutes());
	    }

	    /// <summary>
	    /// Invokes a delegate that causes a new window to open, and return an object representing the new window
	    /// </summary>
	    /// <param name="action">The delegate that should cause a new window to open</param>
	    /// <param name="windowDescription">A description that will identify the window in the log</param>
	    /// <param name="timeout">The maximal time to wait for the window to open</param>
	    /// <returns>The <see cref="BrowserWindow"/> object that represent the newly opened window</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="action"/> or <paramref name="windowDescription"/> are null</exception>
	    /// <exception cref="TimeoutException">A new window wasn't opened for the specified timeout after the delegate completed</exception>
	    /// <remarks>
	    /// When the current <see cref="IIsolationScope"/> ends, the window is automatically closed
	    /// </remarks>
	    /// <example>
	    /// <code>
	    /// var openNewWindowButton = myBrowser.WaitForElement(By.Id("openNewWindowButtonId"), "Open new window button");
	    /// var newWindow = myBrowser.OpenWindow(() => openNewButton.Click(), "New window");
	    /// Assert.AreEqual("New window Title", newWindow.Title);
	    /// </code>
	    /// </example>
	    public BrowserWindow OpenWindow([InstantHandle]Action action, string windowDescription, TimeSpan timeout)
        {
            //	CheckDisposed();
            //	if (action == null)
            //		throw new ArgumentNullException("action");
            //	if (windowDescription == null)
            //		throw new ArgumentNullException("windowDescription");

            //          Activate();
            //	var webDriver = GetWebDriver();
            //	var existingHandles = webDriver.WindowHandles;
            //	action();

            //	var newWindowHandle = Wait.Until(() => webDriver.WindowHandles.Except(existingHandles).SingleOrDefault(),
            //		handle => handle != null,
            //		60.Seconds(), "Window '{0}' wasn't opened for 60 seconds", windowDescription);
            //	Logger.WriteLine("Opened window '{0}' with id={1} ({2})", windowDescription, newWindowHandle.GetHashCode(), newWindowHandle);

            // TODO: consider this implementation vs. the one above.
            string newWindowHandle;
            try
            {
                newWindowHandle = new PopupWindowFinder(WebDriver, timeout).Invoke(action);
            }
            catch (WebDriverTimeoutException)
            {
                // For backward compatibility
                throw new TimeoutException();
            }
            // END TODO

            var newWindow = new BrowserWindow(this, newWindowHandle/*, windowDescription*/);
            TestBase.AddCleanupAction(() => newWindow.Close());

            return newWindow;
		}

        //private void CheckDisposed()
        //{
        //	if (_isDisposed)
        //		throw new ObjectDisposedException("Browser object has been disposed");
        //}

        //protected internal sealed override void Activate()
        //{
        //	MainWindow.Activate();
        //}

        void IDOMRoot.Activate()
        {
            //CheckDisposed();
            //Activate();
        }
	}
}