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
			TestContext.AddResultFile(results.FullPath);
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
			TestContext.AddResultFile(results.FullPath);
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
			TestContext.AddResultFile(results.FullPath);
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
			TestContext.AddResultFile(results.FullPath);
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
	public static void ClassInitialize(TestContext testContext)
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
			StringAssert.Contains(testResults.UnitTestResults[0].ErrorMessage, "does not have a [ClassInitialize] method");
		}

		[TestMethod]
		public void ClassInitializedIsCalledOnceBeforeAllTestsInClass()
		{
			
		}

		[TestMethod]
		public void CleanupActionsInClassInitializedAreCalledAfterAllTestsInClass()
		{
		
		}

		[TestMethod]
		public void CleanupActionsInClassInitializedAreCalledBeforeNextClassInitialize()
		{
			
		}



//		[TestMethod]
//		public void TestsFailIfClassInitializeIsMissing()
//		{
//			var testClass = CreateTestClass(
//GetLinePragma() +
//@"using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TestAutomationEssentials.MSTest;
//
//[TestClass]
//public class TestClass1 : TestBase
//{
//	[TestMethod]
//	public void TestMethod1()
//	{
//	}
//}
//");
//			var results = testClass.Execute();
//			TestContext.AddResultFile(results.FullPath);
//			Assert.AreEqual(0, results.PassedTests);
//			Assert.AreEqual(0, results.FailedTests);
//			Assert.AreEqual(1, results.Inconclusive);
//			StringAssert.Contains(results.UnitTestResults.Content().ErrorMessage, "does not have a [ClassInitialize] method.");
//		}

//		[TestMethod]
//		public void TestFailsIfClassInitializeExistsButDoesNotCallBaseClassInitialize()
//		{
//			var testClass = CreateTestClass(
//GetLinePragma() +
//@"using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TestAutomationEssentials.MSTest;
//
//[TestClass]
//public class TestClass1 : TestBase
//{
//	[ClassInitialize]
//	public static void ClassInit(TestContext testContext)
//	{
//	}
//	
//	[TestMethod]
//	public void TestMethod1()
//	{
//	}
//}
//");
//			var results = testClass.Execute();
//			TestContext.AddResultFile(results.FullPath);
//			Assert.AreEqual(0, results.PassedTests);
//			Assert.AreEqual(0, results.FailedTests);
//			Assert.AreEqual(1, results.Inconclusive);
//			StringAssert.Contains(results.UnitTestResults.Content().ErrorMessage, "must call ClassInitialize(typeof");
//		}

//		[TestMethod]
//		public void TestFailsIfClassCleanupIsMissing()
//		{
//			var testClass = CreateTestClass(
//GetLinePragma() +
//@"using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TestAutomationEssentials.MSTest;
//
//[TestClass]
//public class TestClass1 : TestBase
//{
//	[ClassInitialize]
//	public static void ClassInit(TestContext testContext)
//	{
//		ClassInitialize(typeof(TestClass1));
//	}
//	
//	[TestMethod]
//	public void TestMethod1()
//	{
//	}
//}
//");
//			var results = testClass.Execute();
//			TestContext.AddResultFile(results.FullPath);
//			Assert.AreEqual(0, results.PassedTests);
//			Assert.AreEqual(0, results.FailedTests);
//			Assert.AreEqual(1, results.Inconclusive);
//			StringAssert.Contains(results.UnitTestResults.Content().ErrorMessage, "does not have a [ClassCleanup] method");
//		}

//		[TestMethod]
//		public void TestSucceedsIfCleanupExistsAndCallsBase()
//		{
//			var testClass = CreateTestClass(
//GetLinePragma() +
//@"using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TestAutomationEssentials.MSTest;
//
//[TestClass]
//public class TestClass1 : TestBase
//{
//	[ClassInitialize]
//	public static void ClassInit(TestContext testContext)
//	{
//		ClassInitialize(typeof(TestClass1));
//	}
//	
//	[ClassCleanup]
//	public static void ClassCleanup()
//	{
//		ClassCleanup(null);
//	}
//
//	[TestMethod]
//	public void TestMethod1()
//	{
//	}
//}
//");
//			var results = testClass.Execute();
//			TestContext.AddResultFile(results.FullPath);
//			Assert.AreEqual(1, results.PassedTests, "# of Passed tests");
//			Assert.AreEqual(0, results.FailedTests, "# of failed tests");
//		}

		private string GetLinePragma([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string file = "")
		{
			return string.Format("#line {0} \"{1}\"\n", lineNumber + 1, file);
		}

		private TestClass CreateTestClass(string testClassCode)
		{
			var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
			const string outputName = "mytest.dll";

			var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", typeof(TestClassAttribute).Assembly.Location, typeof(TestBase).Assembly.Location }, outputName, true);
			parameters.GenerateExecutable = false;
			parameters.IncludeDebugInformation = true;
			CompilerResults results = csc.CompileAssemblyFromSource(parameters, testClassCode);
			Assert.AreEqual(0, results.Errors.Count, string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));

			return new TestClass(outputName);
		}

		private class TestClass
		{
			private readonly string _fileName;

			public TestClass(string fileName)
			{
				_fileName = fileName;
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
