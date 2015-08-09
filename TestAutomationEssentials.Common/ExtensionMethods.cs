using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TestAutomationEssentials.Common
{
	// TODO: split this class to seperate classes according to subjects
	/// <summary>
	/// Provides various general-purpose useful extension methods
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Returns the result of func(obj) if obj is not null, or null otherwise. Similiar to the <a href="https://msdn.microsoft.com/en-us/library/ty67wk28.aspx">Elvis Operator</a>
		/// available in C# 6.
		/// </summary>
		/// <typeparam name="T1">The type of object</typeparam>
		/// <typeparam name="T2">The type of the result of func</typeparam>
		/// <param name="obj">The object to invoke the function on. Can be null</param>
		/// <param name="func">The function to invoke on <paramref name="obj"/> in case it's not null</param>
		/// <returns>The result of func(obj) if obj is ot null, or null otherwise</returns>
		/// <exception cref="ArgumentNullException"><paramref name="func"/> is null</exception>
		/// <remarks>
		/// This method is extremely useful when it is used when it is used few times in a row, to get a specific value from a complex data structure that any of its part may be null.
		/// One common such scenario is when using objects that are generated through XSD.exe or a similiar tool.
		/// </remarks>
		/// <example>
		/// var order = GetOrder(); // may return null
		/// var street = order.(x => x.Customer).TryGet(x => x.Address).TryGet(x => x.Address);
		/// </example>
		public static T2 TryGet<T1, T2>(this T1 obj, Func<T1, T2> func)
			where T2 : class
		{
			if (func == null)
				throw new ArgumentNullException("func");

			return obj == null ? null : func(obj);
		}

		/// <summary>
		/// Compares 2 objects, even if they're null.
		/// </summary>
		/// <typeparam name="T">The type of object</typeparam>
		/// <param name="obj1">First object to compare</param>
		/// <param name="obj2">2nd object to compare</param>
		/// <returns><b>true</b> if the values of <paramref name="obj1"/> and <see cref="obj2"/> are the same, even if it's <b>null</b>. Otherwise <b>false</b></returns>
		public static bool SafeEquals<T>(this T obj1, T obj2)
		{
			return obj1 == null && obj2 == null
				   || obj1 != null && obj1.Equals(obj2);
		}

		/// <summary>
		/// Determines whether the specified sequence is empty
		/// </summary>
		/// <typeparam name="T">The type of the elements in the sequence</typeparam>
		/// <param name="source">The sequence</param>
		/// <returns><b>true</b> if the sequence is empty, otherwise <b>false</b></returns>
		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}
		
		/// <summary>
		/// Copies a portion of the array into a new array, given the start index and length
		/// </summary>
		/// <typeparam name="T">The type of the elements in the array</typeparam>
		/// <param name="arr">The original array</param>
		/// <param name="startIndex">The index of the first element to copy</param>
		/// <param name="length">The number of elements to copy</param>
		/// <returns>A new array containing only the specified elements</returns>
		public static T[] SubArray<T>(this T[] arr, int startIndex, int length)
		{
			var destArray = new T[length];
			Array.Copy(arr, startIndex, destArray, 0, length);
			return destArray;
		}

		/// <summary>
		/// Copies a portion of the array into a new array, starting at the specified index up to the end of the array
		/// </summary>
		/// <typeparam name="T">The type of the elements in the array</typeparam>
		/// <param name="arr">The original array</param>
		/// <param name="startIndex">The index of the first element to copy</param>
		/// <returns>A new array containing the last portion, starting at the specified index</returns>
		public static T[] SubArray<T>(this T[] arr, int startIndex)
		{
			return arr.SubArray(startIndex, arr.Length - startIndex);
		}

		/// <summary>
		/// Calculates the number of milliseconds in the specified number of minutes
		/// </summary>
		/// <param name="minutes">The number of minutes to convert to milliseconds</param>
		/// <returns>The result of the calculation</returns>
		public static int MinutesAsMilliseconds(this int minutes)
	    {
		    return (minutes*60).SecondsAsMilliseconds();
	    }

		/// <summary>
		/// Calculates the number of milliseconds in the specified number of seconds
		/// </summary>
		/// <param name="seconds">The number of seconds to convert to milliseconds</param>
		/// <returns>The result of the calculation</returns>
	    public static int SecondsAsMilliseconds(this int seconds)
	    {
		    return seconds*1000;
	    }

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of milliseconds
		/// </summary>
		/// <param name="milliseconds">The number of milliseconds</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of milliseconds</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Millisconds();
		/// </example>
		public static TimeSpan Milliseconds(this int milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of seconds
		/// </summary>
		/// <param name="seconds">The number of seconds</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of seconds</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Seconds();
		/// </example>
		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of minutes
		/// </summary>
		/// <param name="minutes">The number of minutes</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of minutes</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Minutes();
		/// </example>
		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Returns a <b>TimeSpan</b> that represents the given number of hours
		/// </summary>
		/// <param name="hours">The number of hours</param>
		/// <returns>The <b>TimeSpan</b> struct that represent the given number of hours</returns>
		/// <example>
		/// TimeSpan timeSpan = 3.Hours();
		/// </example>
		public static TimeSpan Hours(this int hours)
		{
			return TimeSpan.FromHours(hours);
		}

		/// <summary>
		/// Multiplies the specified TimeSpan by the specified multiplier
		/// </summary>
		/// <param name="multiplicand">The <b>TimeSpan</b> struct to multiply</param>
		/// <param name="multiplier">The multiplier</param>
		/// <returns>A <b>TimeSpan</b> representing the calculated multiplication</returns>
		/// <example>
		/// Assert.AreEquals(6.Minutes(), 2.Minutes().MultiplyBy(3));
		/// </example>
		public static TimeSpan MutliplyBy(this TimeSpan multiplicand, double multiplier)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
		}

		/// <summary>
		/// Returns a string that represent the most significant portions of the specified <b>TimeSpan</b>
		/// </summary>
		/// <param name="timeSpan">The <b>TimeSpan</b> to convert to string</param>
		/// <returns>The string that contains the significant portions of the TimeSpan</returns>
		public static string ToSpokenString(this TimeSpan timeSpan)
		{
			if (timeSpan == 1.Minutes())
				return "1 minute";

			if (timeSpan == 1.Hours())
				return "1 hour";

			if (timeSpan > 1.Hours())
				return Format("hours", timeSpan.Hours, timeSpan.Minutes);

			if (timeSpan > 1.Minutes())
				return Format("minutes", timeSpan.Minutes, timeSpan.Seconds);

			var seconds = timeSpan.TotalSeconds;
			
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return seconds == 1.0 ? "1 second" : seconds + " seconds";
		}

		private static string Format(string uom, int mainValue, int subValue)
		{
			if (subValue == 0)
				return mainValue + " " + uom;

			return string.Format("{0}:{1:00} " + uom, mainValue, subValue);
		}

		/// <summary>
		/// Returns the title of the main window of the specified process as it is now
		/// </summary>
		/// <param name="process">The process to look for its main window title</param>
		/// <returns>The title of the main window</returns>
		/// <remarks>
		/// Unlike <see cref="Process.MainWindowTitle"/>, this method always return the up-to-date title of the main window, even if it has changed after the <see cref="Process"/> has been started
		/// </remarks>
		public static string GetCurrentMainWindowTitle(this Process process)
		{
			process.Refresh();
			return process.MainWindowTitle;
		}

		/// <summary>
		/// Returns the single element contained in the specified sequence
		/// </summary>
		/// <typeparam name="T">The type of the element inside the sequence</typeparam>
		/// <param name="source">The sequence containing the single element</param>
		/// <returns>The element contained in the sequence</returns>
		/// <exception cref="ArgumentNullException">source is null</exception>
		/// <exception cref="InvalidOperationException">The sequence is empty or it contains more than one element</exception>
		/// <remarks>
		/// This method is similiar to <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
		/// but provide more detailed message if it fails.
		/// <br/>
		/// Because it's not possible to "override" a static method, and in order to prevent the inadvertent use of the original 
		/// <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> method, this class hides the original
		/// method by declaring another method with the same name, and an [Obsolete] attribute.
		/// </remarks>
		public static T Content<T>(this IEnumerable<T> source)
		{
			string message;
			if (source == null)
			{
				message = string.Format("Sequence of '{0}' was expected, but was null", typeof (T).Name);
				throw new ArgumentNullException(null, message);
			}

			var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				message = String.Format("Sequence of type '{0}' contains no elements", typeof(T).Name);
				throw new InvalidOperationException(message);
			}

			var first = enumerator.Current;

			if (!enumerator.MoveNext())
			{
				return first;
			}

			var second = enumerator.Current;
			message = string.Format("Sequence of type '{0}' contains more than one element. The first 2 are '{1}' and '{2}'",
				typeof(T).Name, first, second);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Returns the single element in the specified sequence whose specified property has the specified value
		/// </summary>
		/// <typeparam name="T1">The type of the elements in the sequence</typeparam>
		/// <typeparam name="T2">The type of the property</typeparam>
		/// <param name="source">The sequence containing the element to look for</param>
		/// <param name="propertyAccessor">A lambda expression that returns the property to compare with <paramref name="expectedValue"/></param>
		/// <param name="expectedValue">The value of the property that the element should have</param>
		/// <returns>The element that its property matches the expected value</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null or <paramref name="propertyAccessor"/> is null</exception>
		/// <exception cref="InvalidOperationException">The sequence contains no matching element or it contains more than one matching element</exception>
		/// <remarks>
		/// This method is similiar to <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource, System.Boolean})"/>
		/// but provide more detailed message if it fails.
		/// </remarks>
		public static T1 Find<T1, T2>(this IEnumerable<T1> source, Expression<Func<T1, T2>> propertyAccessor, T2 expectedValue)
		{
			if (propertyAccessor == null)
				throw new ArgumentNullException("propertyAccessor");

			var expressionText = string.Format("{0} == {1}", propertyAccessor, expectedValue);
			return FindInternal(source, x => propertyAccessor.Compile()(x).SafeEquals(expectedValue), expressionText);
		}

		/// <summary>
		/// Returns the single element in the specified sequence that matches the specified criteria
		/// </summary>
		/// <typeparam name="T">The type of the element in the sequence</typeparam>
		/// <param name="source">The sequence containing the element to look for</param>
		/// <param name="condition">A lambda expression of a condition that the matching element should pass</param>
		/// <returns>The element that matches the specified condition</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null or <paramref name="condition"/> is null</exception>
		/// <exception cref="InvalidOperationException">The sequence contains no matching element or it contains more than one matching element</exception>
		/// <remarks>
		/// This method is similiar to <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource, System.Boolean})"/>
		/// but provide more detailed message if it fails.
		/// <br/>
		/// Because it's not possible to "override" a static method, and in order to prevent the inadvertent use of the original 
		/// <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource, System.Boolean})"/> method, this class hides the original
		/// method by declaring another method with the same name, and an [Obsolete] attribute.
		/// </remarks>
		public static T Find<T>(this IEnumerable<T> source, Expression<Func<T, bool>> condition)
		{
			if (condition == null)
				throw new ArgumentNullException("condition");

			var predicate = condition.Compile();
			var expressionText = condition.ToString();

			return FindInternal(source, predicate, expressionText);
		}

		private static T FindInternal<T>(IEnumerable<T> source, Func<T, bool> predicate, string expressionText)
		{
			string message;
			if (source == null)
			{
				message = string.Format("Sequence of '{0}' was expected, but was null", typeof (T).Name);
				throw new ArgumentNullException(null, message);
			}

			var enumerator = source.Where(predicate).GetEnumerator();
			if (!enumerator.MoveNext())
			{
				message = String.Format("Sequence of type '{0}' contains no element that matches the condition '{1}'",
					typeof (T).Name, expressionText);
				throw new InvalidOperationException(message);
			}

			var first = enumerator.Current;

			if (!enumerator.MoveNext())
			{
				return first;
			}

			var second = enumerator.Current;
			message =
				string.Format(
					"Sequence of type '{0}' contains more than element that matches the condition '{1}'. The first 2 are '{2}' and '{3}'",
					typeof (T).Name, expressionText, first, second);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// This method intentionally hides <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> in order to force you to use
		/// <see cref="Content{T}"/> instead.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the sequence</typeparam>
		/// <param name="source">A sequence</param>
		/// <returns>Nothing</returns>
		/// <exception cref="NotImplementedException">Always</exception>
		/// <remarks>This method intentionally creates a compilation error if used! Use <see cref="Content{T}"/> instead to get better error messages on failures</remarks>
		[Obsolete("Use Content instead to get better better error messages")]
		[ExcludeFromCodeCoverage]
		public static T Single<T>(this IEnumerable<T> source)
		{
			throw new NotImplementedException("This method is intentially not implemented! You should use Content instead.");
		}

		/// <summary>
		/// This method intentionally hides <see cref="Enumerable.Single{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource, bool})"/> in order to force you to use
		/// <see cref="Find{T1,T2}"/> or <see cref="Find{T1}"/> instead.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the sequence</typeparam>
		/// <param name="source">A sequence</param>
		/// <param name="predicate">A condition that the element to look for should match</param>
		/// <returns>Nothing</returns>
		/// <exception cref="NotImplementedException">Always</exception>
		/// <remarks>This method intentionally creates a compilation error if used! Use <see cref="Find{T1,T2}"/> or <see cref="Find{T1}"/> instead to get better error messages on failures</remarks>
		[Obsolete("Use Find instead to get better better error messages")]
		[ExcludeFromCodeCoverage]
		public static T Single<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			throw new NotImplementedException("This method is intentially not implemented! You should use Find instead.");
		}

		/// <summary>
		/// Appends a a formatted line to a StringBuilder
		/// </summary>
		/// <param name="sb">The <see cref="StringBuilder"/> to which to append the formatted line</param>
		/// <param name="format">The format string</param>
		/// <param name="args">Any arguments to embedded in the format string</param>
		/// <remarks>
		/// This method is a simple combination of <see cref="StringBuilder.AppendFormat(string,object)"/> and <see cref="StringBuilder.AppendLine()"/>
		/// </remarks>
		public static void AppendFormatLine(this StringBuilder sb, string format, params object[] args)
		{
			if (sb == null)
				throw new ArgumentNullException("sb");

			sb.AppendFormat(format, args);
			sb.AppendLine();
		}

		/// <summary>
		/// Appends one dictionary to another
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionaries</typeparam>
		/// <typeparam name="TValue">The type of the values in the dictionaries</typeparam>
		/// <param name="originalDictionary">The original dictionary to add the elements to</param>
		/// <param name="additionalElements">The dictionary that contain the additional elements to add to the original dictionary</param>
		/// <exception cref="ArgumentNullException">Either <paramref name="originalDictionary"/> or <paramref name="additionalElements"/> is null</exception>
		/// <exception cref="ArgumentException">One or more keys exist for in originalDictionary and additionalElements</exception>
		public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> originalDictionary, IDictionary<TKey, TValue> additionalElements)
		{
			if (originalDictionary == null)
				throw new ArgumentNullException("originalDictionary");

			if (additionalElements == null)
				throw new ArgumentNullException("additionalElements");

			if (originalDictionary.Keys.Intersect(additionalElements.Keys).Any())
				throw new ArgumentException("One or more keys exist for in originalDictionary and additionalElements", "additionalElements");

			foreach (var pair in additionalElements)
			{
				originalDictionary.Add(pair.Key, pair.Value);
			}
		}
	}
}
