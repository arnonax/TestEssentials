using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	[ExcludeFromCodeCoverage] // needed in class level because lambda expressions are class-level methods. (used in the call
							  // to ExpectException inside InitializeValidateItsArgument test.
	public class LoggerTests
	{
	    private readonly List<string> _output = new List<string>();

		[TestInitialize]
		public void TestInitialize()
		{
			Logger.Initialize(line => _output.Add(line));
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
		public void InitializeValidateItsArgument()
		{
		    Action<string> nullAction = null;
            var ex = Assert.ThrowsException<ArgumentNullException>(() => Logger.Initialize(nullAction));
			Assert.AreEqual("writeLineImpl", ex.ParamName);

		    ICustomLogger nullCustomLogger = null;
		    ex = Assert.ThrowsException<ArgumentNullException>(() => Logger.Initialize(nullCustomLogger));
            Assert.AreEqual("customLogger", ex.ParamName);
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

#pragma warning disable 618 // Logger.IncreaseIndent is obsolete
        [TestMethod]
		public void IncreaseIndentIncreasesTheIndentationOfTheText()
		{
			Logger.WriteLine("first line");
			Logger.IncreaseIndent();
			Logger.WriteLine("second line");
			Logger.IncreaseIndent();
			Logger.WriteLine("third line");

		    Assert.AreEqual("\tfirst line", GetLineContentWithIndents(0));
			Assert.AreEqual("\t\tsecond line", GetLineContentWithIndents(1));
			Assert.AreEqual("\t\t\tthird line", GetLineContentWithIndents(2));
		}
#pragma warning restore 618

#pragma warning disable 618 // Logger.IncreaseIndent and Logger.DecreaseIndent are obsolete
        [TestMethod]
		public void DecreaseIndentDecreasesTheIndentationOfTheText()
		{
			Logger.WriteLine("first line");
			Logger.IncreaseIndent();
			Logger.WriteLine("second line");
			Logger.DecreaseIndent();
			Logger.WriteLine("third line");

			Assert.AreEqual("\tfirst line", GetLineContentWithIndents(0));
			Assert.AreEqual("\t\tsecond line", GetLineContentWithIndents(1));
			Assert.AreEqual("\tthird line", GetLineContentWithIndents(2));
		}
#pragma warning restore 618

#pragma warning disable 618 // Logger.IncreaseIndent and Logger.DecreaseIndent are obsolete
        [TestMethod]
		public void DecreaseIndentThrowsExpceptionIfIndentIsZero()
		{
			Logger.IncreaseIndent();
			Logger.DecreaseIndent();
			Assert.ThrowsException<InvalidOperationException>((Action)Logger.DecreaseIndent);
		}
#pragma warning restore 618

        [TestMethod]
		public void StartSectionAutomaticallyIncreasesTheIndentationForTheSection()
		{
			using (Logger.StartSection("start of section"))
			{
				Logger.WriteLine("indented line");
			}
			Logger.WriteLine("last line");

			Assert.AreEqual("\tstart of section", GetLineContentWithIndents(0));
			Assert.AreEqual("\t\tindented line", GetLineContentWithIndents(1));
			Assert.AreEqual("\tlast line", GetLineContentWithIndents(2));
		}

	    private class MyCustomLogger : ICustomLogger
	    {
	        public DateTime Timestamp;
	        public string Text;
	        public string LastMethodCalled { get; private set; }

	        public void WriteLine(DateTime  timestamp, string text)
	        {
	            Timestamp = timestamp;
	            Text = text;

	            LastMethodCalled = nameof(WriteLine);
	        }

	        public void StartSection(DateTime timestamp, string message)
	        {
	            LastMethodCalled = nameof(StartSection);
	        }

	        public void EndSection(DateTime timestamp)
	        {
	            LastMethodCalled = nameof(EndSection);
	        }
	    }

	    [TestMethod]
	    public void CustomLoggerGetsTimestampAndMessage()
	    {
            var customLogger = new MyCustomLogger();
            Logger.Initialize(customLogger);

	        var timeBeforeWrite = DateTime.Now;
            Logger.WriteLine("Hello");
	        var timeAfterWrite = DateTime.Now;
            Assert.IsTrue(timeBeforeWrite <= customLogger.Timestamp && customLogger.Timestamp <= timeAfterWrite, $"timestamp={customLogger.Timestamp} should be between {timeBeforeWrite} and {timeAfterWrite}");
            Assert.AreEqual("Hello", customLogger.Text);

            Thread.Sleep(100);
            timeBeforeWrite = DateTime.Now;
            Logger.WriteLine("World");
            timeAfterWrite = DateTime.Now;
	        Assert.IsTrue(timeBeforeWrite <= customLogger.Timestamp && customLogger.Timestamp <= timeAfterWrite);
	        Assert.AreEqual("World", customLogger.Text);
        }

	    [TestMethod]
	    public void CustomLoggerIsNotifiedForStartSectionAndEndSection()
	    {
	        var customLogger = new MyCustomLogger();
	        Logger.Initialize(customLogger);

	        using (Logger.StartSection("Section1"))
	        {
	            Assert.AreEqual(nameof(customLogger.StartSection), customLogger.LastMethodCalled);

                Logger.WriteLine("Something");
                Assert.AreEqual(nameof(customLogger.WriteLine), customLogger.LastMethodCalled);

	            using (Logger.StartSection("Nested section"))
	            {
	                Assert.AreEqual(nameof(customLogger.StartSection), customLogger.LastMethodCalled);
	            }
                Assert.AreEqual(nameof(customLogger.EndSection), customLogger.LastMethodCalled);
	        }
	        Assert.AreEqual(nameof(customLogger.EndSection), customLogger.LastMethodCalled);
        }

        [TestMethod]
	    public void UsesFormatSpecifiers()
	    {
	        Logger.WriteLine("{0}, {1}!", "Hello", "World");
            Assert.AreEqual("Hello, World!", GetLineContent(0));
	    }

	    [TestMethod]
	    public void DoesNotUseTreatFormatSpecifiersIfNoArgumentsProvided()
	    {
	        Logger.WriteLine("{0} {1}");
            Assert.AreEqual("{0} {1}", GetLineContent(0));
	    }

	    [TestMethod]
	    public void HandleFormatExceptionGracefully()
	    {
	        Logger.WriteLine("{0}, {1}", "Hello");
	        var lineContent = GetLineContent(0);
            StringAssert.Contains(lineContent, "{0}, {1}");
            StringAssert.Contains(lineContent, "Hello");

	        Console.WriteLine(string.Join(Environment.NewLine, _output));
	    }

	    private string GetLineContentWithIndents(int lineNumber)
	    {
	        var lengthOfDateTime = 12;
	        return _output[lineNumber].Substring(lengthOfDateTime);
	    }

	    private string GetLineContent(int lineNumber)
	    {
	        return GetLineContentWithIndents(lineNumber).TrimStart('\t');
	    }
	}
}
