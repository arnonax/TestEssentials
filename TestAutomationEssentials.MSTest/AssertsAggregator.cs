using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest
{
    public class AssertsAggregator : IDisposable
    {
        private readonly string _description;
        private bool _failed;
        private readonly IDisposable _loggerSection;

        public AssertsAggregator(string description)
        {
            _description = description;
            _loggerSection = Logger.StartSection("Verifying: {0}", description);
        }

        public void Dispose()
        {
            try
            {
                if (_failed)
                    return;

                Assert.Fail("Verfying '{0}' failed. See log for details.", _description);
            }
            finally
            {
                _loggerSection.Dispose();
            }
        }

        public void AreEqual<T>(T expected, Func<T> getActual, string validationMessage, params object[] args)
        {
            Try(() =>
            {
                LoggerAssert.AreEqual(expected, getActual(), validationMessage, args);
            });
        }

        public void IsTrue(Func<bool> condition, string validationMessage, params object[] args)
        {
            Try(() =>
            {
                LoggerAssert.IsTrue(condition(), validationMessage, args);
            });
        }

        private void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _failed = true;
                Logger.WriteLine("Assertion fail: " + ex);
            }
        }
    }
}