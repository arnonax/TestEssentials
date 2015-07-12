using System;

namespace TestAutomationEssentials.MSTest.ExecutionContext
{
	public interface IIsolationContext
	{
		void AddCleanupAction(Action cleanupAction);
	}
}