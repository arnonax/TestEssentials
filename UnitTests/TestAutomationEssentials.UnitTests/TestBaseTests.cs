using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;
using TestAutomationEssentials.TrxParser.Generated;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class TestBaseTests
	{
		public TestContext TestContext { get; set; }

		// TODO: extract strings that are used in TestBase and IsolationContext to an external class

		[TestMethod]
		public void SimpleTestClassShouldJustRunTheTests()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
			
[TestClass]
public class TestClass1 : TestBase
{
	[TestMethod]
	public void TestMethod1()
	{
	}

	[TestMethod]
	public void TestMethod2()
	{
	}
}
");
			var results = testClass.Execute();
			Assert.AreEqual(2, results.PassedTests, "Passed tests");
			Assert.AreEqual(0, results.FailedTests);
		}

		[TestMethod]
		public void CleanupActionsAreCalledAtTheEndOfEachTest()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	[TestMethod]
	public void TestMethod1()
	{
		Console.WriteLine(""TestMethod1"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction1""); });
	}

	[TestMethod]
	public void TestMethod2()
	{
		Console.WriteLine(""TestMethod2"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction2""); });
	}
}
");
			var results = testClass.Execute();
			Assert.AreEqual(2, results.PassedTests, "Passed tests");
			Assert.AreEqual(0, results.FailedTests);
			StringAssert.Matches(results.UnitTestResults[0].StdOut, GetOutputRegex(string.Empty, "TestMethod1", "CleanupAction1"), "Output");
			StringAssert.Matches(results.UnitTestResults[1].StdOut, GetOutputRegex(string.Empty, "TestMethod2", "CleanupAction2"), "Output");
		}

		[TestMethod]
		public void TestInitializeIsCalledBeforeEachTest()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	protected override void TestInitialize()
	{
		Console.WriteLine(""MyTestInitialize"");
	}

	[TestMethod]
	public void TestMethod1()
	{
		Console.WriteLine(""TestMethod1"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction1""); });
	}

	[TestMethod]
	public void TestMethod2()
	{
		Console.WriteLine(""TestMethod2"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction2""); });
	}
}
");
			var results = testClass.Execute();
			Assert.AreEqual(2, results.PassedTests, "Passed tests");
			StringAssert.Matches(results.UnitTestResults[0].StdOut, GetOutputRegex("MyTestInitialize", "TestMethod1", "CleanupAction1"), "Output");
			StringAssert.Matches(results.UnitTestResults[1].StdOut, GetOutputRegex("MyTestInitialize", "TestMethod2", "CleanupAction2"), "Output");
		}

		private static Regex GetOutputRegex(string testInitializeOutput, string testMethodOutput, string cleanupActionOutput)
		{
			return new Regex(@"\*+ Initializing Test \*+\n" + 
				testInitializeOutput + 
@"\n?\*+ Initializing Test Completed succesfully \*+\n" + 
testMethodOutput + 
@"\n?\*+ Cleanup Test \*+\n" + 
cleanupActionOutput);
		}

		[TestMethod]
		public void CleanupActionsFromTestInitializeAreCalledAfterEachTest()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	protected override void TestInitialize()
	{
		Console.WriteLine(""MyTestInitialize"");
		AddCleanupAction(() => { Console.WriteLine(""MyTestInitialize.Cleanup""); });
	}

	[TestMethod]
	public void TestMethod1()
	{
		Console.WriteLine(""TestMethod1"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction1""); });
	}

	[TestMethod]
	public void TestMethod2()
	{
		Console.WriteLine(""TestMethod2"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction2""); });
	}
}
");
			var results = testClass.Execute();
			Assert.AreEqual(2, results.PassedTests, "Passed tests");
			StringAssert.Contains(results.UnitTestResults[0].StdOut,
@"***************************** Initializing Test *****************************
MyTestInitialize
***************************** Initializing Test Completed succesfully *****************************
TestMethod1
***************************** Cleanup Test *****************************
CleanupAction1
MyTestInitialize.Cleanup".Replace("\r\n", "\n")
				, "Test1 Output");
			StringAssert.Contains(results.UnitTestResults[1].StdOut, 
@"***************************** Initializing Test *****************************
MyTestInitialize
***************************** Initializing Test Completed succesfully *****************************
TestMethod2
***************************** Cleanup Test *****************************
CleanupAction2
MyTestInitialize.Cleanup".Replace("\r\n", "\n"),
				"Test 2 Output");
		}

		[TestMethod]
		public void IfClassInitializeAttributeIsUsedClassInitializeWithTypeofThisMustBeCalled()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
	}

	[TestMethod]
	public void TestMethod1()
	{
	}

	[TestMethod]
	public void TestMethod2()
	{
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(2, testResults.Inconclusive, "Inconclusive");
			StringAssert.Contains(testResults.UnitTestResults[0].ErrorMessage, "Method TestClass1.MyClassInitialize has a [ClassInitialize] attribute, but it does not call the base class's ClassInitialize method");
		}

		[TestMethod]
		public void IfClassInitializeIsOverridrenThenClassInitializeAttributeMustBeUsed()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	protected override void ClassInitialize()
	{
	}

	[TestMethod]
	public void TestMethod1()
	{
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(1, testResults.Inconclusive, "Inconclusive");
			StringAssert.Contains(testResults.UnitTestResults[0].ErrorMessage, "Method TestClass1.ClassInitialize() will not be called");
		}

		[TestMethod]
		public void IfClassInitializeIsUsedClassCleanupMustBeUsedToo()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[TestMethod]
	public void TestMethod1()
	{
	}

	[TestMethod]
	public void TestMethod2()
	{
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(2, testResults.Inconclusive, "Inconclusive");
			var errorMessage = testResults.UnitTestResults[0].ErrorMessage;
			StringAssert.Contains(errorMessage, "does not have a [ClassCleanup] method");
			StringAssert.Contains(errorMessage, "TestClass1");
		}

		[TestMethod]
		public void ClassInitializedIsCalledOnceBeforeAllTestsInClass()
		{
			var outputFileName = Path.GetFullPath("Output.txt");
			File.Delete(outputFileName);

			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

public class TestBaseWithMethodLogger : TestBase
{
	public void LogMethodCall([CallerMemberName] string callingMethod = null)
	{
		File.AppendAllText(@""" + outputFileName + @""", GetType() + ""."" + callingMethod + Environment.NewLine);
	}
}

[TestClass]
public class TestClass1 : TestBaseWithMethodLogger
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}
	
	protected override void ClassInitialize()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod2()
	{
		LogMethodCall();
	}
}

[TestClass]
public class TestClass2 : TestBaseWithMethodLogger
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass2));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}
	
	protected override void ClassInitialize()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod2()
	{
		LogMethodCall();
	}
}
");

			testClass.Execute();
			var expectedResults = new[]
			{
				"TestClass1.ClassInitialize",
				"TestClass1.TestMethod1",
				"TestClass1.TestMethod2",
				"TestClass2.ClassInitialize",
				"TestClass2.TestMethod1",
				"TestClass2.TestMethod2"
			};

			TestContext.AddResultFile(outputFileName);
			CollectionAssert.AreEqual(expectedResults, File.ReadAllLines(outputFileName));
		}

		[TestMethod]
		public void ClassInitializeCanBeSharedInACommonBaseClass()
		{
			var outputFileName = Path.GetFullPath("Output.txt");
			File.Delete(outputFileName);

			var testClass = CreateTestClass(
				GetLinePragma() +
				@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

public class CommonTestBase : TestBase
{
	public void LogMethodCall([CallerMemberName] string callingMethod = null)
	{
		File.AppendAllText(@""" + outputFileName + @""", GetType() + ""."" + callingMethod + Environment.NewLine);
	}

	protected override void ClassInitialize()
	{
		LogMethodCall();
	}
}

[TestClass]
public class TestClass1 : CommonTestBase
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}
}

[TestClass]
public class TestClass2 : CommonTestBase
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass2));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}
}

");
			testClass.Execute();
			var expectedResult = new[]
			{
				"TestClass1.ClassInitialize",
				"TestClass1.TestMethod1",
				"TestClass2.ClassInitialize",
				"TestClass2.TestMethod1"
			};

			CollectionAssert.AreEqual(expectedResult, File.ReadAllLines(outputFileName));
		}

		[TestMethod]
		public void CleanupActionsInClassInitializedAreCalledAfterAllTestsInClass()
		{
			var outputFileName = Path.GetFullPath("Output.txt");
			File.Delete(outputFileName);

			var testClass = CreateTestClass(
				GetLinePragma() +
				@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

public class TestBaseWithMethodLogger : TestBase
{
	public void LogMethodCall([CallerMemberName] string callingMethod = null)
	{
		File.AppendAllText(@""" + outputFileName + @""", GetType() + ""."" + callingMethod + Environment.NewLine);
	}
}

[TestClass]
public class TestClass1 : TestBaseWithMethodLogger
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}
	
	protected override void ClassInitialize()
	{
		LogMethodCall();
		AddCleanupAction(ClassCleanupAction1);
		AddCleanupAction(ClassCleanupAction2);
	}

	private void ClassCleanupAction1()
	{
		LogMethodCall();
	}

	private void ClassCleanupAction2()
	{
		LogMethodCall();
	}

	private void TestCleanupAction1()
	{
		LogMethodCall();
	}

	private void TestCleanupAction2()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod1()
	{
		AddCleanupAction(TestCleanupAction1);
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod2()
	{
		AddCleanupAction(TestCleanupAction2);
		LogMethodCall();
		Assert.Fail(); // cleanup should still be called!
	}
}");
			testClass.Execute();

			var expectedResults = new[]
			{
				"TestClass1.ClassInitialize",
				"TestClass1.TestMethod1",
				"TestClass1.TestCleanupAction1",
				"TestClass1.TestMethod2",
				"TestClass1.TestCleanupAction2",
				"TestClass1.ClassCleanupAction2",
				"TestClass1.ClassCleanupAction1"
			};

			TestContext.AddResultFile(outputFileName);
			CollectionAssert.AreEqual(expectedResults, File.ReadAllLines(outputFileName));
		}

		[TestMethod]
		public void CleanupActionsInClassInitializedAreCalledBeforeNextClassInitialize()
		{
			var outputFileName = Path.GetFullPath("Output.txt");
			File.Delete(outputFileName);

			var testClass = CreateTestClass(
				GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

public class TestBaseWithMethodLogger : TestBase
{
	public void LogMethodCall([CallerMemberName] string callingMethod = null)
	{
		File.AppendAllText(@""" + outputFileName + @""", GetType() + ""."" + callingMethod + Environment.NewLine);
	}
}

[TestClass]
public class TestClass1 : TestBaseWithMethodLogger
{
	[ClassInitialize]
	public static void MyClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}
	
	protected override void ClassInitialize()
	{
		LogMethodCall();
		AddCleanupAction(Class1CleanupAction);
	}

	private void Class1CleanupAction()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}
}

[TestClass]
public class TestClass2 : TestBaseWithMethodLogger
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext context)
	{
		ClassInitialize(typeof(TestClass2));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}

	protected override void ClassInitialize()
	{
		LogMethodCall();
		AddCleanupAction(Class2CleanupAction);
	}

	private void Class2CleanupAction()
	{
		LogMethodCall();
	}

	[TestMethod]
	public void TestMethod1()
	{
		LogMethodCall();
	}
}");
			testClass.Execute();

			var expectedResults = new[]
			{
				"TestClass1.ClassInitialize",
				"TestClass1.TestMethod1",
				"TestClass1.Class1CleanupAction",
				"TestClass2.ClassInitialize",
				"TestClass2.TestMethod1",
				"TestClass2.Class2CleanupAction"
			};

			TestContext.AddResultFile(outputFileName);
			CollectionAssert.AreEqual(expectedResults, File.ReadAllLines(outputFileName));
		}

		[TestMethod]
		public void InstanceClassMemberArePreservedBetweenClassInitializeAndTestMethods()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext testContext)
	{
		ClassInitialize(typeof(TestClass1));
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		ClassCleanup(null);
	}

	private bool _instanceMemberWasSet = false;

	protected override void ClassInitialize()
	{
		_instanceMemberWasSet = true;
	}

	[TestMethod]
	public void TestMethod1()
	{
		Assert.IsTrue(_instanceMemberWasSet);
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(1, testResults.PassedTests, "Passed");
		}

		[TestMethod]
		public void CompilationErrorOccursIfUsingTestInitializedOrTestCleanupAttributesDirectly()
		{
			var compileResults = Compile(
				GetLinePragma() +
				@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
#line 10
	[TestInitialize] // Compile error!
	public void TestInitialize()
	{
	}

#line 20
	[TestCleanup]
	public void TestCleanup() // Compile error
	{
	}
}");

			const string obsoleteErrorCode = "CS0619";
			var errors = compileResults.Errors.Cast<CompilerError>().Where(x => x.IsWarning == false).ToArray();
			Assert.AreEqual(2, errors.Count(), "Number of compilation errors");
			var error = errors[0];
			Assert.AreEqual(10, error.Line);
			Assert.AreEqual(obsoleteErrorCode, error.ErrorNumber, error.ErrorText);

			error = errors[1];
			Assert.AreEqual(20, error.Line);
			Assert.AreEqual(obsoleteErrorCode, error.ErrorNumber, error.ErrorText);
		}

		[TestMethod]
		public void WhenExceptionOccursInTestInitializeThenOnTestFailureIsCalled()
		{
			var outputFileName = Path.GetFullPath("Output.txt");
			File.Delete(outputFileName);

			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest;
using System;
using System.IO;

public class MyTestBase : TestBase
{
	protected override void OnTestFailure(TestContext testContext)
	{
		File.WriteAllText(@""" + outputFileName + @""", ""An error occured in "" + testContext.TestName);
	}
}

[TestClass]
public class TestClass1 : MyTestBase
{
	protected override void TestInitialize()
	{
		throw new Exception(""Something wrong..."");
	}

	[TestMethod]
	public void TestMethod1()
	{
		
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(1, testResults.FailedTests, "Failed");

			TestContext.AddResultFile(outputFileName);
			Assert.AreEqual("An error occured in TestMethod1", File.ReadAllText(outputFileName));
		}

		private string GetLinePragma([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string file = "")
		{
			return string.Format("#line {0} \"{1}\"\n", lineNumber + 1, file);
		}

		private TestClass CreateTestClass(string testClassCode)
		{
			const string outputName = "mytest.dll";
			var results = Compile(testClassCode, outputName);
			Assert.AreEqual(0, results.Errors.Count, string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));

			return new TestClass(outputName, TestContext);
		}

		private static CompilerResults Compile(string testClassCode, string outputName = "dummy.dll")
		{
			var csc = new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", "v4.0"}});

			var parameters =
				new CompilerParameters(
					new[]
					{
						"mscorlib.dll", "System.Core.dll", typeof (TestClassAttribute).Assembly.Location,
						typeof (TestBase).Assembly.Location
					}, outputName, true);
			parameters.GenerateExecutable = false;
			parameters.IncludeDebugInformation = true;
			CompilerResults results = csc.CompileAssemblyFromSource(parameters, testClassCode);
			return results;
		}

		private class TestClass
		{
			private readonly string _fileName;
			private readonly TestContext _testContext;

			public TestClass(string fileName, TestContext testContext)
			{
				_fileName = fileName;
				_testContext = testContext;
			}

			public TestResults Execute()
			{
				var msTestFullPath = GetMsTestFullPath();

				var trxFile = Path.GetFullPath(Path.GetRandomFileName()) + ".trx";

				var parameters = string.Format("/testContainer:\"{0}\" /resultsFile:\"{1}\"", _fileName, trxFile);
				var startInfo = new ProcessStartInfo(msTestFullPath, parameters)
				{
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
				};

				Console.WriteLine("Executing: {0} {1}", msTestFullPath, parameters);
				using (var msTest = Process.Start(startInfo))
				{
					msTest.WaitForExit();
					var output = msTest.StandardOutput.ReadToEnd();
					var error = msTest.StandardError.ReadToEnd();
					Console.WriteLine("Output:");
					Console.WriteLine(output);
					Console.WriteLine("Error:");
					Console.WriteLine(error);
				}

				Console.WriteLine("TrxFile: \"file://{0}\"", trxFile);
				
				_testContext.AddResultFile(trxFile);

				return new TestResults(trxFile);
			}

			private static string GetMsTestFullPath()
			{
				// Our current process is either QTAgent32*.exe or vstest.console.exe
				// In either case, MSTest.exe should be nearby, in the IDE folder
				var currentProcessPath = Process.GetCurrentProcess().Modules[0].FileName;
				var currentProcessFolder = Path.GetDirectoryName(currentProcessPath);
				var mstestFolder = PathUtils.GetAncestorPath(currentProcessFolder, "IDE");
				var msTestFullPath = Path.Combine(mstestFolder, "MSTest.exe");
				return msTestFullPath;
			}
		}
	}

	internal class TestResults
	{
		private readonly TestRunType _testRunType;
		private readonly CountersType _counters;

		public TestResults(string trxFile)
		{
			var serializer = new XmlSerializer(typeof (TestRunType));
			using (var fileStream = File.OpenRead(trxFile))
			{
				_testRunType = (TestRunType) serializer.Deserialize(fileStream);
			}
			_counters = _testRunType.Items.OfType<TestRunTypeResultSummary>().Content().Items.OfType<CountersType>().Content();
			FullPath = trxFile;
		}

		public int PassedTests
		{
			get { return _counters.passed; }
		}

		public int FailedTests
		{
			get { return _counters.failed; }
		}

		public int Inconclusive
		{
			get { return _counters.inconclusive; }
		}

		public string FullPath { get; private set; }

		public IReadOnlyList<UnitTestResult> UnitTestResults
		{
			get
			{
				return
					_testRunType.Items.OfType<ResultsType>().Content().Items.OfType<UnitTestResultType>()
						.Select(x => new UnitTestResult(x)).ToList();
			}
		}
	}

	internal class UnitTestResult
	{
		private readonly UnitTestResultType _content;

		public UnitTestResult(UnitTestResultType content)
		{
			_content = content;
		}

		public string ErrorMessage
		{
			get { return ((XmlNode[]) _content.Items.OfType<OutputType>().Content().ErrorInfo.Message).Content().InnerText; }
		}

		public string StdOut
		{
			get { return ((XmlNode[])_content.Items.OfType<OutputType>().Content().StdOut).Content().InnerText; }
		}
	}
}
