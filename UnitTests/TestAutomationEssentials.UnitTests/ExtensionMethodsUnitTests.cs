using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage]
	public class ExtensionMethodsUnitTests
	{
		private static readonly string IntTypeName = typeof(int).Name;

		[TestMethod]
		public void IsEmptyReturnTrueIfTheSequenceIsEmpty()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var emptyList = new List<int>();
			Assert.IsTrue(emptyList.IsEmpty());
		}

		[TestMethod]
		public void IsEmptyReturnsFalseIfTheSequenceContains1Element()
		{
			var listWith1Element = new List<int> {3};
			Assert.IsFalse(listWith1Element.IsEmpty());
		}

		[TestMethod]
		public void IsEmptyReturnsFalseIfTheSequenceContains3Elements()
		{
			var listWith3Elements = new List<int> {1, 2, 3};
			Assert.IsFalse(listWith3Elements.IsEmpty());
		}

		[TestMethod]
		public void SubArrayWithStartAndLength()
		{
			var arr = new[] {1, 2, 3, 4, 5};
			CollectionAssert.AreEqual(new[] {2, 3, 4}, arr.SubArray(1, 3));
		}

		[TestMethod]
		public void SubArrayWithoutLengthReturnsAllTheElementsFromTheStartIndexToTheEnd()
		{
			var arr = new[] {1, 2, 3, 4, 5};
			CollectionAssert.AreEqual(new[] {2, 3,4, 5}, arr.SubArray(1));
		}

		[TestMethod]
		public void _2MinutesAre120000Milliseconds()
		{
			Assert.AreEqual(120000, 2.MinutesAsMilliseconds());
		}

		[TestMethod]
		public void _2SecondsAre2000Milliseconds()
		{
			Assert.AreEqual(2000, 2.SecondsAsMilliseconds());
		}

		[TestMethod]
		public void MillisecondsAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromMilliseconds(1234), 1234.Milliseconds());
		}

		[TestMethod]
		public void SecondsAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromSeconds(1234), 1234.Seconds());
		}

		[TestMethod]
		public void MinutesAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromMinutes(1234), 1234.Minutes());
		}

		[TestMethod]
		public void HoursAsTimeSpan()
		{
			Assert.AreEqual(TimeSpan.FromHours(1234), 1234.Hours());
		}

		[TestMethod]
		public void TimeSpanToSpokenString()
		{
			Assert.AreEqual("0 seconds", TimeSpan.Zero.ToSpokenString());
			Assert.AreEqual("0.001 seconds", 1.Milliseconds().ToSpokenString());
			Assert.AreEqual("1 second", 1.Seconds().ToSpokenString());
			Assert.AreEqual("1.001 seconds", (1.Seconds() + 1.Milliseconds()).ToSpokenString());
			Assert.AreEqual("2 seconds", 2.Seconds().ToSpokenString());
			Assert.AreEqual("1 minute", 1.Minutes().ToSpokenString());
			Assert.AreEqual("1:01 minutes",  (1.Minutes() + 1.Seconds()).ToSpokenString());
			Assert.AreEqual("1:01 minutes", (1.Minutes() + 1.Seconds() + 1.Milliseconds()).ToSpokenString());
			Assert.AreEqual("2 minutes", 2.Minutes().ToSpokenString());
			Assert.AreEqual("1 hour", 1.Hours().ToSpokenString());
			Assert.AreEqual("1:01 hours", (1.Hours() + 1.Minutes() + 1.Seconds() + 1.Milliseconds()).ToSpokenString());
		}

		[DllImport("user32.dll")]
		static extern int SetWindowText(IntPtr hWnd, string text);

		[TestMethod]
		public void GetCurrentWindowTitleReturnsTheUpToDateTitle()
		{
			var process = Process.Start("notepad.exe");
			try
			{
				var isIdle = process.WaitForInputIdle(2000);
				Assert.IsTrue(isIdle, "notepad didn't reach idle state");

				const string newTitle = "TestNotepad";
				var oldTitle = process.MainWindowTitle;
				SetWindowText(process.MainWindowHandle, newTitle);

				Assert.AreEqual(oldTitle, process.MainWindowTitle, "Process.MainWindowTitle already returns the up to date title, so GetCurrentMainWindowTitle is irrelevant...");
				Assert.AreEqual(newTitle, process.GetCurrentMainWindowTitle(), "GetCurrentMainWindowTitle didn't return the new title");
			}
			finally
			{
				process.Kill();
				process.Dispose();
			}
		}

		[TestMethod]
		public void ContentDisplaysClearErrorMessageWhenNoMatchingElementsExist()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var emptyList = new List<int>();
			var ex = TestUtils.ExpectException<InvalidOperationException>(() => emptyList.Content());
			Assert.AreEqual("Sequence of type '" + IntTypeName + "' contains no elements", ex.Message);
		}

		[TestMethod]
		public void ContentReturnsSingleElement()
		{
			var emptyList = new List<int> {3};
			Assert.AreEqual(3, emptyList.Content());
		}
		
		[TestMethod]
		public void FindDisplaysClearErrorMessageWhenNoElementMatchesCondition()
		{
			var emptyList = new List<int> { 1, 2,3};
			Expression<Func<int, bool>> expr = x => x == 4;
			var ex = TestUtils.ExpectException<InvalidOperationException>(() => emptyList.Find(expr));
			Assert.AreEqual("Sequence of type '" + IntTypeName + "' contains no elements that matches the condition '" + expr + "'", ex.Message);
		}

		[TestMethod]
		public void FindReturnsSingleElementThatMatchesCondition()
		{
			var emptyList = new List<int> { 1, 2, 3 };
			Assert.AreEqual(3, ExtensionMethods.Find(emptyList, x => x > 2));
		}
		
		[TestMethod]
		public void ContentDisplaysClearErrorMessageWhenSourceIsNull()
		{
			List<int> nullList = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => nullList.Content());
			Assert.AreEqual("Sequence of '" + IntTypeName + "' was expected, but was null", ex.Message);
		}

		[TestMethod]
		public void FindDisplaysClearErrorMessageWhenSourceIsNull()
		{
			List<int> nullList = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => ExtensionMethods.Find(nullList, x => true));
			Assert.AreEqual("Sequence of '" + IntTypeName + "' was expected, but was null", ex.Message);
		}

		[TestMethod]
		public void ContentDisplaysClearErrorMessageWhenMoreThanOneElementExist()
		{
			var emptyList = new List<int> {2, 4, 6};
			var ex = TestUtils.ExpectException<InvalidOperationException>(() => emptyList.Content());
			Assert.AreEqual("Sequence of type '" + IntTypeName + "' contains more than one element. The first 2 are '2' and '4'", ex.Message);
		}

		[TestMethod]
		public void FindDisplaysClearErrorMessageWhenMoreThanOneElementMatchesCondition()
		{
			var emptyList = new List<int> { 2, 4, 6 };
			Expression<Func<int, bool>> condition = x => x > 2;
			var ex = TestUtils.ExpectException<InvalidOperationException>(() => emptyList.Find(condition));
			Assert.AreEqual("Sequence of type '" + IntTypeName + "' contains more than element that matches the condition '" + condition + "'. The first 2 are '4' and '6'", ex.Message);
		}

		[TestMethod]
		public void FindDisplaysClearErrorMessageWhenSearchingByAPropertyValue()
		{
			var list = from n in new List<int> {1, 2, 3}
				select new {Value = n, StringValue = n.ToString()};

			Action action = () => list.Find(x => x.StringValue, "4");
			var ex = TestUtils.ExpectException<InvalidOperationException>(action);

			StringAssert.Contains(ex.Message, "contains no elements that matches the condition 'x => x.StringValue == 4");
		}

		[TestMethod]
		public void FindReturnsTheCorrectElementWhenSearchingByPropertyValue()
		{
			var list = from n in new List<int> {1, 2, 3}
				select new {Value = n, StringValue = n.ToString()};

			Assert.AreEqual(2, list.Find(x => x.StringValue, "2").Value, "Find didn't return the expected result");
		}

		[TestMethod]
		public void AppendFormatLinesAppendsTheFormattedStringAndANewLine()
		{
			var sb = new StringBuilder();
			sb.AppendFormatLine("{0} {1}", "Hello", "World");

			Assert.AreEqual("Hello World" + Environment.NewLine, sb.ToString());
		}

		[TestMethod]
		public void DerivesFromTest()
		{
			Assert.IsFalse(typeof(List<int>).DerivesFrom(typeof(IDictionary<int, string>)));
			Assert.IsTrue(typeof(List<int>).DerivesFrom(typeof(IList<int>)));
		}

		[TestMethod]
		public void GetDefaultValueOfValueTypeReturnsItsDefaultValue()
		{
			Assert.AreEqual(default(int), typeof(int).GetDefaultValue());
		}

		[TestMethod]
		public void GetDefaultValueOfReferenceTypeReturnsNull()
		{
			Assert.AreEqual(null, default(string), "Just to make sure :-)");
			Assert.AreEqual(default(string), typeof(string).GetDefaultValue());
		}

		[TestClass]
		[ExcludeFromCodeCoverage] // must be on a class level in order to apply to the lamdba inside the test :-(
		public class AnonymousClass
		{
			[TestMethod]
			public void TryGetReturnsNullIfSourceIsNull()
			{
				// ReSharper disable once RedundantAssignment
				var someObject = new {SomeProperty = "Something"};
				someObject = null;
				Assert.IsNull(someObject.TryGet(x => x.SomeProperty), "TryGet should have returned null");
			}
		}

		[TestMethod]
		public void TryGetReturnsThePropertyValueIfNotNull()
		{
			const string expectedValue = "Something";
			var someObject = new {SomeProperty = expectedValue};
			Assert.AreEqual(expectedValue, someObject.TryGet(x => x.SomeProperty), "TryGet should have returned the property value");
		}

		[TestMethod]
		public void SafeEqualsReturnsTrueIfBothSidesAreNull()
		{
			List<int> nullList = null;
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			TestUtils.ExpectException<NullReferenceException>( () => nullList.Equals(null), "Just to make sure, regular Equals throws a NullReferenceException");
			Assert.IsTrue(nullList.SafeEquals(null));
		}

		[TestMethod]
		public void SafeEqualsReturnsFalseIfLeftSideIsNullAndRightSideIsnt()
		{
			string leftSide = null;
			Assert.IsFalse(leftSide.SafeEquals("Not null"));
		}

		[TestMethod]
		public void SafeEqualsReturnsFalseIsLeftSideIsNotNullAndRightSideIs()
		{
			Assert.IsFalse("NotNull".SafeEquals(null));
		}

		[TestMethod]
		public void SafeEqualsReturnsFalseIfStringsAreDifferent()
		{
			Assert.IsFalse("Foo".SafeEquals("Bar"));
		}

		[TestMethod]
		public void SafeEqualsReturnsTrueIfStringsAreEqual()
		{
			Assert.IsTrue("Foo".SafeEquals("Foo"));
		}

		[TestMethod]
		public void AddRangeAddsOneDictionaryToAnother()
		{
			var originalDictionary = new Dictionary<int, string>
			{
				{1, "One"},
				{2, "Two"}
			};

			var additionalElements = new Dictionary<int, string>
			{
				{3, "Three"},
				{4, "Four"}
			};

			var expectedResult = new Dictionary<int, string>
			{
				{1, "One"},
				{2, "Two"},
				{3, "Three"},
				{4, "Four"}
			};

			originalDictionary.AddRange(additionalElements);
			CollectionAssert.AreEquivalent(expectedResult, originalDictionary);
		}

		[TestMethod]
		public void AddRangeThrowsAnExceptionIfKeyAlreadyExistsInOriginal()
		{
			var originalDictionary = new Dictionary<int, string>
			{
				{1, "One"},
				{2, "Two"}
			};

			var additionalEntries = new Dictionary<int, string>
			{
				{2, "Two"},
				{3, "Three"}
			};

			var ex = TestUtils.ExpectException<ArgumentException>(() => originalDictionary.AddRange(additionalEntries));
			Assert.AreEqual("An item with the same key has already been added.", ex.Message, "Exception message");
		}
	}
}
