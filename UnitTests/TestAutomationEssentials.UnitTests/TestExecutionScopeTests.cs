using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;
using TestAutomationEssentials.MSTest.ExecutionContext;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage]
	public class TestExecutionScopeTests
	{
		[TestMethod]
		public void NoCleanupActions()
		{
			var context = new TestExecutionScopesManager("dummy", ctx => { });
			context.EndIsolationScope();
		}

		[TestMethod]
		public void CleanupActionIsCalledFromInitialization()
		{
			bool cleanupWasCalled = false;
			
			TestUtils.ExpectException<Exception>(() => new TestExecutionScopesManager("dummy", ctx =>
			{
				ctx.AddCleanupAction(() => cleanupWasCalled = true);
				throw new Exception();
			}));

			Assert.IsTrue(cleanupWasCalled);
		}

		[TestMethod]
		public void CleaupActionIsCalledAfterInitialize()
		{
			bool cleanupWasCalled = false;
			var context = new TestExecutionScopesManager("dummy", ctx => { });
			context.AddCleanupAction(() => cleanupWasCalled = true);

			context.EndIsolationScope();
			Assert.IsTrue(cleanupWasCalled);
		}

		[TestMethod]
		public void CleanupActionsAddedInInitializeAreCalledAlsoFromRegularCleanup()
		{
			bool cleanupWasCalled = false;
			var context = new TestExecutionScopesManager("dummy", ctx => ctx.AddCleanupAction(() => cleanupWasCalled = true));

			context.EndIsolationScope();
			Assert.IsTrue(cleanupWasCalled);
		}

		[TestMethod]
		public void CleanupActionInNestedIsolationLevelIsCalledOnlyOnPop()
		{
			bool cleaupWasCalled = false;
			var context = new TestExecutionScopesManager("dummy", ctx => { });
			context.BeginIsolationScope("dummyIsolationLevel", ctx => { });
			context.AddCleanupAction(() => cleaupWasCalled = true);
			context.EndIsolationScope();
			Assert.IsTrue(cleaupWasCalled);
			cleaupWasCalled = false;
			context.EndIsolationScope();
			Assert.IsFalse(cleaupWasCalled);
		}

		[TestMethod]
		public void ExceptionInNestedLevelInitialization()
		{
			var calledActions = new List<string>();
			var context = new TestExecutionScopesManager("dummy", ctx => { });
			context.AddCleanupAction(() => calledActions.Add("action1"));

			var ex = TestUtils.ExpectException<Exception>(() => context.BeginIsolationScope("nested", ctx =>
			{
				context.AddCleanupAction(() => calledActions.Add("action2"));
				throw new Exception("DummyExceptionMessage");
			}));
			Assert.AreEqual("DummyExceptionMessage", ex.Message);

			Assert.AreEqual("action2", calledActions.Content());

			calledActions.Clear();
			context.EndIsolationScope();
			Assert.AreEqual("action1", calledActions.Content());
		}

		[TestMethod]
		public void ExceptionIsThrownIfCallingAddCleanupActionFromWithinACleaupAction()
		{
			var context = new TestExecutionScopesManager("dummy", ctx => { });
			context.AddCleanupAction(() =>
			{
				context.AddCleanupAction(() => { Assert.Fail("This code should never be called!"); });
			});

			TestUtils.ExpectException<InvalidOperationException>(() =>
			{
				context.EndIsolationScope();
			});
		}

		[TestMethod]
		public void CanAddCleanupActionAfterOneLevelWasPoppoed()
		{
			var context = new TestExecutionScopesManager("dummy", Functions.EmptyAction<IIsolationScope>());
			context.BeginIsolationScope("Level1", Functions.EmptyAction<IIsolationScope>());
			context.EndIsolationScope();
			var cleanupCalled = false;
			context.AddCleanupAction(() => cleanupCalled = true);
			context.EndIsolationScope();
			Assert.IsTrue(cleanupCalled, "Cleanup action hasn't been called");
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
		public void DoesNothingIfInitializeIsNull()
		{
			bool outerCleanupIsCalled = false, innerCleanupIsCalled = false;

			Action<IIsolationScope> nullInitialize = null;

			var context = new TestExecutionScopesManager("OuterScope", nullInitialize);
			context.AddCleanupAction(() => outerCleanupIsCalled = true);

			context.BeginIsolationScope("InnerScope", nullInitialize);
			context.AddCleanupAction(() => innerCleanupIsCalled = true);
			
			context.EndIsolationScope();
			Assert.IsTrue(innerCleanupIsCalled);
			context.EndIsolationScope();
			Assert.IsTrue(outerCleanupIsCalled);
		}

		[TestMethod]
		public void WhenMultipleCleanupActionsThrowExceptionsAnAggregatedExceptionIsThrown()
		{
			var context = new TestExecutionScopesManager("dummy", Functions.EmptyAction<IIsolationScope>());
			var ex1 = new Exception("1st Exception");
			var ex2 = new Exception("2nd Exception");

			context.AddCleanupAction(() =>
			{
				throw ex1;
			});

			context.AddCleanupAction(() => 
			{ 
				throw ex2;
			});

			var aggregatedEx = TestUtils.ExpectException<AggregateException>(() => context.EndIsolationScope());

			Assert.AreEqual(2, aggregatedEx.InnerExceptions.Count, "Invalid number of inner exceptions");
			Assert.IsTrue(aggregatedEx.InnerExceptions.Contains(ex1), "1st exception is not found in the aggergate exception");
			Assert.IsTrue(aggregatedEx.InnerExceptions.Contains(ex2), "2nd exception is not found in the aggergate exception");
		}
	}
}