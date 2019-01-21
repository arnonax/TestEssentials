using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Internal;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
    /// <summary>
    /// Represents a single DOM element in a browser
    /// </summary>
    /// <remarks>
    /// This class wraps Selenium's <see cref="IWebElement"/> to provide additional capabilities for logging, automatic waiting and more.
    /// </remarks>
    public class BrowserElement : ElementsContainer, IWebElement, IWrapsElement
    {
        private readonly IDOMRoot _domRoot;

        /// <summary>
        /// Returns the root of the DOM that contains this object. This can be the browser, the containing frame or a window.
        /// </summary>
        public override IDOMRoot DOMRoot
        {
            get { return _domRoot; }
        }

        private readonly Func<IWebElement> _getWebElement;

        internal /*private */BrowserElement(IDOMRoot domRoot, Func<IWebElement> getWebElement, string description)
            : base(description)
        {
            _domRoot = domRoot;
            _getWebElement = getWebElement;
        }

        //	/// <summary>
        //	/// Constructs a new instance of <see cref="BrowserElement"/> by copying its properties, except of its description from another element
        //	/// </summary>
        //	/// <param name="otherElement">The existing element to copy its properties from</param>
        //	/// <param name="description">The new description to use for the new object</param>
        //	/// <exception cref="ArgumentNullException"><paramref name="otherElement"/> is null</exception>
        //	protected BrowserElement(BrowserElement otherElement, string description)
        //           : this(SafeGetDOMRoot(otherElement, "otherElement"), () => otherElement.WebElement, description)
        //	{
        //	}

        private static IDOMRoot SafeGetDOMRoot(ElementsContainer otherElement/*, string paramName*/)
        {
            //if (otherElement == null)
            //    throw new ArgumentNullException(paramName);

            return otherElement.DOMRoot;
        }

        //	/// <summary>
        //	/// Copy constructor
        //	/// </summary>
        //	/// <param name="otherElement">Other object from which to copy the properties into the new object</param>
        //	protected BrowserElement(BrowserElement otherElement)
        //        : this(otherElement, otherElement.Description)
        //    {
        //    }

        //	/// <summary>
        //	/// Initializes a new instance of <see cref="BrowserElement"/> given its container, a 'By' filter a selector and description
        //	/// </summary>
        //	/// <param name="container">The container that contains the relevant element. Typically this is a <see cref="Browser"/>, <see cref="BrowserWindow"/>, <see cref="Frame"/> or a containing <see cref="BrowserElement"/></param>
        //	/// <param name="by">A filter mechanism used to filter the matching elements</param>
        //	/// <param name="selector">A delegate that is used to select the sepecific element from the filtered elements</param>
        //	/// <param name="description">The description of the new element</param>
        //	/// <exception cref="ArgumentNullException">Any of the arguments is null</exception>
        //	public BrowserElement(ElementsContainer container, By by, Func<IReadOnlyCollection<IWebElement>, IWebElement> selector, string description)
        //		: this(SafeGetDOMRoot(container, "container"), () => GetWebElement(container, by, selector, description), description)
        //	{
        //	}

        /// <summary>
        /// Initialized a new instance of <see cref="BrowserElement"/> given its container, a specific 'By' filter and description
        /// </summary>
        ///// <param name="container">The container that contains the relevant element. Typically this is a <see cref="Browser"/>, <see cref="BrowserWindow"/>, <see cref="Frame"/> or a containing <see cref="BrowserElement"/></param>
        ///// <param name="by">A filter mechanism used to find the element. If multiple elements match the filter, the first one is used</param>
        ///// <param name="description">The description of the new element</param>
        ///// <exception cref="ArgumentNullException">Any of the arguments is null</exception>
        public BrowserElement(ElementsContainer container, By by, string description)
            : this(SafeGetDOMRoot(container/*, "container"*/), () => container.GetSearchContext().FindElement(by), description)
        {
        }

        //	private static IWebElement GetWebElement(ElementsContainer elementsContainer, By by, Func<IReadOnlyCollection<IWebElement>, IWebElement> selector, string description)
        //	{
        //		if (elementsContainer == null)
        //			throw new ArgumentNullException("elementsContainer");
        //		if (by == null)
        //			throw new ArgumentNullException("by");
        //		if (selector == null)
        //			throw new ArgumentNullException("selector");

        //		var searchContext = elementsContainer.GetSearchContext();
        //		var matchingElements = searchContext.FindElements(by);
        //		var selectedElement = selector(matchingElements);
        //		if (selectedElement == null)
        //			throw new NoSuchElementException(string.Format("Element '{0}' could not be found or is not available", description));

        //		return selectedElement;
        //	}

        void IWebElement.Clear()
        {
            WebElement.Clear();
        }

        void IWebElement.SendKeys(string text)
        {
            WebElement.SendKeys(text);
        }

        void IWebElement.Submit()
        {
            WebElement.Submit();
        }

        /// <summary>Clicks this element.</summary>
        /// <exception cref="T:OpenQA.Selenium.ElementNotVisibleException">Thrown when the target element is not visible.</exception>
        /// <exception cref="T:OpenQA.Selenium.StaleElementReferenceException">Thrown when the target element is no longer valid in the document DOM.</exception>
        /// <remarks>
        /// See further remarks of <seealso cref="IWebElement.Click"/>
        /// </remarks>
        public void Click()
        {
            Logger.WriteLine("Click on '{0}'", Description);
            WebElement.Click();
        }

        // Note: XML comments copies from base (using Resharper)
        /// <summary>
        /// Gets the value of the specified attribute for this element.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute's current value. Returns a <see langword="null" /> if the
        /// value is not set.</returns>
        /// <remarks>The <see cref="M:OpenQA.Selenium.IWebElement.GetAttribute(System.String)" /> method will return the current value
        /// of the attribute, even if the value has been modified after the page has been
        /// loaded. Note that the value of the following attributes will be returned even if
        /// there is no explicit attribute on the element:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute name</term>
        ///         <term>Value returned if not explicitly specified</term>
        ///         <term>Valid element types</term>
        ///     </listheader>
        ///     <item>
        ///         <description>checked</description>
        ///         <description>checked</description>
        ///         <description>Check Box</description>
        ///     </item>
        ///     <item>
        ///         <description>selected</description>
        ///         <description>selected</description>
        ///         <description>Options in Select elements</description>
        ///     </item>
        ///     <item>
        ///         <description>disabled</description>
        ///         <description>disabled</description>
        ///         <description>Input and other UI elements</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <exception cref="T:OpenQA.Selenium.StaleElementReferenceException">Thrown when the target element is no longer valid in the document DOM.</exception>
        public string GetAttribute(string attributeName)
        {
            return WebElement.GetAttribute(attributeName);
        }

        string IWebElement.GetProperty(string propertyName)
        {
            return WebElement.GetProperty(propertyName);
        }

        string IWebElement.GetCssValue(string propertyName)
        {
            return WebElement.GetCssValue(propertyName);
        }

        string IWebElement.TagName
        {
            get { return WebElement.TagName; }
        }

        /// <summary>
        ///  Gets or sets the text of this element
        /// </summary>
        /// <exception cref="StaleElementReferenceException">Thrown when the target element is no longer valid in the document DOM</exception>
        /// <remarks>The setter of this property can be used instead of <see cref="IWebElement.SendKeys"/>. However, in addition to <see cref="IWebElement.SendKeys"/>
        /// it first clears the current content of the element and also logs the typeing to the Log using <see cref="Logger"/></remarks>
        public string Text
        {
            get { return WebElement.Text; }
            set
            {
                Logger.WriteLine("Type '{0}' in '{1}'", value, Description);
                WebElement.Clear();
                WebElement.SendKeys(value);
            }
        }

        /// <summary>
        /// Determines whether the element is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return WebElement.Enabled;
            }
        }

        /// <summary>
        /// Returns whether the element is seleted.
        /// </summary>
        /// <remarks>
        /// This operation only applies to input elements such as checkboxes, options in a select element and radio buttons.
        /// </remarks>
        public bool Selected
        {
            get
            {
                return WebElement.Selected;
            }
        }

        /// <inheritdoc />
        public Point Location
        {
            get
            {
                return WebElement.Location;
            }
        }

        /// <inheritdoc />
        public Size Size
        {
            get
            {
                return WebElement.Size;
            }
        }

        /// <inheritdoc />
        public bool Displayed
        {
            get
            {
                try
                {
                    return WebElement.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return false; // the element is removed from the DOM and therefore it's not displayed.
                }
            }
        }

        private IWebElement WebElement
        {
            get
            {
                //DOMRoot.Activate();
                try
                {
                    return _getWebElement();
                }
                catch (Exception ex)
                // we catch all exceptions here, because _getWebElement may throw different exceptions if the element could not be found after the first time.
                {
                    // If we can't find the element now, even though we first found it (when the constructor is called), it most probably means that it was removed from the DOM
                    throw new StaleElementReferenceException(
                        string.Format("Failed to locate element '{0}' (after it was already found before)", Description), ex);
                }
            }
        }

        //	private bool IsReady()
        //	{
        //		var result = WebElement.Displayed && WebElement.Enabled;
        //		return result;
        //	}

        /// <summary>
        /// Performs a double-click on the element
        /// </summary>
        public void DoubleClick()
        {
            //		Wait.Until(IsReady, 30.Seconds(), string.Format("{0} is not ready", Description));

            Logger.WriteLine("Double click on '{0}'", Description);
            var actions = CreateActionsSequence();
            var doublClick = actions/*.MoveToElement(WebElement)*/.DoubleClick(WebElement).Build();
            doublClick.Perform();
        }

        private Actions CreateActionsSequence()
        {
            return new Actions(DOMRoot.Browser.GetWebDriver());
        }

        IWebElement ISearchContext.FindElement(By by)
        {
            return WebElement.FindElement(by);
        }

        ReadOnlyCollection<IWebElement> ISearchContext.FindElements(By by)
        {
            return WebElement.FindElements(by);
        }

        /// <inheritdoc />
        protected internal override ISearchContext GetSearchContext()
        {
            return WebElement;
        }

        //	protected internal override void Activate()
        //	{
        //		DOMRoot.Activate();
        //	}

        //	internal static bool IsAvailable(IWebElement el)
        //    {
        //        try
        //        {
        //            return el.Displayed;
        //        }
        //        catch (StaleElementReferenceException)
        //        {
        //               Logger.WriteLine("Warning: Stale element is caught");
        //            return false;
        //        }
        //    }

        /// <summary>
        /// Hovers the mouse over the element
        /// </summary>
        public void Hover()
        {
            Logger.WriteLine("Move to '{0}'", Description);
            var action = CreateActionsSequence();
            var moveToElement = action.MoveToElement(WebElement).Build();
            moveToElement.Perform();
        }

        /// <summary>
        /// Returns the immediate parent element containing the current element
        /// </summary>
        /// <param name="description">The description to give to the parent element</param>
        /// <returns>A <see cref="BrowserElement"/> that represents the parent element</returns>
        public BrowserElement GetParent(string description)
        {
            return new BrowserElement(this, By.XPath(".."), description);
        }

        //	internal static By GetParentLocator()
        //	{
        //		return By.XPath("..");
        //  }

        //	// TODO: check if this method can be removed?
        //	internal IWebElement GetWebElement()
        //    {
        //        return WebElement;
        //    }

        /// <summary>
        /// Drags the current element onto the target element
        /// </summary>
        /// <param name="target">The target element to drop the current element onto</param>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        [Obsolete("This method doesn't work with GeckoDriver right now. If this will be implemented by that driver" +
            "in the future, this Obsolete warning should be removed")]
        public void DragAndDrop(BrowserElement target)
        {
            //		if (target == null)
            //			throw new ArgumentNullException("target");

            //           Logger.WriteLine("Drag element '{0}' to '{1}'", Description, target.Description);
                       var targetWebElement = target.WebElement;
            var action = CreateActionsSequence();
            var dragAndDrop = action./*MoveToElement(WebElement).*/DragAndDrop(WebElement, target).Build();
            dragAndDrop.Perform();
        }

        /// <summary>
        /// Waits for the current element to disappear. That is, either become invisible or completely removed from the DOM
        /// </summary>
        /// <param name="seconds">Timeout in seconds to wait for the element to disappear</param>
        /// <exception cref="TimeoutException">The current element hasn't been disappeared for the specified period</exception>
        public void WaitToDisappear(int seconds = DefaultWaitTimeout)
        {
            WaitToDisappear(seconds.Seconds());
        }

        /// <summary>
        /// Waits for the current element to disappear. That is, either become invisible or completely removed from the DOM
        /// </summary>
        /// <param name="timeout">Timeout to wait for the element to disappear</param>
        /// <exception cref="TimeoutException">The current element hasn't been disappeared for the specified period</exception>
        public void WaitToDisappear(TimeSpan timeout)
        {
            Wait.While(() => Displayed, timeout, "Element '{0}' still appears after '{1}'", Description, timeout.ToSpokenString());
        }

        #region IWrapsElement Members

        IWebElement IWrapsElement.WrappedElement
        {
            get { return WebElement; }
        }

        #endregion
    }
}