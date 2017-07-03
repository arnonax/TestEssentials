using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Writes log entries, using indentations to enhance its readability
	/// </summary>
	public static class Logger
	{
		/// <summary>
		/// Provides default implementations for WriteLine that can be used with <see cref="Logger.Initialize"/>
		/// </summary>
		public static class DefaultImplementations
		{
			/// <summary>
			/// Provides the implementation that writes the line to the Console's output
			/// </summary>
			public static readonly Action<string> Console = System.Console.WriteLine;

			/// <summary>
			/// Provides the implementation that writes the line to the debugger's trace output
			/// </summary>
			public static readonly Action<string> Trace = WriteTrace;

			[ExcludeFromCodeCoverage]
			private static void WriteTrace(string str)
			{
				System.Diagnostics.Trace.WriteLine(str);
			}
		}

		private static int _indentation;
		private static Action<string> _writeLineImpl = DefaultImplementations.Console;

		/// <summary>
		/// Initializes the logger to use the specified implementation for writing a line to the log
		/// </summary>
		/// <param name="writeLineImpl">A delegate that writes a line to the log. You can use any of the members of 
		/// <seealso cref="DefaultImplementations"/> or provide your own
		/// </param>
		/// <exception cref="ArgumentNullException"><paramref name="writeLineImpl"/> is null</exception>
		/// <remarks>
		/// You should only call this method once in your assembly initialization code (i.e. [AssemblyInitialize] method if you're using MSTest)
		/// </remarks>
		public static void Initialize(Action<string> writeLineImpl)
		{
			if (writeLineImpl == null)
				throw new ArgumentNullException("writeLineImpl");

			_indentation = 0;
			_writeLineImpl = writeLineImpl;
		}

		/// <summary>
		/// Writes a line to the log, in the current indentation level, and the current time
		/// </summary>
		/// <param name="format">The format of the line</param>
		/// <param name="args">The format arguments to embbed in the line</param>
		public static void WriteLine(string format, params object[] args)
		{
			var indent = _indentation + 1;

			var sb = new StringBuilder();

            sb.Append(DateTime.Now.ToString("HH:mm:ss.fff"));
			sb.Append('\t', indent);

            if (args.Length == 0)
		        sb.Append(format);
		    else
			    sb.AppendFormat(format, args);

			_writeLineImpl(sb.ToString());
		}

		/// <summary>
		/// Writes the string representation (.ToString()) of the object to the log
		/// </summary>
		/// <param name="obj">The object whose string representation should be written to the log</param>
		public static void WriteLine(object obj)
		{
			WriteLine(obj.ToString());
		}

		/// <summary>
		/// Increases the indentation for the upcoming writes
		/// </summary>
		public static void IncreaseIndent()
		{
			_indentation++;
		}

		/// <summary>
		/// Decreases the indentation for the upcoming writes
		/// </summary>
		public static void DecreaseIndent()
		{
			if (_indentation == 0)
				throw new InvalidOperationException("Indentation is already at its minimum. Can't decrease any further.");
			_indentation--;
		}

		/// <summary>
		/// Writes the specified formatted line to the log, and increases the indentation for the upcoming writes, until the returned object is disposed
		/// </summary>
		/// <param name="format">The format of the line</param>
		/// <param name="args">The format arguments to embbed in the line</param>
		/// <returns>An IDisposable object, which when disposed returns the indentation back to what it was before <seealso cref="StartSection"/> was called</returns>
		/// <remarks>
		/// This method is mainly useful inside a <b>using</b> statement, as in the following example:
		/// </remarks>
		/// <example>
		/// <code>
		/// Logger.WriteLine("This is an ordinary line");
		/// using(Logger.StartSection("This is the section header"))
		/// {
		///		Logger.WriteLine("This is an indented line");
		///		using(Logger.StartSection("This is a section header for a deeper section"))
		///		{
		///			Logger.Writeline("This line is indented even deeper");
		///		}
		///		Logger.WriteLine("This is still part of the outer section");
		/// }
		/// Logger.WriteLine("This is an ordinary line after the section has completed");
		/// </code>
		/// <para>The result is:</para>
		/// <code>
		/// This is an ordinary line
		/// This is the section header
		///		This is an indented line
		///		This is a section header for a deeper section
		/// 			This line is indented even deeper
		///		This is still part of the outer section
		///	This is an ordinary line after the section has completed
		/// </code>
		/// </example>
		public static IDisposable StartSection(string format, params object[] args)
		{
			WriteLine(format, args);
			IncreaseIndent();
			return new AutoDecreaseIndent();
		}

		private class AutoDecreaseIndent : IDisposable
		{
			public void Dispose()
			{
				DecreaseIndent();
			}
		}
	}
}