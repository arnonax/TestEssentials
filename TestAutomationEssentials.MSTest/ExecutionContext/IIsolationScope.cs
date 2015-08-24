using System;

namespace TestAutomationEssentials.MSTest.ExecutionContext
{
	/// <summary>
	/// Represents a scope of isolation for tests. Delegates to cleanup actions can be added to the scope at runtime, and they should be called 
	/// in reverse order when the scope ends.
	/// </summary>
	public interface IIsolationScope
	{
		/// <summary>
		/// Adds a delegate to an action to be called on cleanup
		/// </summary>
		/// <param name="cleanupAction">The delegate to the action to perform</param>
		/// <exception cref="ArgumentNullException"><paramref name="cleanupAction"/> is null</exception>
		void AddCleanupAction(Action cleanupAction);
	}
}