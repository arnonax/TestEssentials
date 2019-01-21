using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.UnitTests
{
    internal class TestClassWrapper : ITestClass
    {
        private readonly string _dllName;
        private readonly TestContext _testContext;

        public TestClassWrapper(string dllName, TestContext testContext)
        {
            _dllName = dllName;
            _testContext = testContext;
        }

        public TestResults Execute()
        {
            var msTestFullPath = GetVsTestConsoleFullPath();

            var parameters = $"{_dllName} /UseVsixExtensions:true /logger:trx /TestAdapterPath:.";

            var startInfo = new ProcessStartInfo(msTestFullPath, parameters)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            Logger.WriteLine("Executing: {0} {1}", msTestFullPath, parameters);
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

            const string testResultsFolder = "TestResults";
            var trxFiles = new DirectoryInfo(testResultsFolder).EnumerateFiles("*.trx").OrderByDescending(file => file.LastWriteTime);
            var trxFile = trxFiles.First().FullName;
            Logger.WriteLine("TrxFile: \"file://{0}\"", trxFile);

            _testContext.AddResultFile(trxFile);

            return new TestResults(trxFile);
        }

        private static string GetVsTestConsoleFullPath()
        {
            var currentProcessPath = Process.GetCurrentProcess().Modules[0].FileName;
            var currentProcessFolder = Path.GetDirectoryName(currentProcessPath);
            return Path.Combine(currentProcessFolder, "vstest.console.exe");
        }
    }
}