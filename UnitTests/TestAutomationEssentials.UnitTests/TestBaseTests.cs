using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
		
		[TestMethod]
		public void TestsNotRunIfClassInitializeIsMissing()
		{
			var testClass = CreateTestClass(
GetLinePragma() +
@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest;

[TestClass]
public class TestClass1 : TestBase
{
	public TestClass1()
	{
		//System.Diagnostics.Debugger.Launch();
	}

/*	public TestContext TestContext
	{
		get { return null; }
		set { }
	}
*/
	[TestMethod]
	public void TestMethod1()
	{
		System.Diagnostics.Debugger.Launch(); 
		var a = 3;
		a++;
		throw new System.Exception();
	}
}
");
			var results = testClass.Execute();
			TestContext.AddResultFile(results.FullPath);
			Assert.AreEqual(0, results.PassedTests);
			Assert.AreEqual(1, results.FailedTests);
		}

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
		private TestRunType _testRunType;
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

		public string FullPath { get; private set; }
	}
}
