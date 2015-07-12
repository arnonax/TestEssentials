using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest.ExecutionContext;

namespace TestAutomationEssentials.MSTest
{
	[TestClass]
	public abstract class TestBase : IIsolationContext
	{
		protected static bool PrepareDataPerClass;
		private static TestExecutionContext _testExuecutionContext;
		protected static readonly Dictionary<Type, TestBase> InitializedInstances = new Dictionary<Type, TestBase>();
		public TestContext TestContext { get; set; }

		[Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
		public void InitializeTest()
		{
			AssertClassInitializeIsCalled();
			Console.WriteLine("Tests started: {0}", DateTime.Now);

			CopyFields();

			try
			{
				_testExuecutionContext.PushIsolationLevel("Tests", TestInitialize);
			}
			catch(Exception ex)
			{
				OnInitializationException(ex);
				throw;
			}
		}

		protected virtual void OnInitializationException(Exception exception)
		{
			TakeScreenshot("Initialize");
		}

		private void TakeScreenshot(string name)
		{
			var filename = Path.GetFullPath(TestContext.FullyQualifiedTestClassName + "." + name + ".jpg");
			try
			{
				var screenLeft = SystemInformation.VirtualScreen.Left;
				var screenTop = SystemInformation.VirtualScreen.Top;
				var screenWidth = SystemInformation.VirtualScreen.Width;
				var screenHeight = SystemInformation.VirtualScreen.Height;

				// Create a bitmap of the appropriate size to receive the screenshot.
				using (var bmp = new Bitmap(screenWidth, screenHeight))
				{
					// Draw the screenshot into our bitmap.
					using (var g = Graphics.FromImage(bmp))
					{
						g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
					}

					bmp.Save(filename, ImageFormat.Jpeg);
				}
				TestContext.AddResultFile(filename);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An exception occured while trying to take screenshot:");
				Console.WriteLine(ex);
			}
		}

		private void AssertClassInitializeIsCalled()
		{
			var actualtype = GetType();
			var publicStaticMethods = actualtype.GetMethods(BindingFlags.Public | BindingFlags.Static);
			if (publicStaticMethods.All(x => x.GetCustomAttribute<ClassInitializeAttribute>() == null))
				Assert.Fail(@"Class {0} does not have a [ClassInitialize] method. Please add the following code to class '{0}' to fix it: 
************************************
[ClassInitialize]
public static void ClassInitialize(TestContext testContext)
{{
	ClassInitialize(typeof({1}));
}}
************************************
", actualtype, actualtype.Name);
		}

		[Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanup]
		public void CleanupTest()
		{
			_testExuecutionContext.PopIsolationLevel();
		}

		private void CopyFields()
		{
			var thisType = GetType();
			var initializedInstance = InitializedInstances[thisType];
			var fields = thisType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (var fieldInfo in fields)
			{
				var valueFromInitializedInstance = fieldInfo.GetValue(initializedInstance);
				var valueFromCurrentInstance = fieldInfo.GetValue(this);
				var defaultValue = fieldInfo.FieldType.GetDefaultValue();
				if (valueFromCurrentInstance == defaultValue && valueFromInitializedInstance != defaultValue)
					fieldInfo.SetValue(this, valueFromInitializedInstance);
			}
		}

		protected virtual void TestInitialize(IIsolationContext isolationContext)
		{ }

		public void AddCleanupAction(Action cleanupAction)
		{
			_testExuecutionContext.AddCleanupAction(cleanupAction);
		}

		[Obsolete("Override PrintFlowManagerTestBase.TestInitialize method instead of using the [TestInitialize] attribute")]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestInitializeAttribute : Attribute
		{ }

		[Obsolete("Use AddCleanupAction instead")]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestCleanupAttribute : Attribute
		{ }

		protected static void AssemblyInitialize(Action<IIsolationContext> initializationMethod)
		{
			_testExuecutionContext = new TestExecutionContext("Assembly", initializationMethod);
		}

		protected static void AssemblyCleanup(object dummy)
		{
			_testExuecutionContext.Cleanup();
		}

		/// <summary>
		/// Call this method from your [ClassInitialize] method in order to ensure proper cleanup in your [ClassCleanup] method
		/// </summary>
		/// <param name="testClass">the class of the test class (e.g. typeof(MyTestClass))</param>
		/// <param name="initialize">A delegate to a method that does the actual initialization</param>
		protected static void ClassInitialize(Type testClass, Action<IIsolationContext> initialize)
		{
			AssertClassCleanupIsCalled(testClass);

			_testExuecutionContext.PushIsolationLevel("Class Initialize", initialize);
		}

		private static void AssertClassCleanupIsCalled(Type testClass)
		{
			var publicStaticMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
			if (publicStaticMethods.All(x => x.GetCustomAttribute<ClassCleanupAttribute>() == null))
				Assert.Fail(@"Class {0} does not have a [ClassCleanup] method. Please add the following code to class '{0}' to fix it:
************************************
[ClassCleanup]
public static void ClassCleanup()
{{
	ClassCleanup(null);
}}
************************************
", testClass);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			AssemblyCleanup(null);
		}

		/// <summary>
		/// Call this method from your [ClassCleanup] method in order to ensure proper cleanup of all relevant actions that were done in the [ClassInitialize] method
		/// </summary>
		/// <param name="dummy">This argument has no use, it's there just to let you use the name ClassCleanup in your test class without having the compiler complaining that it hides a member with the same name in the base class. Simply pass <b>null</b> here.</param>
		protected static void ClassCleanup(object dummy)
		{
			_testExuecutionContext.PopIsolationLevel();
		}
	}
}