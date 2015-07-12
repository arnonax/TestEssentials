using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.CodedUI
{
	/// <summary>
	/// Provides useful extension methods for identifying controls using Coded UI
	/// </summary>
	public static class CodedUIExtensions
	{
		/// <summary>
		/// Finds all child controls from the specified type
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child controls to find</typeparam>
		/// <param name="parent">The control in which to search for the relevant children</param>
		/// <returns>A collection containing the children that matches the specified type</returns>
		public static IEnumerable<TTestControl> FindChildren<TTestControl>(this UITestControl parent)
			where TTestControl : UITestControl
		{
			return parent.FindChildren<TTestControl>(null);
		}

		/// <summary>
		/// Finds all child controls from the specified type and criteria
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child controls to find</typeparam>
		/// <param name="parent">The control in which to search for the relevent children</param>
		/// <param name="by">The criteria to use for filtering the relevant children. Use the methods in the <see cref="By"/> class to create the relevant criteria</param>
		/// <returns>A collection containing the children that matches the specified type and criteria</returns>
		public static IEnumerable<TTestControl> FindChildren<TTestControl>(this UITestControl parent, PropertyExpression by)
			where TTestControl : UITestControl
		{
			var testControl = parent.Get<TTestControl>(@by);
			return testControl.FindMatchingControls().Cast<TTestControl>();
		}

		public static TTestControl Find<TTestControl>(this UITestControl parent) 
			where TTestControl : UITestControl
		{
			return parent.Find<TTestControl>(null);
		}

		public static TTestControl Find<TTestControl>(this UITestControl parent, PropertyExpression by) 
			where TTestControl : UITestControl
		{
			var control = parent.Get<TTestControl>(@by);
			control.SearchConfigurations.Add(SearchConfiguration.AlwaysSearch);
			var visibleMatchingControls = control.FindMatchingControls().Where(IsVisible).ToList();

			if (visibleMatchingControls.Count == 0)
				Assert.Fail("No control found that matches the specified criteria: {0} for parent: {1}", @by.TryGet(x => x.ToString()), parent);
			else if (visibleMatchingControls.Count > 1)
				Assert.Fail("{0} controls were found matching the specified criteria: {1} for parent: {2}", visibleMatchingControls.Count, @by.TryGet(x => x.ToString()), parent);

			var result = (TTestControl)visibleMatchingControls.Content();
			return result;
		}

		public static TTestControl Get<TTestControl>(this UITestControl parent)
			where TTestControl : UITestControl
		{
			return parent.Get<TTestControl>(null);
		}

		public static TTestControl Get<TTestControl>(this UITestControl parent, PropertyExpression by)
			where TTestControl : UITestControl
		{
			var result = (TTestControl) Activator.CreateInstance(typeof (TTestControl), parent);
			
			if (@by != null)
				result.SearchProperties.Add(@by);

			return result;
		}

		public static WpfTreeItem GetChild(this WpfTreeItem parent, string name)
		{
			parent.Expanded = true;
			var nodes = parent.Nodes;
			return GetChild(nodes, name);
		}

		public static WpfTreeItem GetChild(this WpfTree parent, string name)
		{
			return GetChild(parent.Nodes, name);
		}

		private static WpfTreeItem GetChild(UITestControlCollection nodes, string name)
		{
			return nodes.OfType<WpfTreeItem>().Find(treeItem => new WpfText(treeItem).Name == name);
		}

		public static void Click(this UITestControl control)
		{
			Mouse.Click(control);
		}

		public static string GetValue(this UITestControl control)
		{
			return control.GetProperty("Value") as string;
		}

		public static bool IsVisible(this UITestControl control)
		{
			return control.Exists && (ControlStates)control.GetProperty("State") != ControlStates.Offscreen;
		}
	}
}