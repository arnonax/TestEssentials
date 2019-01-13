using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAutomationEssentials.MSTest
{
	/// <summary>
	/// Provide useful methods for tests
	/// </summary>
	public class TestUtils
	{
		/// <summary>
		/// Asserts that the specified exception was thrown by the given action, and returns the exception object 
		/// to allow you to further assert on its properties
		/// </summary>
		/// <typeparam name="TException">The type of the exception you expect the action to throw</typeparam>
		/// <param name="action">A delegate to the action that is expected to throw the exception</param>
		/// <returns>The caught exception</returns>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is null</exception>
		/// <exception cref="AssertFailedException">No exception of type <typeparamref name="TException"/> was 
		/// thrown by <paramref name="action"/></exception>
		public static TException ExpectException<TException>([InstantHandle]Action action)
			where TException : Exception
		{
			const string message = "Expected an exception of type {0} but it wasn't thrown";
			var messageArgs = typeof (TException).Name;
			return ExpectException<TException>(action, message, messageArgs);
		}

		/// <summary>
		/// Asserts that the specified exception was thrown by the given action, and returns the exception object 
		/// to allow you to further assert on its properties. This overload allows you to specify the assertion
		/// message and its arguments.
		/// </summary>
		/// <typeparam name="TException">The type of the exception you expect the action to throw</typeparam>
		/// <param name="action">A delegate to the action that is expected to throw the exception</param>
		/// <param name="message">The message to use in the assertion in case the exception is not thrown</param>
		/// <param name="messageArgs">Format arguments for the assertion messge</param>
		/// <returns>The caught exception</returns>
		/// <exception cref="ArgumentNullException">Either <paramref name="action"/>, <paramref name="message"/> or 
		/// <paramref name="messageArgs"/> are null</exception>
		/// <exception cref="AssertFailedException">No exception of type <typeparamref name="TException"/> was 
		/// thrown by <paramref name="action"/></exception>
		public static TException ExpectException<TException>(Action action, string message, params object[] messageArgs)
			where TException : Exception
		{
			if (action == null)
				throw new ArgumentNullException("action");

			if (message == null)
				throw new ArgumentNullException("message");

			if (messageArgs == null)
				throw new ArgumentNullException("messageArgs");

			var assertionMessage = string.Format(message, messageArgs);

			try
			{
				action();
			}
			catch (TException ex)
			{
				return ex;
			}
			throw new AssertFailedException(assertionMessage);
		}
	}
}