using OpenQA.Selenium;

namespace TestAutomationEssentials.Selenium
{
	/// <summary>
	/// Represents an object to which you can <see cref="IWebDriver.SwitchTo"/> (except of Alert)
	/// </summary>
	public interface IDOMRoot
	{
		/// <summary>
		/// Ensures that the current object is active. Calling <see cref="IWebDriver.SwitchTo"/> to do that if neccesary.
		/// </summary>
		void Activate();
		
        /// <summary>
		/// Returns the <see cref="Browser"/> object which is the owner of this object
		/// </summary>
		Browser Browser { get; }
	}
}