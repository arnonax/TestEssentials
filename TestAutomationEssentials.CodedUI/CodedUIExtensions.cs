using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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

		/// <summary>
		/// Finds a single control from the specified type
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child control to find</typeparam>
		/// <param name="parent">The control in which to search the relevant child</param>
		/// <returns>The child control that matches the matches the specified type</returns>
		/// <exception cref="AssertFailedException">The parent control either has no child of the specified type, or it has more than one</exception>
		public static TTestControl Find<TTestControl>(this UITestControl parent) 
			where TTestControl : UITestControl
		{
			return parent.Find<TTestControl>(null);
		}

		/// <summary>
		/// Finds a single control from the specified type and criteria
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child control to find</typeparam>
		/// <param name="parent">The control in which to search for the specified child</param>
		/// <param name="by">The criteria to use for finding the relevant child. Use the methods in the <see cref="By"/> class to create the relevant criteria</param>
		/// <returns>The child control that matches the specified type and criteria</returns>
		/// <exception cref="AssertFailedException">The parent control has no child that matches the specified type and criteria, or it has more than one</exception>
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

		/// <summary>
		/// Returns an object that represents a potential control from the specified type.
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child control</typeparam>
		/// <param name="parent">The control that contains the specified child should it exist</param>
		/// <returns>An object that represents the potential control</returns>
		/// <remarks>The returned object may or may not exist, or might be invisible at the time this method is called. 
		/// You can use this object to check if the control exist or visible, and you can also use this object with any Coded UI API that uses a UITestControl object</remarks>
		public static TTestControl Get<TTestControl>(this UITestControl parent)
			where TTestControl : UITestControl
		{
			return parent.Get<TTestControl>(null);
		}

		/// <summary>
		/// Returns and object that represents a potential control from the specified type and criteria
		/// </summary>
		/// <typeparam name="TTestControl">The type of the child control</typeparam>
		/// <param name="parent">The control that contains the specified child should it exist</param>
		/// <param name="by">The criteria that will be used to look for the child control</param>
		/// <returns>An object that represents the potential control that matches the criteria</returns>
		/// <remarks>The returned object may or may not exist, or might be invisible at the time this method is called. 
		/// You can use this object to check if the control exist or visible, and you can also use this object with any Coded UI API that uses a UITestControl object</remarks>
		public static TTestControl Get<TTestControl>(this UITestControl parent, PropertyExpression by)
			where TTestControl : UITestControl
		{
			var result = (TTestControl) Activator.CreateInstance(typeof (TTestControl), parent);
			
			if (@by != null)
				result.SearchProperties.Add(@by);

			return result;
		}

		/// <summary>
		/// Returns the child element of a WpfTreeItem that has the specified name
		/// </summary>
		/// <param name="parent">The element in the tree in which to look for the child element</param>
		/// <param name="name">The name of the child element</param>
		/// <returns>A <see cref="WpfTreeItem"/> that represents the found child</returns>
		/// <exception cref="InvalidOperationException">The parent element does not have a child with specified name, or it contain more than one</exception>
		public static WpfTreeItem GetChild(this WpfTreeItem parent, string name)
		{
			parent.Expanded = true;
			var nodes = parent.Nodes;
			return GetChild(nodes, name);
		}

		/// <summary>
		/// Returns the root element of a WpfTree that has the specified name
		/// </summary>
		/// <param name="parent">The tree control</param>
		/// <param name="name">The name of the root element to look for</param>
		/// <returns>A <see cref="WpfTreeItem"/> that represents the found root element</returns>
		/// <exception cref="InvalidOperationException">The tree control does not have a root element with specified name, or it contain more than one</exception>
		public static WpfTreeItem GetChild(this WpfTree parent, string name)
		{
			return GetChild(parent.Nodes, name);
		}

		private static WpfTreeItem GetChild(UITestControlCollection nodes, string name)
		{
			return nodes.OfType<WpfTreeItem>().Find(treeItem => new WpfText(treeItem).Name == name);
		}

		/// <summary>
		/// Clicks on the specified control
		/// </summary>
		/// <param name="control">The control to click</param>
		public static void Click(this UITestControl control)
		{
			Mouse.Click(control);
		}

		/// <summary>
		/// Drags one control on to another
		/// </summary>
		/// <param name="draggedControl">The control to drag</param>
		/// <param name="destination">The destination control, where the dragged control should be dropped</param>
		public static void DragTo(this UITestControl draggedControl, UITestControl destination)
		{
			Mouse.StartDragging(draggedControl);
			Mouse.StopDragging(destination);
		}

		/// <summary>
		/// Right-clicks on the specified control
		/// </summary>
		/// <param name="control">The control to right-click</param>
		public static void RightClick(this UITestControl control)
		{
			Mouse.Click(control, MouseButtons.Right);
		}

		/// <summary>
		/// Double-clicks on the specified control
		/// </summary>
		/// <param name="control">The control to double-click</param>
		public static void DoubleClick(this UITestControl control)
		{
			Mouse.DoubleClick(control);
		}

		/// <summary>
		/// Determines whether the speciied control is visible
		/// </summary>
		/// <param name="control">The control to check</param>
		/// <returns><b>true</b> if the control is visible, otherwise <b>false</b></returns>
		public static bool IsVisible(this UITestControl control)
		{
			return control.Exists && (ControlStates)control.GetProperty("State") != ControlStates.Offscreen;
		}
	}
}