using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
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


        public static void AreEqual(DateTime expected, DateTime actual, string expectationMessage,
            params object[] args)
        {
            IsTrue(Equals(expected,actual), "'{0}' equals to '{1}'", expected, actual, string.Format(expectationMessage, args));
        }


        public static void AreEqual(Image expectedImage, Image actualImage, string expectationMessage, params object[] args)
        {
            var expectedBytes = expectedImage.GetBitmapBytes();
            var actualBytes = actualImage.GetBitmapBytes();

            Logger.WriteLine("Verifying that images are equals");
            if (!(expectedBytes.SequenceEqual(actualBytes)))
            {
                // AddResultImage(expectedImage, "Expected");
                //AddResultImage(actualImage, "Actual");
                Assert.Fail(expectationMessage, args);
            }
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

        public static void AreNotEqual<T>(T expected, T actual, string validationMessage, params object[] args)
        {
            var message = string.Format(validationMessage, args);
            Logger.WriteLine("Verifying that '{0}' is different than '{1}' ('{2}')", expected, actual, message);
            Assert.AreNotEqual(expected, actual, "Validation failed: " + message);
        }

        public static void IsFalse(bool condition, string validationMessage, params object[] args)
        {
            var message = string.Format(validationMessage, args);
            Logger.WriteLine("Verifying that condition is false: '{0}'", message);
            Assert.IsFalse(condition, "Validation failed: " + message);
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string validationMessage, params object[] args)
        {
            // TODO: improve the message in case of failure
            var message = string.Format(validationMessage, args);
            Logger.WriteLine("Verifying that the collections are equal ('{0}')", message);
            CollectionAssert.AreEqual(actual.ToArray(), expected.ToArray(), message);
        }

        /// <summary>
        /// Asserts that the actual string equals to the expected one removing whitespace and upper case, and writes the verification to the log even when it passes
        /// </summary>
        /// <param name="expected">The expected string</param>
        /// <param name="actual">The actual string</param>
        /// <param name="expectationMessage">The message to write to the log and to use in the assertion in case of a failure. Tip: use the word 'should' when you phrase the sentence</param>
        /// <param name="args">Additional arguments to the <paramref name="expectationMessage"/></param>
        public static void AreEqual(string expected, string actual, string expectationMessage, params object[] args)
        {
            if (expectationMessage == null)
                throw new ArgumentNullException();

            var message = string.Format(expectationMessage, args);
            Logger.WriteLine("Verifying that '{0}' equals to '{1}' ('{2}')", expected, actual, message);
            Assert.AreEqual(RemoveWhitespaceAndUpperCase(expected), RemoveWhitespaceAndUpperCase(actual), "Validation failed: " + message + " (Ignoring whitespace and upper case)");
        }

        private static string RemoveWhitespaceAndUpperCase(string str)
        {
            str = str.Replace(" ", string.Empty);
            return str.ToLower();
        }

        /// <summary>
        /// Asserts that the value string contains the substring, and writes the verification to the log even when it passes
        /// </summary>
        /// <param name="value">The Full string</param>
        /// <param name="substring">The substring</param>
        /// <param name="validationMessage">The message to write to the log and to use in the assertion in case of a failure. Tip: use the word 'should' when you phrase the sentence</param>
        /// <param name="args">Additional arguments to the <paramref name="validationMessage"/></param>
        public static void Contains(string value, string substring, string validationMessage, params object[] args)
        {
            var message = string.Format(validationMessage, args);
            Logger.WriteLine("Verifying that '{0}' contains '{1}' ('{2}')", value, substring, message);
            StringAssert.Contains(value, substring, message);
        }
    }
}
