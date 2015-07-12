using System;
using System.Diagnostics.CodeAnalysis;

namespace TestAutomationEssentials.Common
{
	public static class Functions
	{
		public static Func<bool> Negate(this Func<bool> func)
		{
			return () => !func();
		}

		[ExcludeFromCodeCoverage] // Syntactic sugar - nothing to test
		public static Action EmptyAction()
		{
			return DoNothing;
		}

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
