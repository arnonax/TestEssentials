using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
	/// <summary>
	/// Acts as a base class for all objects that can contain DOM elements
	/// </summary>
	public abstract class ElementsContainer
	{
		/// <summary>
		/// The default timeout that is used to wait for elements to appear or disappear
		/// </summary>
		public const int DefaultWaitTimeout = 30;

	    /// <summary>
	    /// Initializes the instance of <see cref="ElementsContainer"/> with the specified description
	    /// </summary>
	    /// <param name="description">A description representing the element in the log</param>
	    /// <exception cref="ArgumentNullException"><paramref name="description"/> is null</exception>
	    protected ElementsContainer(string description)
		{
			if (description == null)
				throw new ArgumentNullException("description");

			Description = description;
		}

        ///// <summary>
        ///// Returns the specified frame which is contained inside the current container
        ///// </summary>
        ///// <param name="frameName">The name of the frame</param>
        ///// <param name="description">A description representing the frame in the log</param>
        ///// <returns>An object representing the specified frame</returns>
        ///// <exception cref="ArgumentNullException">Any of the arguments is null</exception>
        ///// <exception cref="TimeoutException">The frame wasn't found after <see cref="DefaultWaitTimeout"/> seconds</exception>
        //public Frame GetFrame(string frameName, string description)
        //{
        //	if (frameName == null)
        //		throw new ArgumentNullException("frameName");
        //	if (description == null)
        //		throw new ArgumentNullException("description");

        //	var locator = By.XPath(string.Format(".//iframe[@name='{0}' or @id='{0}']", frameName));
        //	return GetFrame(locator, description);
        //}

        //protected internal abstract void Activate();

        ///// <summary>
        ///// Returns the frame that matches the 'by' criteria which is contained inside the current container
        ///// </summary>
        ///// <param name="by">The criteria that is used to locate the frame element</param>
        ///// <param name="description">A description representing the frame in the log</param>
        ///// <returns>An object representing the specified frame</returns>
        ///// <exception cref="ArgumentNullException">Any of the arguments is null</exception>
        ///// <exception cref="TimeoutException">The frame wasn't found after <see cref="DefaultWaitTimeout"/> seconds</exception>
        ///// <remarks>If more than 1 frame matches the specified criteria, the first match is returned;</remarks>
        //public Frame GetFrame(By @by, string description)
        //{
        //	if (by == null)
        //		throw new ArgumentNullException("by");
        //	if(description == null)
        //		throw new ArgumentNullException("description");

        //	var element = WaitForElement(@by, description);
        //	return new Frame(element);
        //}

        /// <summary>
        /// When implemented in a derived class, should return the root of the DOM that contains this object. This can be the browser, a containing frame or a window.
        /// </summary>
        public abstract IDOMRoot DOMRoot { get; }

        /// <summary>
        /// Finds an element, waiting for it to appear for the specified time if needed.
        /// </summary>
        /// <param name="by">The locator to use in order to find the element</param>
        /// <param name="description">The description of the element to find</param>
        /// <param name="seconds">Number of seconds to wait. If this parameter is omitted, <see cref="DefaultWaitTimeout"/> is used</param>
        /// <returns>A <see cref="BrowserElement"/> representing the found element</returns>
        /// <exception cref="TimeoutException">The element is not found after the specified number of seconds</exception>
        public BrowserElement WaitForElement(By by, string description, int seconds = DefaultWaitTimeout)
        {
            //Activate();

            var searchContext = GetSearchContext();
            Wait.Until(() => searchContext.FindElements(by).FirstOrDefault( /*BrowserElement.IsAvailable*/),
                el => el != null,
                seconds.Seconds(),
                "Element '{0}' not found inside '{1}' using '{2}' for '{3}' seconds", description, Description, by, seconds);

            return new BrowserElement(this, by, /*elements => elements.FirstOrDefault(BrowserElement.IsAvailable), */description);
        }

        /// <summary>
        /// Returns the description of the current object
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// This method is intended for internal use only. Do not call or override it directly.
        /// </summary>
        /// <returns></returns>
        protected internal virtual ISearchContext GetSearchContext()
        {
            return DOMRoot.Browser.GetWebDriver();
        }

        /// <summary>
        /// Returns all child elements that match the specified locator
        /// </summary>
        /// <param name="by">The locator to use to find the elements</param>
        /// <param name="description">A common description prefix to assign to the matching elements. The actual 
        /// description of the returned elements will have a sequential number appended to the specified description</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BrowserElement"/> containing the matching elements</returns>
        public IEnumerable<BrowserElement> FindElements(By @by, string description)
        {
            //Activate();

            return
                GetSearchContext().FindElements(by)
                    .Select(
                        (element, i) =>
                            //new BrowserElement(this, by, matches => matches.ElementAt(i), string.Format("{0}[{1}]", description, i)));
                            new BrowserElement(DOMRoot, () => element, string.Format("{0}[{1}]", description, i)));
        }

        /// <summary>
        /// Determines whether an element matching the specified locator exists and is visible
        /// </summary>
        /// <param name="by">The locator of the element</param>
        /// <returns>True if the element appears; False if not</returns>
        public bool ElementAppears(By by)
        {
            return GetSearchContext().FindElements(by).Any(el => el.Displayed);
            
        }
    }
}