using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
	public abstract class TestBase
	{
		private static readonly TestExecutionContext TestExecutionContext = new TestExecutionContext("Assembly", Functions.EmptyAction<IIsolationContext>());
		protected static readonly Dictionary<Type, TestBase> InitializedInstances = new Dictionary<Type, TestBase>();
//		public TestContext TestContext { get; set; }
		private static bool _classCleanupPending;

		protected TestBase()
		{
			CopyFields();
		}

		[Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
		public void InitializeTest()
		{
			AssertClassInitializeIsCalled();
//			Console.WriteLine("Tests started: {0}", DateTime.Now);

//			try
//			{
				TestExecutionContext.PushIsolationLevel("Test", ctx => TestInitialize());
//			}
//			catch(Exception ex)
//			{
//				OnInitializationException(ex);
//				throw;
//			}
		}

//		protected virtual void OnInitializationException(Exception exception)
//		{
//			TakeScreenshot("Initialize");
//		}

//		private void TakeScreenshot(string name)
//		{
//			var filename = Path.GetFullPath(TestContext.FullyQualifiedTestClassName + "." + name + ".jpg");
//			try
//			{
//				var screenLeft = SystemInformation.VirtualScreen.Left;
//				var screenTop = SystemInformation.VirtualScreen.Top;
//				var screenWidth = SystemInformation.VirtualScreen.Width;
//				var screenHeight = SystemInformation.VirtualScreen.Height;

//				// Create a bitmap of the appropriate size to receive the screenshot.
//				using (var bmp = new Bitmap(screenWidth, screenHeight))
//				{
//					// Draw the screenshot into our bitmap.
//					using (var g = Graphics.FromImage(bmp))
//					{
//						g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
//					}

//					bmp.Save(filename, ImageFormat.Jpeg);
//				}
//				TestContext.AddResultFile(filename);
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine("An exception occured while trying to take screenshot:");
//				Console.WriteLine(ex);
//			}
//		}

		private void AssertClassInitializeIsCalled()
		{
			var actualtype = GetType();
			var publicStaticMethods = actualtype.GetMethods(BindingFlags.Public | BindingFlags.Static);
			var classInitializedDecoratedMethod = publicStaticMethods.SingleOrDefault(x => x.GetCustomAttribute<ClassInitializeAttribute>() != null);

			var classInitializeImplementationClass = actualtype.GetMethod("ClassInitialize", BindingFlags.Instance|BindingFlags.NonPublic, null, new Type[0], new ParameterModifier[0]).DeclaringType;

			if (classInitializedDecoratedMethod == null)
			{
				if (classInitializeImplementationClass != typeof (TestBase))
				{
					Assert.Inconclusive(@"Method {0}.ClassInitialize() will not be called unless you add the following code to class {1}:
************************************
[ClassInitialize]
public static void ClassInitialize(TestContext testContext)
{{
	ClassInitialize(typeof({1}));
}}
************************************
);", classInitializeImplementationClass.Name, actualtype.Name);
				}

				return;
			}

			if (!InitializedInstances.ContainsKey(actualtype))
			{
				Assert.Inconclusive(@"Method {0}.{1} has a [ClassInitialize] attribute, but it does not call the base class's ClassInitialize method. Please change the method to be exactly as follows: 
************************************
[ClassInitialize]
public static void {1}(TestContext testContext)
{{
	ClassInitialize(typeof({2}));
}}
************************************
", actualtype, classInitializedDecoratedMethod.Name, actualtype.Name);
			}
		}

		[Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanup]
		public void CleanupTest()
		{
			TestExecutionContext.PopIsolationLevel();
		}

		private void CopyFields()
		{
			var thisType = GetType();

			TestBase initializedInstance;
			if (!InitializedInstances.TryGetValue(thisType, out initializedInstance))
				return;

			var fields = thisType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (var fieldInfo in fields)
			{
				var valueFromInitializedInstance = fieldInfo.GetValue(initializedInstance);
				//var valueFromCurrentInstance = fieldInfo.GetValue(this);
				//var defaultValue = fieldInfo.FieldType.GetDefaultValue();
				//if (valueFromCurrentInstance.Equals(defaultValue) && !valueFromInitializedInstance.Equals(defaultValue))
					fieldInfo.SetValue(this, valueFromInitializedInstance);
			}
		}

		protected virtual void TestInitialize()
		{ }

		public static void AddCleanupAction(Action cleanupAction)
		{
			TestExecutionContext.AddCleanupAction(cleanupAction);
		}

		[ExcludeFromCodeCoverage]
		protected virtual void ClassInitialize()
		{
		}

		[Obsolete("Override PrintFlowManagerTestBase.TestInitialize method instead of using the [TestInitialize] attribute", true)]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestInitializeAttribute : Attribute
		{ }

		[Obsolete("Use AddCleanupAction instead", true)]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestCleanupAttribute : Attribute
		{ }

//		protected static void AssemblyInitialize(Action<IIsolationContext> initializationMethod)
//		{
//			_testExuecutionContext = new TestExecutionContext("Assembly", initializationMethod);
//		}

//		protected static void AssemblyCleanup(object dummy)
//		{
//			_testExuecutionContext.Cleanup();
//		}

		protected static void ClassInitialize(Type testClass)
		{
			if (_classCleanupPending)
			{
				ClassCleanup(null);
				_classCleanupPending = false;
			}

			AssertClassCleanupIsCalled(testClass);

			var instance = (TestBase)Activator.CreateInstance(testClass);
			InitializedInstances[testClass] = instance;
			TestExecutionContext.PushIsolationLevel("Class Initialize", isolationContext =>
			{
				instance.ClassInitialize();
			});

			_classCleanupPending = true;
		}

		private static void AssertClassCleanupIsCalled(Type testClass)
		{
			var publicStaticMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
			if (publicStaticMethods.All(x => x.GetCustomAttribute<ClassCleanupAttribute>() == null))
				Assert.Inconclusive(@"Class {0} have a proper [ClassInitialize] method, but does not have a [ClassCleanup] method. Please add the following code to class '{0}' to fix it:
************************************
[ClassCleanup]
public static void ClassCleanup()
{{
	ClassCleanup(null);
}}
************************************
", testClass);
		}

//		[AssemblyCleanup]
//		public static void AssemblyCleanup()
//		{
//			AssemblyCleanup(null);
//		}

		/// <summary>
		/// Call this method from your [ClassCleanup] method in order to ensure proper cleanup of all relevant actions that were done in the [ClassInitialize] method
		/// </summary>
		/// <param name="dummy">This argument has no use, it's there just to let you use the name ClassCleanup in your test class without having the compiler complaining that it hides a member with the same name in the base class. Simply pass <b>null</b> here.</param>
		protected static void ClassCleanup(object dummy)
		{
			_classCleanupPending = false;
			TestExecutionContext.PopIsolationLevel();
		}
	}
}