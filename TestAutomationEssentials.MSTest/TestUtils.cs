using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAutomationEssentials.MSTest
{
	public class TestUtils
	{
		public static TException ExpectException<TException>(Action action)
			where TException : Exception
		{
			const string message = "Expected an exception of type {0} but it wasn't thrown";
			var messageArgs = typeof (TException).Name;
			return ExpectException<TException>(action, message, messageArgs);
		}

		public static TException ExpectException<TException>(Action action, string message, params object[] messageArgs)
			where TException : Exception
		{
			try
			{
				action();
			}
			catch (TException ex)
			{
				return ex;
			}
			throw new AssertFailedException(string.Format(message, messageArgs));
		}
	}
}