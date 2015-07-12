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
	public static class ExtensionMethods
	{
		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}
		
		public static T[] SubArray<T>(this T[] arr, int startIndex, int length)
		{
			var destArray = new T[length];
			Array.Copy(arr, startIndex, destArray, 0, length);
			return destArray;
		}

		public static T[] SubArray<T>(this T[] arr, int startIndex)
			{
			return arr.SubArray(startIndex, arr.Length - startIndex);
		}

	    public static int MinutesAsMilliseconds(this int minutes)
	    {
		    return (minutes*60).SecondsAsMilliseconds();
	    }

	    public static int SecondsAsMilliseconds(this int seconds)
	    {
		    return seconds*1000;
	    }

		public static TimeSpan Milliseconds(this int milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}

		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}

		public static TimeSpan Hours(this int hours)
		{
			return TimeSpan.FromHours(hours);
		}

		public static TimeSpan MutliplyBy(this TimeSpan multiplicand, double multiplier)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
		}

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

		public static string GetCurrentMainWindowTitle(this Process process)
		{
			process.Refresh();
			return process.MainWindowTitle;
		}

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

		public static T1 Find<T1, T2>(this IEnumerable<T1> source, Expression<Func<T1, T2>> propertyAccessor, T2 expectedValue) 
			where T2 : class
		{
			var expressionText = string.Format("{0} == {1}", propertyAccessor, expectedValue);
			return FindInternal(source, x => propertyAccessor.Compile()(x).SafeEquals(expectedValue), expressionText);
		}

		public static T Find<T>(this IEnumerable<T> source, Expression<Func<T, bool>> condition)
		{
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
				message = String.Format("Sequence of type '{0}' contains no elements that matches the condition '{1}'",
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

		[Obsolete("Use Content instead to get better better error messages")]
		[ExcludeFromCodeCoverage]
		public static T Single<T>(this IEnumerable<T> source)
		{
			throw new NotImplementedException("This method is intentially not implemented! You should use Content instead.");
		}

		[Obsolete("Use Find instead to get better better error messages")]
		[ExcludeFromCodeCoverage]
		public static T Single<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			throw new NotImplementedException("This method is intentially not implemented! You should use Find instead.");
		}

		public static void AppendFormatLine(this StringBuilder sb, string formatString, params object[] args)
		{
			sb.AppendFormat(formatString, args);
			sb.AppendLine();
		}

		public static bool DerivesFrom(this Type t1, Type t2)
		{
			return t2.IsAssignableFrom(t1);
		}

		public static object GetDefaultValue(this Type t)
		{
			return t.IsValueType ? Activator.CreateInstance(t) : null;
		}

		public static T2 TryGet<T1, T2>(this T1 obj, Func<T1, T2> func)
			where T2 : class
		{
			return obj == null ? null : func(obj);
		}

		public static bool SafeEquals<T>(this T obj1, T obj2)
			where T : class
		{
			return obj1 == null && obj2 == null
				   || obj1 != null && obj1.Equals(obj2);
		}

		public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> originalDictionary, IDictionary<TKey, TValue> additionalElements)
		{
			foreach (var pair in additionalElements)
			{
				originalDictionary.Add(pair.Key, pair.Value);
			}
		}
	}
}
