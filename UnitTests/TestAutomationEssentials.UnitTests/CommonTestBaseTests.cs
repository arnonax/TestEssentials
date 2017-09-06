using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
    // IMPORTANT: This class is linked to 2 project: MSTest and MSTestv2.
    // The reason that it is linked and not re-used in the normal way, is that each require different 
    // references, even though they're compatible at the code level.

    [TestClass]
	public abstract class CommonTestBaseTests
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
using TestAutomationEssentials.Common;
using System;
			
[TestClass]
public class TestClass1 : TestBase
{
	protected override void TestInitialize()
	{
		Logger.WriteLine(""MyTestInitialize"");
	}

	[TestMethod]
	public void TestMethod1()
	{
		Logger.WriteLine(""TestMethod1"");
		AddCleanupAction(() => { Console.WriteLine(""CleanupAction1""); });
	}

	[TestMethod]
	public void TestMethod2()
	{
		Logger.WriteLine(""TestMethod2"");
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
			return new Regex(@".*\*+ Initializing Test \*+\n.*" + 
				testInitializeOutput + 
@"\n?.*\*+ Initializing Test Completed succesfully \*+\n.*" + 
testMethodOutput + 
@"\n?.*\*+ Cleanup Test \*+\n.*" + 
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
			AssertLogContains(results.UnitTestResults[0].StdOut,
@"***************************** Initializing Test *****************************
MyTestInitialize
***************************** Initializing Test Completed succesfully *****************************
TestMethod1
***************************** Cleanup Test *****************************
CleanupAction1
MyTestInitialize.Cleanup"
				, "Test1 Output");
			AssertLogContains(results.UnitTestResults[1].StdOut, 
@"***************************** Initializing Test *****************************
MyTestInitialize
***************************** Initializing Test Completed succesfully *****************************
TestMethod2
***************************** Cleanup Test *****************************
CleanupAction2
MyTestInitialize.Cleanup",
				"Test 2 Output");
		}

		// ReSharper disable once UnusedParameter.Local
		private static void AssertLogContains(string actual, string expected, string message)
		{
			var actualLines = GetLines(actual).ToArray();
			var expectedLines = GetLines(expected).ToArray();
			for (var i = 0; i < actualLines.Length - expectedLines.Length+1; i++)
			{
				if (Matches(actualLines.SubArray(i, expectedLines.Length), expectedLines))
					return;
			}

			Assert.Fail("The expected lines was not found in the log: {0}\nExpected:\n{1}\n\nActual:\n{2}", message, expected, actual);
		}

		private static bool Matches(IReadOnlyList<string> actualLines, IReadOnlyList<string> expectedLines)
		{
			for (var i = 0; i < actualLines.Count; i++)
			{
				if (!actualLines[i].Contains(expectedLines[i]))
					return false;
			}

			return true;
		}

		private static IEnumerable<string> GetLines(string multilineString)
		{
			using (var reader = new StringReader(multilineString))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
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
		public void CleanupActionsInAssemblyInitializedAreCalledAfterAllTestsInTheAssemblyCompletes()
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
		LogMethodCall(GetType(), callingMethod);
	}

	public static void LogMethodCall(Type type, [CallerMemberName] string callingMethod = null)
	{
		File.AppendAllText(@""" + outputFileName + @""", type + ""."" + callingMethod + Environment.NewLine);
	}
}

[TestClass]
public class TestClass1 : TestBaseWithMethodLogger
{
	[AssemblyInitialize]
	public static void AssemblyInitialize(TestContext testContext)
	{
		AddCleanupAction(AssemblyCleanupAction);
	}

	private static void AssemblyCleanupAction()
	{
		LogMethodCall(typeof(TestClass1));
	}
	
	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		AssemblyCleanup(null);
	}

	[TestMethod]
	public void Test1()
	{
		LogMethodCall();
	}
}

[TestClass]
public class TestClass2 : TestBaseWithMethodLogger
{
	[TestMethod]
	public void Test2()
	{
		LogMethodCall();
	}
}
");
			testClass.Execute();

			TestContext.AddResultFile(outputFileName);
			var allLines = File.ReadAllLines(outputFileName);

			Assert.AreEqual("TestClass1.AssemblyCleanupAction", allLines.Last());
			Assert.IsTrue(allLines.Contains("TestClass1.Test1"));
			Assert.IsTrue(allLines.Contains("TestClass2.Test2"));
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
			Assert.AreEqual(2, errors.Length, "Number of compilation errors");
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

		[TestMethod]
		public void WhenExceptionOccursInTestMethodThenOnTestFailureIsCalled()
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
	[TestMethod]
	public void TestMethod1()
	{
		throw new Exception(""Something wrong..."");
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(1, testResults.FailedTests, "Failed");

			TestContext.AddResultFile(outputFileName);
			Assert.AreEqual("An error occured in TestMethod1", File.ReadAllText(outputFileName));
		}

	    [TestMethod]
	    public void OnTestFailureIsNotCalledIfTestPasses()
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

[TestClass]
public class TestClass1 : TestBase
{
	protected override void OnTestFailure(TestContext testContext)
	{
		File.WriteAllText(@""" + outputFileName + @""", ""OnTestFailure was called"");
	}

	[TestMethod]
	public void TestMethod1()
	{
		// Pass
	}
}
");
            var testResults = testClass.Execute();
            Assert.AreEqual(0, testResults.FailedTests, "No failures expected");

            TestContext.AddResultFile(outputFileName);
            Assert.IsFalse(File.Exists(outputFileName), "File should not be created because OnTestFailure shouldn't have been called");
        }

	    [TestMethod]
	    public void OnTestFailureShouldBeCalledBeforeCleanup()
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

[TestClass]
public class TestClass1 : TestBase
{
	protected override void OnTestFailure(TestContext testContext)
	{
		File.AppendAllText(@""" + outputFileName + @""", ""OnTestFailure "");
	}

	[TestMethod]
	public void TestMethod1()
	{
        AddCleanupAction(() => File.AppendAllText(@""" + outputFileName + @""", ""Cleanup""));
		throw new Exception();
	}
}
");
            var testResults = testClass.Execute();
            Assert.AreEqual(1, testResults.FailedTests, "Test should fail");
            TestContext.AddResultFile(outputFileName);
	        Assert.AreEqual("OnTestFailure Cleanup", File.ReadAllText(outputFileName));
	    }

	    [TestMethod]
	    public void CleanupShouldBeCalledEvenIfOnTestFailureThrewException()
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

[TestClass]
public class TestClass1 : TestBase
{
	protected override void OnTestFailure(TestContext testContext)
	{
		throw new Exception(""Exception from OnTestFailure"");
	}

	[TestMethod]
	public void TestMethod1()
	{
        AddCleanupAction(() => File.AppendAllText(@""" + outputFileName + @""", ""Cleanup""));
		throw new Exception();
	}
}
");

            var testResults = testClass.Execute();
            Assert.AreEqual(1, testResults.FailedTests, "Test should fail");
            TestContext.AddResultFile(outputFileName);
            Assert.AreEqual("Cleanup", File.ReadAllText(outputFileName));
            StringAssert.Contains(testResults.UnitTestResults[0].StdOut, "Exception from OnTestFailure");
            StringAssert.Contains(testResults.UnitTestResults[0].StdOut, " at TestClass1.OnTestFailure");
        }

        [TestMethod]
		public void UITestBaseTakesScreenshotOnFailure()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using TestAutomationEssentials.MSTest.UI;
using System;
using System.IO;

[TestClass]
public class TestClass1 : TestBase
{
	[TestMethod]
	public void TestMethod1()
	{
		throw new Exception(""Something wrong..."");
	}
}
");
			var testResults = testClass.Execute();
			Assert.AreEqual(1, testResults.FailedTests, "Failed");

		    var resultFiles = testResults.UnitTestResults.Content().ResultFiles;
		    Assert.AreEqual(1, resultFiles.Count, "Number of result files");

            var resultFile = resultFiles.Content();
		    Assert.AreEqual("TestClass1.jpg", Path.GetFileName(resultFile), "Result file name");
		    Assert.IsTrue(File.Exists(resultFile), $"Screenshot file '{resultFile}' does exist");
		}

	    protected string GetLinePragma([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string file = "")
		{
			return string.Format("#line {0} \"{1}\"\n", lineNumber + 1, file);
		}

	    protected ITestClass CreateTestClass(string testClassCode)
		{
			const string outputName = "mytest.dll";
			var results = Compile(testClassCode, outputName);
			Assert.AreEqual(0, results.Errors.Count, string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));

			return CreateTestClass(outputName, TestContext);
		}

	    protected abstract ITestClass CreateTestClass(string dllName, TestContext testContext);

        private CompilerResults Compile(string testClassCode, string outputName = "dummy.dll")
		{
			var csc = new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", "v4.0"}});

			var parameters =
				new CompilerParameters(
					new[]
					{
						"mscorlib.dll", "System.Core.dll", typeof (TestClassAttribute).Assembly.Location,
						typeof (TestBase).Assembly.Location,
						typeof (Logger).Assembly.Location
					}, outputName, true)
				{
					GenerateExecutable = false,
					IncludeDebugInformation = true
				};

            parameters.ReferencedAssemblies.AddRange(GetAdditionalReferences().ToArray());
			var results = csc.CompileAssemblyFromSource(parameters, testClassCode);
			return results;
		}

	    protected virtual IEnumerable<string> GetAdditionalReferences()
	    {
	        yield break;
	    }
	}

    public interface ITestClass
    {
        TestResults Execute();
    }

    public class UnitTestResult
	{
		private readonly UnitTestResultType _content;
	    private readonly TestResults _trx;

	    public UnitTestResult(UnitTestResultType content, TestResults trx)
	    {
	        _content = content;
	        _trx = trx;
	    }

		public string ErrorMessage => ((XmlNode[]) Output().ErrorInfo.Message).Content().InnerText;

	    public string StdOut => ((XmlNode[])Output().StdOut).Content().InnerText;

	    private OutputType Output()
	    {
	        return _content.Items.OfType<OutputType>().Content();
	    }

	    public IReadOnlyList<string> ResultFiles
	    {
	        get
	        {
	            var resultsFileElement = _content.Items.OfType<ResultFilesType>().Content();
	            var currentFolder = Path.GetDirectoryName(_trx.FullPath);
	            var resultsFolder = Path.GetFileNameWithoutExtension(_trx.FullPath);
	            resultsFolder = Path.Combine(currentFolder, resultsFolder);
	            var executionId = _content.executionId;
	            var resultExecutionFolder = Path.Combine(resultsFolder, "In", executionId);
	            return (from file in resultsFileElement.ResultFile
	                select Path.Combine(resultExecutionFolder, file.path)).ToList();
	        }
	    }
	}

    public class TestResults
    {
        private readonly TestRunType _testRunType;

        public TestResults(string trxFile)
        {
            var serializer = new XmlSerializer(typeof(TestRunType));
            using (var fileStream = File.OpenRead(trxFile))
            {
                _testRunType = (TestRunType)serializer.Deserialize(fileStream);
            }
            _testRunType.Items.OfType<TestRunTypeResultSummary>().Content().Items.OfType<CountersType>().Content();
            FullPath = trxFile;
        }

        public int PassedTests => CountOutcomes("Passed");

        public int FailedTests => CountOutcomes("Failed");

        public int Inconclusive => CountOutcomes("NotExecuted");

        private int CountOutcomes(string outcome)
        {
            return GetUnitTestResults().Count(x => x.outcome == outcome);
        }

        private IEnumerable<UnitTestResultType> GetUnitTestResults()
        {
            return _testRunType.Items.OfType<ResultsType>().Content().Items.OfType<UnitTestResultType>();
        }

        public string FullPath { get; }

        public IReadOnlyList<UnitTestResult> UnitTestResults
        {
            get
            {
                return GetUnitTestResults().Select(x => new UnitTestResult(x, this)).ToList();
            }
        }
    }
}
