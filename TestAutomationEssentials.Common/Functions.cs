using System;
using System.Diagnostics.CodeAnalysis;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Provides general purpose methods that are useful for working in a functional manner in C#
	/// </summary>
	public static class Functions
	{
		/// <summary>
		/// Returns a new function that negates the result of another boolean function
		/// </summary>
		/// <param name="func">The original boolean function</param>
		/// <returns>A new method that returns the opposite result of <paramref name="func"/></returns>
		public static Func<bool> Negate(this Func<bool> func)
		{
			if (func == null)
				throw new ArgumentNullException("func");

			return () => !func();
		}

		/// <summary>
		/// Returns a delegate to an <see cref="Action"/> that does nothing
		/// </summary>
		/// <returns>A delegate to an <see cref="Action"/> that does nothing</returns>
		[ExcludeFromCodeCoverage] // Syntactic sugar - nothing to test
		public static Action EmptyAction()
		{
			return DoNothing;
		}

		/// <summary>
		/// Returns a delegate to an <see cref="Action{T}"/> that does nothing
		/// </summary>
		/// <returns>A delegate to an <see cref="Action{T}"/> that does nothing</returns>
		[ExcludeFromCodeCoverage] // Syntactic sugar - nothing to test
		public static Action<T> EmptyAction<T>()
		{
			return DoNothing<T>;
		}

		[ExcludeFromCodeCoverage]
		private static void DoNothing<T>(T dummy)
		{	
		}

		[ExcludeFromCodeCoverage]
		private static void DoNothing()
		{
		}

	}
}
