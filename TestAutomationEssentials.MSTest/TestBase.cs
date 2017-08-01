using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.Common.ExecutionContext;

namespace TestAutomationEssentials.MSTest
{
	namespace UI
	{
		/// <summary>
		/// Extends <see cref="MSTest.TestBase"/> for UI tests, that take a screenshot in case of a failure. This is useful for example for Selenium tests.
		/// </summary>
		[TestClass]
		public abstract class TestBase : MSTest.TestBase
		{
			/// <summary>
			/// This method is called in case of a test failure and takes a screenshot. The screenshot is saved in a file whose name is the full test name with a ".jpg" suffix.
			/// </summary>
			/// <param name="testContext">Information about the current test</param>
			protected override void OnTestFailure(TestContext testContext)
			{
				TakeScreenshot(testContext);
			}

			private void TakeScreenshot(TestContext testContext)
			{
				var filename = Path.GetFullPath(testContext.FullyQualifiedTestClassName + ".jpg");
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
					testContext.AddResultFile(filename);
				}
				catch (Exception ex)
				{
					Logger.WriteLine("An exception occured while trying to take screenshot:");
					Logger.WriteLine(ex);
				}
			}

		}
	}

	/// <summary>
	/// Provides a general base class for tests, with a useful mechanism to handle cleanup as described in http://blogs.microsoft.co.il/arnona/2014/09/02/right-way-test-cleanup/
	/// </summary>
	[TestClass]
	public abstract class TestBase
	{
		/// <summary>
		/// Returns the object that manages the nested isolation scopes. You can use this member is you have a need to create your own isolation scopes.
		/// </summary>
		public static readonly TestExecutionScopesManager TestExecutionScopesManager = new TestExecutionScopesManager("Assembly", Functions.EmptyAction<IIsolationScope>());
		private static readonly Dictionary<Type, TestBase> InitializedInstances = new Dictionary<Type, TestBase>();
		
		/// <summary>
		/// Provides information and utilities related to the current MSTest execution. The setter of this property is used by MSTest - don't call it directly!
		/// </summary>
		public TestContext TestContext { get; set; }

		private static bool _classCleanupPending;

		/// <summary>
		/// Initializes a new instance of TestBase
		/// </summary>
		protected TestBase()
		{
			CopyFields();
		}

		/// <summary>
		/// This method should only be called by MSTest. Don't call this method directly!
		/// </summary>
		[Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
		public void InitializeTest()
		{
			AssertClassInitializeIsCalled();
			Logger.WriteLine("Tests started: {0}", DateTime.Now);

			try
			{
				TestExecutionScopesManager.BeginIsolationScope("Test", ctx => TestInitialize());
			}
			catch(Exception)
			{
				OnTestFailure(TestContext);
				throw;
			}
		}

		/// <summary>
		/// Override this method in order to collect data after a test failure, or do any other action that should be performed after a test failure.
		/// </summary>
		/// <param name="testContext"></param>
		protected virtual void OnTestFailure(TestContext testContext)
		{
		}

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

		/// <summary>
		/// This method should only be called by MSTest. Don't call this method directly!
		/// </summary>
		[Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanup]
		public void CleanupTest()
		{
			var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
		    if (testFailed)
		    {
                try
                {
                    OnTestFailure(TestContext);
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex);
                }
            }

            TestExecutionScopesManager.EndIsolationScope();
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

		/// <summary>
		/// Override this method in order to perform operations that you want to be executed before each test in the derived class
		/// </summary>
		protected virtual void TestInitialize()
		{ }

		/// <summary>
		/// Adds an action to be performed when the current execution scope ends.
		/// </summary>
		/// <param name="cleanupAction">A delegate to an action to perform on cleanup</param>
		/// <remarks>
		/// Actions that are registered with this method are called on cleanup, regardless whether the test passed or failed.
		/// You can call this methods any number of times, and the actions will be invoked in reverse order. This is useful because
		/// if there's a dependency between actions that you perform in the tests, then usually you must clean them up in the reverse
		/// order in order for the cleanup to succeed.
		/// Calling this method from the <see cref="AssemblyInitializeAttribute"/> decorated method, causes the 
		/// action to be called when <see cref="AssemblyCleanup"/> is called.
		/// Calling this method from <see cref="ClassInitialize()"/> causes the action to be called after all tests in the class
		/// have completed (through the call to <see cref="ClassCleanup"/>).
		/// Calling this method from <see cref="TestInitialize"/> or from any test method (decorated with <see cref="TestMethodAttribute"/>)
		/// causes the actions to be called after the currently running test ends. Note that if there's an exception inside 
		/// <see cref="TestInitialize"/>, the test method won't be called, but any cleanup actions that were already been registered
		/// will be.
		/// </remarks>
		public static void AddCleanupAction(Action cleanupAction)
		{
			TestExecutionScopesManager.AddCleanupAction(cleanupAction);
		}

		/// <summary>
		/// Override this method in order to initialize the state of the application under tests for all the tests
		/// in the current class
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note that unlike <see cref="ClassInitializeAttribute"/> decorated methods, this class is not static and
		/// therefore can be reused in a base class of few test classes. However, in order for this method to be 
		/// called, you must add a <see cref="ClassInitializeAttribute"/> decorated method to the relevant classes
		/// and call <see cref="ClassInitialize(Type)"/> with <code>typeof(YourClassName)</code> as an argument.
		/// You also must add a <see cref="ClassCleanupAttribute"/> decorated method and inside of it call
		/// <see cref="ClassCleanup"/>. Fortunately, if you'll forget any of these, you'll get an error that tell
		/// you exactly what is missing :-)
		/// </para>
		/// <para>
		/// IMPORTANT: because this method is not static, you can use instance members in it. While these instance
		/// members will be preserved when the test methods (or <see cref="TestInitialize"/>) executes, the instance
		/// of the class is not actually the same. <see cref="TestBase"/> creates an instance of the class before
		/// calling this method, and copies these members to the instance that MSTest creates for each test.
		/// This means that you can safely use members that you initialize from this method in test methods, but
		/// if you change the value of a member inside a test method, it won't preserve to the next test. In
		/// addition, if you pass a reference to <code>this</code> to some other class, it won't be the same
		/// instance when the tests run.
		/// </para>
		/// </remarks>
		[ExcludeFromCodeCoverage]
		protected virtual void ClassInitialize()
		{
		}

		/// <summary>
		/// The purpose of this class is to hide the <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute"/>.
		/// This attribute is already used internally be <see cref="TestBase"/> and you should not use it directly.
		/// Instead, override <see cref="TestBase.TestInitialize"/>.
		/// </summary>
		[Obsolete("Override PrintFlowManagerTestBase.TestInitialize method instead of using the [TestInitialize] attribute", true)]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestInitializeAttribute : Attribute
		{ }


		/// <summary>
		/// The purpose of this class is to hide the <see cref="TestClassAttribute"/>.
		/// This attribute is already used internally be <see cref="TestBase"/> and you should not use it directly.
		/// Instead, call <see cref="TestBase.AddCleanupAction"/> for every action that you want to perform it the
		/// test cleanup stage.
		/// </summary>
		[Obsolete("Use AddCleanupAction instead", true)]
		[AttributeUsage(AttributeTargets.Method)]
		public class TestCleanupAttribute : Attribute
		{ }

		/// <summary>
		/// You must call this method from a <see cref="AssemblyCleanupAttribute"/> decorated method in order
		/// for the cleanup actions that are added inside a <see cref="AssemblyInitializeAttribute"/> to be called
		/// at the right time
		/// </summary>
		/// <param name="dummy">This argument is not in use. Always pass <b>null</b>. (The purpose of this argument 
		/// is to allow you to name your <see cref="AssemblyCleanupAttribute"/> decorate method "AssemblyCleanup"
		/// and avoid ambiguity).
		/// </param>
		protected static void AssemblyCleanup(object dummy)
		{
			TestExecutionScopesManager.EndIsolationScope();
		}

		/// <summary>
		/// You must call this method from a <see cref="ClassCleanupAttribute"/> decorated method in order for
		/// <see cref="ClassInitialize()"/> to be called
		/// </summary>
		/// <param name="testClass">The type of the current class. Always use the <code>typeof</code> with the
		/// current class's name</param>
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
			TestExecutionScopesManager.BeginIsolationScope("Class Initialize", isolationContext =>
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

		/// <summary>
		/// You must call this method from your [ClassCleanup] method in order to ensure proper cleanup of all 
		/// relevant actions that were done in the [ClassInitialize] method
		/// </summary>
		/// <param name="dummy">This argument is not in use. Always pass <b>null</b>. (The purpose of this argument 
		/// is to allow you to name your <see cref="ClassCleanupAttribute"/> decorate method "ClassCleanup" and 
		/// avoid ambiguity).
		/// </param>
		protected static void ClassCleanup(object dummy)
		{
			_classCleanupPending = false;
			TestExecutionScopesManager.EndIsolationScope();
		}
	}
}