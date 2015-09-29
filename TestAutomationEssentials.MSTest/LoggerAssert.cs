using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest
{
	/// <summary>
	/// Wrapper around the <see cref="Assert"/> methods, which logs the verification even when it passes
	/// </summary>
	public class LoggerAssert
    {
        /// <summary>
        /// Asserts that the actual value equals to the expected one, and writes the verification to the log even when it passes
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="expectationMessage">The message to write to the log and to use in the assertion in case of a failure. Tip: use the word 'should' when you phrase the sentence</param>
        /// <param name="args">Additional arguments to the <paramref name="expectationMessage"/></param>
        /// <typeparam name="T">The type of the value to compare</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="expectationMessage"/> is null</exception>
        /// <exception cref="AssertFailedException"><paramref name="actual"/> is not equal to <paramref name="expected"/></exception>
        public static void AreEqual<T>(T expected, T actual, string expectationMessage, params object[] args)
        {
			if (expectationMessage == null)
				throw new ArgumentNullException();

            var message = string.Format(expectationMessage, args);
            Logger.WriteLine("Verifying that '{0}' equals to '{1}' ('{2}')", expected, actual, message);
            Assert.AreEqual(expected, actual, "Validation failed: " + message);
        }

        /// <summary>
        /// Asserts that the actual date/time value equals to the expected one, within the specified threshold. Writes the verification to the
        /// log even when it passes
        /// </summary>
        /// <param name="expected">The expected date/time</param>
        /// <param name="actual">The actual date/time</param>
        /// <param name="threashold">The threshold for the comparison</param>
		/// <param name="expectationMessage">The message to write to the log and to use in the assertion in case of a failure. Tip: use the word 'should' when you phrase the sentence</param>
		/// <param name="args">Additional arguments to the <paramref name="expectationMessage"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="expectationMessage"/> is null</exception>
		/// <exception cref="AssertFailedException"><paramref name="actual"/> is not within <paramref name="threashold"/> from <paramref name="expected"/></exception>
		public static void AreEqual(DateTime expected, DateTime actual, TimeSpan threashold, string expectationMessage,
            params object[] args)
        {
            IsTrue((expected - actual).Absolute() <= threashold, "'{0}' equals to '{1}'+/-'{2}' ('{3}')",
                expected, actual, threashold, string.Format(expectationMessage, args));
        }

		/// <summary>
		/// Asserts that the specified condition is true, and writes the verification to the log even when it passes
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
		/// <param name="expectationMessage">The message to write to the log and to use in the assertion in case of a failure. Tip: use the word 'should' when you phrase the sentence</param>
		/// <param name="args">Additional arguments to the <paramref name="expectationMessage"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="expectationMessage"/> is null</exception>
		/// <exception cref="AssertFailedException"><paramref name="condition"/> is false</exception>
		public static void IsTrue(bool condition, string expectationMessage, params object[] args)
        {
			if (expectationMessage == null)
				throw new ArgumentNullException("expectationMessage");

            var message = string.Format(expectationMessage, args);
            Logger.WriteLine("Verifying that condition is true: '{0}'", message);
            Assert.IsTrue(condition, "Validation failed: " + message);
        }
    }
}
