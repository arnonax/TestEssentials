using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[ExcludeFromCodeCoverage]
	[TestClass]
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
			var timeBefore = DateTime.Now;
			const string text = "TEST";
			Logger.WriteLine(text);
			var timeAfter = DateTime.Now;

			var splittedString = _output.Content().Split(new[] {'\t'}, 2);
			var loggedTimeAsString = splittedString[0];
			var loggedText = splittedString[1];
			var loggedTime = DateTime.Parse(loggedTimeAsString);

			Assert.IsTrue(timeBefore <= loggedTime && loggedTime <= timeAfter, "Logged time '{0}' should be between '{1}' and '{2}'", loggedTime, timeBefore, timeAfter);
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

			const int lengthOfDateTime = 12;
			Assert.AreEqual("\tfirst line",_output[0].Substring(lengthOfDateTime));
			Assert.AreEqual("\t\tsecond line", _output[1].Substring(lengthOfDateTime));
			Assert.AreEqual("\t\t\tthird line", _output[2].Substring(lengthOfDateTime));
		}

		[TestMethod]
		public void DecreaseIndentDecreasesTheIndentationOfTheText()
		{
			Logger.WriteLine("first line");
			Logger.IncreaseIndent();
			Logger.WriteLine("second line");
			Logger.DecreaseIndent();
			Logger.WriteLine("third line");

			Assert.AreEqual("\tfirst line", _output[0].Substring(LengthOfDateTime));
			Assert.AreEqual("\t\tsecond line", _output[1].Substring(LengthOfDateTime));
			Assert.AreEqual("\tthird line", _output[2].Substring(LengthOfDateTime));
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

			Assert.AreEqual("\tstart of section", _output[0].Substring(LengthOfDateTime));
			Assert.AreEqual("\t\tindented line", _output[1].Substring(LengthOfDateTime));
			Assert.AreEqual("\tlast line", _output[2].Substring(LengthOfDateTime));
		}
	}
}
