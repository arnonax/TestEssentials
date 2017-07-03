using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage] // needed in class level because lambda expressions are class-level methods. (used in the call
							  // to ExpectException inside InitializeValidateItsArgument test.
	public class LoggerTests
	{
		private const int LengthOfDateTime = 12;
		private readonly List<string> _output = new List<string>();

		[TestInitialize]
		public void TestInitialize()
		{
			Logger.Initialize(line => _output.Add(line));
		}

		[TestMethod]
		public void InitializeValidateItsArgument()
		{
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => Logger.Initialize(null));
			Assert.AreEqual("writeLineImpl", ex.ParamName);
		}

		[TestMethod]
		public void WriteLineWritesTheCurrentTimeAtTheBeginningOfTheLine()
		{
			var timeBefore = DateTime.Now.TrimMilliseconds();
			const string text = "TEST";
			Logger.WriteLine(text);
			var timeAfter = DateTime.Now.TrimMilliseconds();

			var splittedString = _output.Content().Split(new[] {'\t'}, 2);
			var loggedTimeAsString = splittedString[0];
			var loggedText = splittedString[1];
			var loggedTime = DateTime.Parse(loggedTimeAsString);

			Assert.IsTrue(timeBefore <= loggedTime && loggedTime <= timeAfter, "Logged time '{0:O}' should be between '{1:O}' and '{2:O}'", loggedTime, timeBefore, timeAfter);
			Assert.AreEqual(text, loggedText, "text");
		}

		[TestMethod]
		public void IncreaseIndentIncreasesTheIndentationOfTheText()
		{
			Logger.WriteLine("first line");
			Logger.IncreaseIndent();
			Logger.WriteLine("second line");
			Logger.IncreaseIndent();
			Logger.WriteLine("third line");

		    Assert.AreEqual("\tfirst line", GetLineContent(0));
			Assert.AreEqual("\t\tsecond line", GetLineContent(1));
			Assert.AreEqual("\t\t\tthird line", GetLineContent(2));
		}

		[TestMethod]
		public void DecreaseIndentDecreasesTheIndentationOfTheText()
		{
			Logger.WriteLine("first line");
			Logger.IncreaseIndent();
			Logger.WriteLine("second line");
			Logger.DecreaseIndent();
			Logger.WriteLine("third line");

			Assert.AreEqual("\tfirst line", GetLineContent(0));
			Assert.AreEqual("\t\tsecond line", GetLineContent(1));
			Assert.AreEqual("\tthird line", GetLineContent(2));
		}

		[TestMethod]
		public void DecreaseIndentThrowsExpceptionIfIndentIsZero()
		{
			Logger.IncreaseIndent();
			Logger.DecreaseIndent();
			TestUtils.ExpectException<InvalidOperationException>(Logger.DecreaseIndent);
		}

		[TestMethod]
		public void StartSectionAutomaticallyIncreasesTheIndentationForTheSection()
		{
			using (Logger.StartSection("start of section"))
			{
				Logger.WriteLine("indented line");
			}
			Logger.WriteLine("last line");

			Assert.AreEqual("\tstart of section", GetLineContent(0));
			Assert.AreEqual("\t\tindented line", GetLineContent(1));
			Assert.AreEqual("\tlast line", GetLineContent(2));
		}

	    private string GetLineContent(int lineIndex)
	    {
	        return _output[lineIndex].Substring(LengthOfDateTime);
	    }

	    [TestMethod]
	    public void WriteLineWritesCurlyBracesIfNoArgsProvided()
	    {
	        const string stringWithCurlyBraces = "{{}{}{{}{";
	        Logger.WriteLine(stringWithCurlyBraces);
            Assert.AreEqual(stringWithCurlyBraces, GetLineContent(0).TrimStart('\t'));
	    }
	}
}
