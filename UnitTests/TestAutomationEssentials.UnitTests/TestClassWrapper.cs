using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.UnitTests
{
    internal class TestClassWrapper : ITestClass
    {
        private readonly string _fileName;
        private readonly TestContext _testContext;

        public TestClassWrapper(string fileName, TestContext testContext)
        {
            _fileName = fileName;
            _testContext = testContext;
        }

        public TestResults Execute()
        {
            var vsTestConsole = GetMsTestFullPath();

            var trxFile = Path.GetFullPath(Path.GetRandomFileName()) + ".trx";

            var parameters = String.Format("/testContainer:\"{0}\" /resultsFile:\"{1}\"", _fileName, trxFile);
            var startInfo = new ProcessStartInfo(vsTestConsole, parameters)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            Logger.WriteLine("Executing: {0} {1}", vsTestConsole, parameters);
            using (var msTest = Process.Start(startInfo))
            {
                msTest.WaitForExit();
                var output = msTest.StandardOutput.ReadToEnd();
                var error = msTest.StandardError.ReadToEnd();
                Logger.WriteLine("Output:");
                Logger.WriteLine(output);
                Logger.WriteLine("Error:");
                Logger.WriteLine(error);
            }

            Logger.WriteLine("TrxFile: \"file://{0}\"", trxFile);
				
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