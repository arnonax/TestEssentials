using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.UnitTests;

namespace TestAutomationEssentials.MSTestV2UnitTests
{
    [TestClass, Ignore("The normal TestBaseTest class already tests V2. This project is about to be removed")]
    public class TestBaseTestsV2 : CommonTestBaseTests
    {
        protected override ITestClass CreateTestClass(string dllName, TestContext testContext)
        {
            return new TestClassWrapperV2(dllName, testContext);
        }

        protected override IEnumerable<string> GetAdditionalReferences()
        {
            yield return "System.Runtime.dll";
            yield return typeof(TestContext).Assembly.Location;
        }

        public class TestClassWrapperV2 : ITestClass
        {
            private readonly string _dllName;
            private readonly TestContext _testContext;

            public TestClassWrapperV2(string dllName, TestContext testContext)
            {
                _dllName = dllName;
                _testContext = testContext;
            }

            public TestResults Execute()
            {
                var msTestFullPath = GetVsTestConsoleFullPath();

                const string testResultsFolder = "TestResults";
                if (Directory.Exists(testResultsFolder))
                    Directory.Delete(testResultsFolder, true);

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

                var trxFile = new DirectoryInfo(testResultsFolder).EnumerateFiles("*.trx").Content().FullName;
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
}
