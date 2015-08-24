using System;
using System.Collections.Generic;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest.ExecutionContext
{
	/// <summary>
	/// Managed nestable scopes of isolation. Upon exit from each scope, it calls the cleanup actions that were registered to it during its lifetime
	/// </summary>
    public class TestExecutionScopesManager : IIsolationScope
    {
	    private class IsolationLevel : IIsolationScope
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

		/// <summary>
		/// Initializes a new TestExecutionScopesManager object, with one (default) isolation scope
		/// </summary>
		/// <param name="name">The name of the isolation scope</param>
		/// <param name="initialize">A delegate to an action that is performed on initialization. If an exception occurs inside this 
		/// method, then the scope is automatically destroyed, calling any cleanup actions that were added during this method</param>
	    public TestExecutionScopesManager(string name, Action<IIsolationScope> initialize)
        {
			BeginIsolationScope(name, initialize);
        }

		/// <summary>
		/// Adds a delegate to an action that will be executed on cleanup
		/// </summary>
		/// <param name="action">A delegate to the action to perform</param>
		public void AddCleanupAction(Action action)
        {
	        if (_currentState == State.Cleanup)
		        throw new InvalidOperationException("Adding cleanup actions from within cleanup is not supported");

	        _currentIsolationLevel.AddCleanupAction(action);
        }

		/// <summary>
		/// Begins a new, nested, isolation scope
		/// </summary>
		/// <param name="isolationScopeName">The name of the new isolation scope</param>
		/// <param name="initialize">A delegate to an action that is performed on initialization. If an exception occurs inside this 
		/// method, then the scope is automatically destroyed, calling any cleanup actions that were added during this method</param>
	    public void BeginIsolationScope(string isolationScopeName, Action<IIsolationScope> initialize)
	    {
		    if (initialize == null)
			    initialize = Functions.EmptyAction<IIsolationScope>();

			_currentState = State.Initialize;
		    var lastIsolationLevel = _currentIsolationLevel;
			_currentIsolationLevel = new IsolationLevel(isolationScopeName);

			Console.WriteLine("***************************** Initializing " + isolationScopeName + " *****************************");
			try
			{
				initialize(this);
				Console.WriteLine("***************************** Initializing " + isolationScopeName + " Completed succesfully *****************************");
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

		/// <summary>
		/// Ends the current isolation scope, calling all cleanup actions that were added to this scope in reverse order
		/// </summary>
	    public void EndIsolationScope()
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