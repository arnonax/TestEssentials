using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.Selenium
{
	public abstract class ElementsContainer
	{
		protected ElementsContainer(string description)
		{
			Description = description;
		}

		public Frame GetFrame(string frameName, string description)
		{
			var locator = By.XPath(string.Format(".//iframe[@name='{0}' or @id='{0}']", frameName));
			return GetFrame(locator, description);
		}

		protected internal abstract void Activate();

		public Frame GetFrame(By @by, string description)
		{
			var element = WaitForElement(@by, description);
			return new Frame(element);
		}

		public abstract IDOMRoot DOMRoot { get; }

		public BrowserElement WaitForElement(By by, string description, int seconds = 30)
		{
			Activate();

			var searchContext = GetSearchContext();
			try
			{
				Wait.Until(() => searchContext.FindElements(@by).FirstOrDefault(BrowserElement.IsAvailable),
					el => el != null,
					seconds.Seconds(),
					"Element '{0}' not found inside '{1}' using '{2}' for '{3}' seconds", description, Description, @by, seconds);

				return new BrowserElement(this, @by, elements => elements.FirstOrDefault(BrowserElement.IsAvailable), description);
			}
			catch (StaleElementReferenceException)
			{
				throw new StaleElementReferenceException(
					string.Format("The element '{0}' is no longer available on inside '{1}' using '{2}'", description, Description, @by));
			}
		}

		public string Description { get; private set; }

		protected internal virtual ISearchContext GetSearchContext()
		{
			return DOMRoot.Browser.GetWebDriver();
		}

		public IEnumerable<BrowserElement> FindElements(By @by, string description)
		{
			return
				GetSearchContext().FindElements(@by)
					.Select(
						(element, i) =>
							new BrowserElement(this, @by, matches => matches.ElementAt(i), string.Format("{0}[{1}]", description, i)));
		}

        public IWebElement FindElementWithActivation(By @by, int seconds = 60)
        {
            Activate();
            var webDriver = GetSearchContext();
            return webDriver.FindElement(@by);
        }
	}
}