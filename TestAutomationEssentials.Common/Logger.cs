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
		/// Provides default implementations for WriteLine that can be used with <see cref="Logger.Initialize(System.Action{string})"/>
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

		private static ICustomLogger _customLogger = new DefaultLogger(DefaultImplementations.Console);

	    private class DefaultLogger : ICustomLogger
	    {
	        private readonly Action<string> _writeLineImpl;
	        private int _indentation;

	        public DefaultLogger(Action<string> writeLineImpl)
	        {
	            _writeLineImpl = writeLineImpl;
	            _indentation = 0;
	        }

	        public void WriteLine(DateTime timestamp, string message)
	        {
	            var indent = _indentation + 1;

	            var sb = new StringBuilder();

	            sb.Append(timestamp.ToString("HH:mm:ss.fff"));
	            sb.Append('\t', indent);
	            sb.Append(message);
	            _writeLineImpl(sb.ToString());
	        }

	        public void StartSection(DateTime timestamp, string message)
	        {
                WriteLine(timestamp, message);
	            _indentation++;
	        }

	        public void EndSection(DateTime timestamp)
	        {
                DecreaseIndentImpl();
            }

            public void IncreaseIndentImpl()
	        {
	            _indentation++;
	        }

	        public void DecreaseIndentImpl()
	        {
	            if (_indentation == 0)
	                throw new InvalidOperationException("Indentation is already at its minimum. Can't decrease any further.");
	            _indentation--;
            }
        }

	    public static string LastMethod { get; private set; }

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
				throw new ArgumentNullException(nameof(writeLineImpl));

		    _customLogger = new DefaultLogger(writeLineImpl);
		}

	    /// <summary>
	    /// Initializes the logger to use a custom logger that performs the writes to the log
	    /// </summary>
	    /// <param name="customLogger">An object that implements <see cref="ICustomLogger"/>. Provide your own implementation for this interface to
	    /// handle where and how the log entries are written
	    /// </param>
	    /// <exception cref="ArgumentNullException"><paramref name="customLogger"/> is null</exception>
	    /// <remarks>
	    /// You should only call this method once in your assembly initialization code (i.e. [AssemblyInitialize] method if you're using MSTest)
	    /// </remarks>
	    public static void Initialize(ICustomLogger customLogger)
	    {
            if (customLogger == null)
                throw new ArgumentNullException(nameof(customLogger));

	        _customLogger = customLogger;
	    }

	    /// <summary>
		/// Writes a line to the log, in the current indentation level, and the current time
		/// </summary>
		/// <param name="format">The format of the line</param>
		/// <param name="args">The format arguments to embbed in the line</param>
		public static void WriteLine(string format, params object[] args)
		{
		    _customLogger.WriteLine(DateTime.Now, SafeFormatMessage(format, args));
		}

	    private static string SafeFormatMessage(string format, object[] args)
	    {
            var sb = new StringBuilder();

	        if (args.Length == 0)
	        {
	            sb.Append(format); // ignore format specifiers if no arguments are specified. Similiar to how Console.WriteLine behaves
	            return format;
	        }

	        try
	        {
	            var formattedMessage = string.Format(format, args);
	            sb.Append(formattedMessage);
	        }
	        catch (FormatException)
	        {
	            sb.AppendLine("WARNING: Failed to format line! See details below:");
	            sb.Append("Format string:");
	            sb.AppendLine(format);
	            if (args.Length > 0)
	            {
	                sb.AppendLine("Format arguments:");
	                for (int i = 0; i < args.Length; i++)
	                {
	                    sb.AppendFormat("{{{0}}}: {1}", i, args[i]);
	                }
	            }
	        }

	        return sb.ToString();
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
        [Obsolete("Use StartSection instead")]
        public static void IncreaseIndent()
		{
		    var defaultLogger = _customLogger as DefaultLogger;
		    defaultLogger?.IncreaseIndentImpl();
		}

		/// <summary>
		/// Decreases the indentation for the upcoming writes
		/// </summary>
		[Obsolete("Call Dispose on the result of StartSection in order to decrease the indentation")]
		public static void DecreaseIndent()
		{
		    var defaultLogger = _customLogger as DefaultLogger;
		    defaultLogger?.DecreaseIndentImpl();
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
		    _customLogger.StartSection(DateTime.Now, SafeFormatMessage(format, args));
			return new AutoDecreaseIndent();
		}

		private class AutoDecreaseIndent : IDisposable
		{
			public void Dispose()
			{
				_customLogger.EndSection(DateTime.Now);
			}
		}
	}

    /// <summary>
    /// Defines methods to be implemented by customer loggers
    /// </summary>
    public interface ICustomLogger
    {
        /// <summary>
        /// Writes a single message to the log
        /// </summary>
        /// <param name="timestamp">The date and time when the message was written</param>
        /// <param name="message">The text of the message</param>
        void WriteLine(DateTime timestamp, string message);

        /// <summary>
        /// Writes a message that begins a new section. All subsequent message are related to this
        /// new section, until <see cref="EndSection"/> is called
        /// </summary>
        /// <param name="timestamp">The date and time when the message was written</param>
        /// <param name="message">The text of the message for the section header</param>
        /// <remarks>
        /// This method can be called multiple times before <see cref="EndSection"/> is called.
        /// This should create a nested sections. Each section should be closed with the matching
        /// <see cref="EndSection"/>.
        /// </remarks>
        void StartSection(DateTime timestamp, string message);

        /// <summary>
        /// Ends the most recently active section.
        /// </summary>
        /// <param name="timestamp">The date and time when the section was ended</param>
        void EndSection(DateTime timestamp);
    }
}