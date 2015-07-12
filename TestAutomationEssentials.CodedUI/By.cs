using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace TestAutomationEssentials.CodedUI
{
	/// <summary>
	/// Provides methods that work together with the extension methods in <see cref="CodedUIExtensions" /> to identify WPF controls
	/// </summary>
	public static class By
	{
		/// <summary>
		/// Identifies a control by its <paramref name="className"/>
		/// </summary>
		/// <param name="className">The ClassName of the control</param>
		/// <returns>A <see cref="PropertyExpression"/> that identifies the control</returns>
		public static PropertyExpression UiaClassName(string className)
		{
			return new PropertyExpression(UITestControl.PropertyNames.ClassName, "Uia." + className);
		}

		/// <summary>
		/// Identifies a control by its <paramref name="name"/>
		/// </summary>
		/// <param name="name">The Name of the control</param>
		/// <returns>A <see cref="PropertyExpression"/> that identifies the control</returns>
		public static PropertyExpression Name(string name)
		{
			return new PropertyExpression(UITestControl.PropertyNames.Name, name);
		}

		/// <summary>
		/// Identifies a control by its <paramref name="automationId"/>
		/// </summary>
		/// <param name="automationId">The Automation ID of the control</param>
		/// <returns>A <see cref="PropertyExpression"/> that identifies the control</returns>
		public static PropertyExpression AutomationId(string automationId)
		{
			return new PropertyExpression(WpfControl.PropertyNames.AutomationId, automationId);
		}
	}
}
