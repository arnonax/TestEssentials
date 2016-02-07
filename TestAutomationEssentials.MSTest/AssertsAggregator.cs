using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest
{
    /// <summary>
    /// Performs a sequence of assertions, reporting the results of all of them indenpendently from one another
    /// </summary>
    /// <remarks>
    /// <para>
    /// In most cases, after one assertion fails, the test should stop and not continue to the rest of the test. However, there are cases
    /// where a test should few related things, even though each can fail indenpendently from one anther. In this case, you can use this class
    /// to make sure you get the results of all the assertions and only the first one that fails. For example, If the test should assert that
    /// a message box appears with certain text, certain title and a certain icon - each of these assertions are independent from one another
    /// even though they validate the result of the same operation.
	/// </para>
	/// <para>
	/// This class evaluates and writes the result of each assertion to the log, immediately when one of the assertion methods is called, but 
	/// throws an <see cref="AssertFailedException"/> only when <see cref="Dispose"/> is called if one or more assertions have failed. Normally,
	/// instead of calling <see cref="Dispose"/> directly, you would use the <code>using</code> keyword.
	/// </para>
    /// </remarks>
    /// <example>
    /// using(var asserts = new AssertsAggregator("Message Box")
    /// {
    ///		asserts.AreEqual("Add customer", () => messageBox.Title, "Title");
    ///		asserts.AreEqual("The customer you tried to add already exists", () => messageBox.Text, "Text");
    ///		asserts.AreEqual(MessageBoxIcon.Error, () => messageBox.Icon, "Icon");
    /// }
    /// </example>
    public class AssertsAggregator : IDisposable
    {
        private readonly string _description;
        private bool _failed;
        private readonly IDisposable _loggerSection;

        /// <summary>
        /// Initializes a new instance of <see cref="AssertsAggregator"/> with the spcified description that is written to the log
        /// </summary>
        /// <param name="description">The description for the section that is written to the log</param>
        public AssertsAggregator(string description)
        {
            _description = description;
            _loggerSection = Logger.StartSection("Verifying: {0}", description);
        }

	    /// <summary>
	    /// Closes the evaluation section in the log and thrown an <see cref="AssertFailedException"/> if one of the assertions in it failed.
	    /// </summary>
	    /// <exception cref="AssertFailedException">One or more of the assertions in the scope failed</exception>
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

        /// <summary>
        /// Verifies that the actual value matches the expected one
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="getActual">A delegate to a method that returns the actual value</param>
        /// <param name="validationMessage">The message that describes the validation, to be written in the log and to be reported in case of a failure</param>
        /// <param name="args">Additional format arguments to <paramref name="validationMessage"/></param>
        /// <typeparam name="T">The type of the value to compare</typeparam>
        /// <remarks>
        /// This method calls the method delegated by <paramref name="getActual"/> inside a try/catch clause, so that if either the comparison
        /// failed or the <paramref name="getActual"/> method failed, the program continues (mainly to reach the next assertions). The 
        /// assertion's result is written to the log (whether it succeeded or failed), and only after <see cref="Dispose"/> is called, an
        /// <see cref="AssertFailedException"/> exception will be thrown.
        /// </remarks>
        public void AreEqual<T>(T expected, Func<T> getActual, string validationMessage, params object[] args)
        {
            Try(() =>
            {
                LoggerAssert.AreEqual(expected, getActual(), validationMessage, args);
            });
        }

        /// <summary>
        /// Verifies the the specified condition is true
        /// </summary>
        /// <param name="condition">A delegate to a method that evaluates the condition</param>
		/// <param name="validationMessage">The message that describes the validation, to be written in the log and to be reported in case of a failure</param>
		/// <param name="args">Additional format arguments to <paramref name="validationMessage"/></param>
		/// <remarks>
		/// This method calls the method delegated by <paramref name="condition"/> inside a try/catch clause, so that if either it returns
		/// false or if fails, the program continues (mainly to reach the next assertions). The assertion's result is written to the log 
		/// (whether it succeeded or failed), and only after <see cref="Dispose"/> is called, an <see cref="AssertFailedException"/> exception 
		/// will be thrown.
		/// </remarks>
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