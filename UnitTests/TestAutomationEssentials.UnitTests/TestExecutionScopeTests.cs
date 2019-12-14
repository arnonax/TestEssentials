using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.Common.ExecutionContext;

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
			
			Assert.ThrowsException<Exception>(() => new TestExecutionScopesManager("dummy", ctx =>
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

			var ex = Assert.ThrowsException<Exception>(() => context.BeginIsolationScope("nested", ctx =>
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

			Assert.ThrowsException<InvalidOperationException>(() =>
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

			context.AddCleanupAction(() => throw ex1);

			context.AddCleanupAction(() => throw ex2);

			var aggregatedEx = Assert.ThrowsException<AggregateException>(() => context.EndIsolationScope());

			Assert.AreEqual(2, aggregatedEx.InnerExceptions.Count, "Invalid number of inner exceptions");
			Assert.IsTrue(aggregatedEx.InnerExceptions.Contains(ex1), "1st exception is not found in the aggergate exception");
			Assert.IsTrue(aggregatedEx.InnerExceptions.Contains(ex2), "2nd exception is not found in the aggergate exception");
		}

		[TestMethod]
		public void StackTraceOfCleanupActionContainsTheOriginalLineItWasThrown()
		{
			var manager = new TestExecutionScopesManager("dummy", Functions.EmptyAction<IIsolationScope>());
			manager.AddCleanupAction(MethodThatThrowsException);

			var ex = Assert.ThrowsException<Exception>((Action)manager.EndIsolationScope);
			
			StringAssert.Contains(ex.StackTrace, "MethodThatThrowsException");
		}

	    private static void MethodThatThrowsException()
	    {
	        throw new Exception("dummy exception");
	    }

        [TestMethod]
        public void ExceptionInInitializeFollowedByAnExceptionInCleanupShouldBeThrownAsAggregateExceptions()
        {
            const string exceptionInCleanup = "Exception in cleanup";
            const string originalException = "Original exception";
            Action<IIsolationScope> initialize = scope =>
            {
                scope.AddCleanupAction(() =>
                {
                    throw new Exception(exceptionInCleanup);
                });

                throw new Exception(originalException);
            };
            var ex = Assert.ThrowsException<AggregateException>(() => new TestExecutionScopesManager("dummy", initialize));
            CollectionAssert.AreEquivalent(
                ex.InnerExceptions.Select(innerEx => innerEx.Message).ToList(),
                new[] { exceptionInCleanup, originalException });
        }
	}
}