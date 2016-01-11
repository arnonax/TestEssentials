using System;
using System.Linq.Expressions;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Provide convinient methods for waiting for a condition to come true.
	/// </summary>
	public class Wait
	{
		/// <summary>
		/// Waits until the specified condition becomes true
		/// </summary>
		/// <param name="conditionExpr">A lamba expression containing the condition</param>
		/// <param name="timeout">The maximum time to wait for the condition</param>
		/// <exception cref="ArgumentNullException"><paramref name="conditionExpr"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative</exception>
		/// <exception cref="TimeoutException">The condition didn't become true for the specified timeout</exception>
		/// <example>
		/// Wait.Until(() => PageIsLoaded(), 30.Seconds());
		/// </example>
		public static void Until(Expression<Func<bool>> conditionExpr, TimeSpan timeout)
		{
			ValidateNullArgument(conditionExpr, "conditionExpr");
			ValidateTimeout(timeout);

			var timeoutMessage = "The condition '" + conditionExpr + "' has not been met for " + timeout.ToSpokenString();

			Until(conditionExpr.Compile(), timeout, timeoutMessage);
		}

		/// <summary>
		/// Waits until the specified condition becomes false
		/// </summary>
		/// <param name="conditionExpr">A lambda expression containing the condition</param>
		/// <param name="timeout">The maximum time to wait for the condition to become false</param>
		/// <exception cref="ArgumentNullException"><paramref name="conditionExpr"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative</exception>
		/// <exception cref="TimeoutException">The condition didn't become false for the specified timeout</exception>
		/// <example>
		/// Wait.While(() => PleaseWaitMessageAppears(), 30.Seconds());
		/// </example>
		public static void While(Expression<Func<bool>>  conditionExpr, TimeSpan timeout)
		{
			ValidateNullArgument(conditionExpr, "conditionExpr");
			ValidateTimeout(timeout);

			var timeoutMessage = "The condition '" + conditionExpr + "' is still true after " + timeout.ToSpokenString();
			var condition = conditionExpr.Compile();
			Until(() => !condition(), timeout, timeoutMessage);
		}

		/// <summary>
		/// Waits until the specified condition becomes true. In case of a timeout, the specified message is used in the exception
		/// </summary>
		/// <param name="condition">The condition to wait for to become true</param>
		/// <param name="timeout">The maximum time to wait for the condition to become true</param>
		/// <param name="timeoutMessage">The message to use in the exception in case of a timeout</param>
		/// <param name="args">Additional format arguments to embedd in <paramref name="timeoutMessage"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="condition"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative</exception>
		/// <exception cref="FormatException">timeoutMessage is invalid.-or- The index of a format item is less than zero, or greater
		///     than or equal to the length of the args array.</exception>
		/// <exception cref="TimeoutException">The condition didn't become true for the specified timeout. The message of the 
		/// exception is specified by the <paramref name="timeoutMessage"/> argument</exception>
		/// <example>
		/// Wait.Until(() => PageIsLoaded(), 30.Seconds(), "Page wasn't loaded!");
		/// </example>
		public static void Until(Func<bool> condition, TimeSpan timeout, string timeoutMessage, params object[] args)
		{
			ValidateNullArgument(condition, "condition");
			ValidateNullArgument(timeoutMessage, "timeoutMessage");
			ValidateNullArgument(args, "args");
			ValidateTimeout(timeout);

			var conditionMet = IfNot(condition, timeout);
			if (!conditionMet)
				throw new TimeoutException(string.Format(timeoutMessage, args));
		}

		/// <summary>
		/// Evaluates the specified expression until it meets the specified condition, and return the value of this expression
		/// </summary>
		/// <param name="getResultExpr">The expression to evaulate</param>
		/// <param name="conditionExpr">The condition to wait for to become true</param>
		/// <param name="timeout">The maximum time to wait for the condition to become true</param>
		/// <typeparam name="T">The type that the expression resolves to</typeparam>
		/// <returns>The value of the expression when it meets the condition</returns>
		/// <exception cref="ArgumentNullException"><paramref name="getResultExpr"/> or <paramref name="conditionExpr"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative</exception>
		/// <exception cref="TimeoutException">The condition didn't become true for the specified timeout</exception>
		/// <example>
		/// // Waits until the quote reaches 10 and returns it
		/// Quote quote = Wait.Until(() => GetQuote(), qt => qt.Value > 10, 5.Minutes());
		/// Console.WriteLine(quote.Name);
		/// </example>
		/// <remarks>
		/// When this method throws <see cref="TimeoutException"/>, it uses the expressions as part of the exception's message
		/// </remarks>
		public static T Until<T>(Expression<Func<T>> getResultExpr, Expression<Func<T, bool>> conditionExpr, TimeSpan timeout)
	    {
			ValidateNullArgument(getResultExpr, "getResultExpr");
			ValidateNullArgument(conditionExpr, "conditionExpr");
	        var compiledGetResult = getResultExpr.Compile();
	        var compiledCondition = conditionExpr.Compile();

            var timeoutMessage = "The condition '" + conditionExpr + "' on '" + getResultExpr + "' has not been met for " + timeout.ToSpokenString();

	        return Until(compiledGetResult, compiledCondition, timeout, timeoutMessage);
	    }

		/// <summary>
		/// Invokes the specified delegate until its return value meets the specified condition, and returns that value
		/// </summary>
		/// <param name="getResult">The delegate to invoke</param>
		/// <param name="condition">The condition to wait for to become true</param>
		/// <param name="timeout">The maximum time to wait for the condition to become true</param>
		/// <param name="timeoutMessage">The message to use in the exception in case of a timeout</param>
		/// <param name="args">Additional format arguments to embedd in <paramref name="timeoutMessage"/></param>
		/// <typeparam name="T">The type that the delegate returns</typeparam>
		/// <returns>The return value of the delegate when it meets the condition</returns>
		/// <exception cref="ArgumentNullException"><paramref name="getResult"/>, <paramref name="condition"/>, <paramref name="timeout"/> or <paramref name="args"/>is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative</exception>
		/// <exception cref="TimeoutException">The condition didn't become true for the specified timeout</exception>
		/// <exception cref="FormatException">timeoutMessage is invalid.-or- The index of a format item is less than zero, or greater
		///     than or equal to the length of the args array.</exception>
		/// <example>
		/// // Waits until the quote reaches 10 and returns it
		/// Quote quote = Wait.Until(() => GetQuote(), qt => qt.Value > 10, 5.Minutes(), "Quote didn't reach {0}", 10);
		/// </example>
		/// <remarks>
		/// When this method throws <see cref="TimeoutException"/> it uses the <paramref name="timeoutMessage"/> and its corresponding 
		/// <paramref name="args"/> as the message of the exception
		/// </remarks>
		public static T Until<T>(Func<T> getResult, Func<T, bool> condition, TimeSpan timeout, string timeoutMessage, params object[] args)
	    {
			ValidateNullArgument(getResult, "getResult");
			ValidateNullArgument(condition, "condition");

	        var result = default(T);
	        Until(() =>
	        {
	            result = getResult();
	            return condition(result);
	        }, timeout, timeoutMessage, args);

	        return result;
	    }

	    /// <summary>
		/// Waits until the specified condition is met or until the specified period has passed, whichever comes first. This method doesn't throw TimeoutException.
		/// </summary>
		/// <param name="condition">The condition to evaluate</param>
		/// <param name="period">The period to wait for the condition</param>
		/// <returns>Whether the condition has been met</returns>
		/// <exception cref="ArgumentNullException"><paramref name="condition"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is negative</exception>
		/// <remarks>
		/// You should use it for non critical and possibly very short conditions that the polling may miss. After calling this method you should verify (or wait) 
		/// for a different condition that indicates that the operation has actually completed or not.
		/// </remarks>
		public static bool IfNot(Func<bool> condition, TimeSpan period)
		{
			ValidateNullArgument(condition, "condition");
			ValidateTimeout(period, "period");

			var endTime = DateTime.Now + period;
			bool conditionMet;
			do
			{
				if (DateTime.Now > endTime)
				{
					return false;
				}

				conditionMet = condition();
			} while (!conditionMet);
			return true;
		}

		// ReSharper disable once UnusedParameter.Local

		/// <summary>
		/// Waits while the specified condition is met or until the specified period has passed. This method doesn't throw TimeoutException.
		/// </summary>
		/// <param name="condition">The condition to evaluate</param>
		/// <param name="period">The period to wait for the condition</param>
		/// <exception cref="ArgumentNullException"><paramref name="condition"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is negative</exception>
		/// <returns>Whether the condition has been met</returns>
		/// <remarks>
		/// You should use it for non critical and possibly very short conditions that the polling may miss. After calling this method you should verify (or wait) 
		/// for a different condition that indicates that the operation has actually completed or not.
		/// </remarks>
		public static bool If(Func<bool> condition, TimeSpan period)
		{
			ValidateNullArgument(condition, "condition");

			// TODO: move the main implementation to this method and change IfNot to call this one. This should resolve the double negation and simplify the code a little
			return IfNot(condition.Negate(), period);
		}

		// ReSharper disable once UnusedParameter.Local
		private static void ValidateNullArgument(object arg, string argName)
		{
			if (arg == null)
				throw new ArgumentNullException(argName);
		}

		// ReSharper disable once UnusedParameter.Local
		private static void ValidateTimeout(TimeSpan timeSpan, string paramName = "timeout")
		{
			if (timeSpan < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(paramName);
		}
	}
}