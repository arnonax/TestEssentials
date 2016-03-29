using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
	public class BrowserElement : ElementsContainer, IWebElement
	{
		private readonly IDOMRoot _domRoot;

		public override IDOMRoot DOMRoot
		{
			get { return _domRoot; }
		}

		private readonly Func<IWebElement> _getWebElement;

		private BrowserElement(IDOMRoot domRoot, Func<IWebElement> getWebElement, string description)
			: base(description)
		{
			_domRoot = domRoot;
			_getWebElement = getWebElement;
		}

		protected BrowserElement(BrowserElement otherElement, string description)
            : this(otherElement.DOMRoot, () => otherElement.WebElement, description)
		{
		}

	    protected BrowserElement(BrowserElement otherElement)
	        : this(otherElement, otherElement.Description)
	    {
	    }

		public BrowserElement(ElementsContainer elementsContainer, By @by, Func<IReadOnlyCollection<IWebElement>, IWebElement> selector, string description)
			: this(elementsContainer.DOMRoot, () => GetWebElement(elementsContainer, @by, selector), description)
		{
		}

		public BrowserElement(ElementsContainer parent, By @by, string description)
			: this(parent.DOMRoot, () => parent.GetSearchContext().FindElement(@by), description)
		{
		}

		private static IWebElement GetWebElement(ElementsContainer container, By @by, Func<IReadOnlyCollection<IWebElement>, IWebElement> selector)
		{
			var searchContext = container.GetSearchContext();
			var matchingElements = searchContext.FindElements(@by);
			return selector(matchingElements);
		}

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

		public void Click()
		{
			Logger.WriteLine("Click on '{0}'", Description);
            //Actions action = new Actions(_domRoot.Browser.GetWebDriver());
            //action.MoveToElement(WebElement, 0, 0).Perform();
            //action.Click(WebElement).Perform();
            WebElement.Click();
		}

		public string GetAttribute(string attributeName)
		{
			return WebElement.GetAttribute(attributeName);
		}

		string IWebElement.GetCssValue(string propertyName)
		{
			return WebElement.GetCssValue(propertyName);
		}

		string IWebElement.TagName
		{
			get { return WebElement.TagName; }
		}

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

		public bool Enabled
		{
			get { return WebElement.Enabled; }
		}

		public bool Selected
		{
			get { return WebElement.Selected; }
		}

		public Point Location
		{
			get { return WebElement.Location; }
		}

		public Size Size
		{
			get { return WebElement.Size; }
		}

		public bool Displayed
		{
			get
			{
				try
				{
				    var webElement = WebElement;
                    // WebElement may return null if the element is not displayed
				    return webElement != null && WebElement.Displayed;
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
				DOMRoot.Activate();
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

		private bool IsReady()
		{
			var result = WebElement.Displayed && WebElement.Enabled;
			return result;
		}

		public void DoubleClick()
		{
			Wait.Until(IsReady, 30.Seconds(), string.Format("{0} is not ready", Description));

			Logger.WriteLine("Double click on '{0}'", Description);
			var actions = CreateActionsSequence();
			var doublClick = actions.MoveToElement(WebElement).DoubleClick(WebElement).Build();
			doublClick.Perform();
		}

		private Actions CreateActionsSequence()
		{
			return new Actions(DOMRoot.Browser.GetWebDriver());
		}

		IWebElement ISearchContext.FindElement(By @by)
		{
			return WebElement.FindElement(by);
		}

		ReadOnlyCollection<IWebElement> ISearchContext.FindElements(By @by)
		{
			return WebElement.FindElements(by);
		}

		// TODO: check why this method is needed (why not use WaitForElement instead?)
	    public BrowserElement FindElement(By @by, string description)
        {
		    WebElement.FindElement(@by);
		    return new BrowserElement(this, by, description);
        }

		protected internal override ISearchContext GetSearchContext()
		{
			return WebElement;
		}

		protected internal override void Activate()
		{
			DOMRoot.Activate();
		}

		internal static bool IsAvailable(IWebElement el)
	    {
	        try
	        {
	            return el.Displayed;
	        }
	        catch (StaleElementReferenceException)
	        {
                Logger.WriteLine("Warning: Stale element is caught");
	            return false;
	        }
	    }

	    public void Hover()
	    {
            Logger.WriteLine("Move to '{0}'", Description);
		    var action = CreateActionsSequence();
	        var moveToElement = action.MoveToElement(WebElement).Build();
            moveToElement.Perform();
	    }

	    public BrowserElement GetParent(string description)
	    {
			return new BrowserElement(this, GetParentLocator(), description);
	    }

		internal static By GetParentLocator()
		{
			return By.XPath("..");
	    }

		// TODO: check if this method can be made internal? First, this class already implements IWebElement. 2nd, the methods that are explicitly implement the interface are intentionally "hidden" because they shouldn't be used directly in most cases.
	    public IWebElement GetWebElement()
	    {
	        return WebElement;
	    }

        public void DragAndDrop(BrowserElement target)
        {
            Logger.WriteLine("Drag element '{0}' to '{1}'", Description, target.Description);
            var targetWebElement = target.GetWebElement();
	        var action = CreateActionsSequence();
            var dragAndDrop = action.MoveToElement(WebElement).DragAndDrop(WebElement, targetWebElement).Build();
            dragAndDrop.Perform();
        }

        public void WaitToDisappear(int seconds = DefaultWaitTimeout)
        {
            Wait.While(() =>
            {
                var element = WebElement;
                return element != null && element.Displayed;
            }, seconds.Seconds(), "Element '{0}' still appears after '{1}' seconds", Description, seconds);
        }
	}
}