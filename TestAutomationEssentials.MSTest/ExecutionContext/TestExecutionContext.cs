using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest.ExecutionContext
{
    public class TestExecutionContext : IIsolationContext
    {
	    private class IsolationLevel : IIsolationContext
	    {
			private readonly Stack<Action> _cleanupActions = new Stack<Action>();

		    public IsolationLevel(string name)
		    {
				Name = name;
		    }

		    public string Name { get; private set; }

		    public void Cleanup()
		    {
				List<Exception> exceptions = new List<Exception>();
				while (!_cleanupActions.IsEmpty())
				{
					var action = _cleanupActions.Pop();
					try
					{
						action();
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
						Console.WriteLine("Exception occured in cleanup. Resuming to additional cleanup actions if exists, though they may fail too.");
						Console.WriteLine(ex);
					}
				}

			    switch (exceptions.Count)
			    {
				    case 0:
					    return;
				    case 1:
					    throw exceptions.Content();
				    default:
					    throw new AggregateException("Multiple exception occured during Cleanup", exceptions);
			    }
		    }

		    public void AddCleanupAction(Action action)
		    {
			    _cleanupActions.Push(action);
		    }
	    }

	    private enum State
	    {
		    Initialize,
			Normal,
			Cleanup
	    }
	    private IsolationLevel _currentIsolationLevel;
	    private State _currentState = State.Initialize;
	    private readonly Stack<IsolationLevel> _isolationLevels = new Stack<IsolationLevel>();

	    public TestExecutionContext(string name, Action<IIsolationContext> initialize)
        {
			PushIsolationLevel(name, initialize);
        }

        public void Cleanup()
        {
			CleanupCurrentLevel();
			Assert.AreEqual(0, _isolationLevels.Count, "Some isolation levels were not been cleaned!");
        }

        public void AddCleanupAction(Action action)
        {
	        if (_currentState == State.Cleanup)
		        throw new InvalidOperationException("Adding cleanup actions from within cleanup is not supported");

	        _currentIsolationLevel.AddCleanupAction(action);
        }

	    public void PushIsolationLevel(string isolationLevelName, Action<IIsolationContext> initialize)
	    {
			_currentState = State.Initialize;
		    var lastIsolationLevel = _currentIsolationLevel;
			_currentIsolationLevel = new IsolationLevel(isolationLevelName);

			Console.WriteLine("***************************** Initializing " + isolationLevelName + " *****************************");
			try
			{
				initialize(this);
				Console.WriteLine("***************************** Initializing " + isolationLevelName + " Completed succesfully *****************************");
			}
			catch
			{
				_currentIsolationLevel.Cleanup();
				_currentIsolationLevel = lastIsolationLevel;
				throw;
			}

			if (lastIsolationLevel != null)
				_isolationLevels.Push(lastIsolationLevel);

			_currentState = State.Normal;
	    }

	    public void PopIsolationLevel()
	    {
		    CleanupCurrentLevel();
		    _currentIsolationLevel = _isolationLevels.Pop();
	    }

	    private void CleanupCurrentLevel()
	    {
			_currentState = State.Cleanup;
		    Console.WriteLine("***************************** Cleanup " + _currentIsolationLevel.Name +
							  " *****************************");

		    _currentIsolationLevel.Cleanup();

			_currentState = State.Normal;
	    }
    }
}