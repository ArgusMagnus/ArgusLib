#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using ArgusLib.Diagnostics.Tracing;
using System.Reflection;

namespace ArgusLib
{
    public static partial class ExtensionMethods
    {
		#region System.Double

		/// <summary>
		/// https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
		/// http://stackoverflow.com/questions/3103782/rule-of-thumb-to-test-the-equality-of-two-doubles-in-c
		/// </summary>
		/// <param name="val"></param>
		/// <param name="other"></param>
		/// <param name="maxAbsoluteError"></param>
		/// <param name="maxRelativeError"></param>
		/// <returns></returns>
		public static bool AlmostEqual(this double val, double other, double maxAbsoluteError = 0, double maxRelativeError = Constants.MaschineEpsilon, int maxUlps = 4)
		{
			if (maxAbsoluteError < 0)
				throw Tracer.ThrowError(new ArgumentOutOfRangeException(nameof(maxAbsoluteError), Exceptions.ArgumentOutOfRange_MustNotBeNegative), typeof(ExtensionMethods));
			if (maxRelativeError < 0)
				throw Tracer.ThrowError(new ArgumentOutOfRangeException(nameof(maxRelativeError), Exceptions.ArgumentOutOfRange_MustNotBeNegative), typeof(ExtensionMethods));

			const int MaxUlpBound = 4 * 1024 * 1024;
			// Make sure maxUlps is non-negative and small enough that the
			// default NAN won't compare as equal to anything.
			if (maxUlps < 0)
				throw Tracer.ThrowError(new ArgumentOutOfRangeException(nameof(maxUlps), Exceptions.ArgumentOutOfRange_MustNotBeNegative), typeof(ExtensionMethods));
			if (maxUlps >= MaxUlpBound)
				throw Tracer.ThrowError(new ArgumentOutOfRangeException(nameof(maxUlps), string.Format(Exceptions.ArgumentOutOfRange_MustBeSmallerThan, MaxUlpBound)), typeof(ExtensionMethods));

			if (val == other)
				return true;

			if (maxAbsoluteError > 0 || maxRelativeError > 0)
			{
				double diff = Math.Abs(val - other);
				if (diff <= maxAbsoluteError)
					return true;

				double max = Math.Max(Math.Abs(val), Math.Abs(other));
				if (diff <= max * maxRelativeError)
					return true;
			}
			if (maxUlps > 0)
			{
				long a = BitConverter.DoubleToInt64Bits(val);
				long b = BitConverter.DoubleToInt64Bits(other);
				// Make a, b lexicographically ordered as a twos-complement int
				if (a < 0)
					a = long.MinValue + (-a);
				if (b < 0)
					b = long.MinValue + (-b);

				long diff = Math.Abs(a - b);
				if (diff <= maxUlps)
					return true;
			}
			return false;
		}

		#endregion

		#region System.String

		public static string Join(this string[] parts, string link = null, bool skipEmpty = false)
		{
			link = link ?? string.Empty;
			int capacity = 0;
			foreach (string part in parts)
				capacity += part.Length + link.Length;
			StringBuilder sb = new StringBuilder(capacity);
			int i;
			for (i = 0; i < parts.Length; i++)
			{
				if (skipEmpty && string.IsNullOrEmpty(parts[i]))
					continue;
				sb.Append(parts[i]);
				break;
			}
			for (i++; i < parts.Length; i++)
			{
				if (skipEmpty && string.IsNullOrEmpty(parts[i]))
					continue;
				sb.Append(link);
				sb.Append(parts[i]);
			}
			return sb.ToString();
		}

		public static string Replace(this string value, string oldValue, string newValue, StringComparison comparison)
		{
			if (comparison == StringComparison.Ordinal)
				return value.Replace(oldValue, newValue);

			int index = value.IndexOf(oldValue, comparison);
			if (index < 0)
				return value;
			return value.Substring(0, index) + newValue + value.Substring(index + oldValue.Length);
		}

		/// <summary>
		/// Removes all the chars in <paramref name="charsToBeRemoved"/> from the string.
		/// If <paramref name="charsToBeRemoved"/> is <c>null</c> or empty, all white-space
		/// characters are removed.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="charsToBeRemoved"></param>
		/// <returns></returns>
		public static string Filter(this string s, params char[] charsToBeRemoved)
		{
			charsToBeRemoved = charsToBeRemoved ?? new char[0];
			StringBuilder sb = new StringBuilder(s.Length);
			if (charsToBeRemoved.Length > 0)
			{
				foreach (char c in s)
				{
					if (!charsToBeRemoved.Contains(c))
						sb.Append(c);
				}
			}
			else
			{
				foreach (char c in s)
				{
					if (!char.IsWhiteSpace(c))
						sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public static int IndexOf(this string s, Predicate<char> predicate)
		{
			int i;
			for(i = 0; i < s.Length; i++)
			{
				if (predicate(s[i]))
					return i;
			}
			return -1;
		}

		public static int LastIndexOf(this string s, Predicate<char> predicate)
		{
			int i;
			for(i = s.Length-1;i> -1;i--)
			{
				if (predicate(s[i]))
					return i;
			}
			return -1;
		}

		public static int IndexOfAny(this string s, IEnumerable<string> anyOf, int startIndex = 0)
		{
			var indices = anyOf.Select((str) => s.IndexOf(str, startIndex)).Where((i) => i > -1);
			if (indices.Count() > 0)
				return indices.Min();
			return -1;
		}

		public static int IndexOfAny(this string s, params string[] anyOf) => s.IndexOfAny(anyOf, 0);

		public static IEnumerable<int> IndicesOf(this string s, string str, int startIndex = 0)
		{
			int i = startIndex;
			while (i < s.Length)
			{
				i = s.IndexOf(str, i);
				if (i < 0)
					yield break;
				yield return i;
				i++;
			}
		}

		public static IEnumerable<int> IndicesOfAny(this string s, IEnumerable<string> anyOf, int startIndex = 0)
		{
			foreach (string str in anyOf)
			{
				foreach (int i in s.IndicesOf(str, startIndex))
					yield return i;
			}
		}

		public static IEnumerable<int> IndicesOfAny(this string s, params string[] anyOf) => s.IndicesOfAny(anyOf, 0);

		#endregion

		#region System.Text.StringBuilder

		public static int IndexOf(this StringBuilder sb, string value, StringComparison comparison = StringComparison.Ordinal)
		{
			if (comparison == StringComparison.CurrentCultureIgnoreCase)
				value = value.ToLower();
			else if (comparison == StringComparison.OrdinalIgnoreCase)
				value = value.ToLowerInvariant();

			for (int i = 0; i < sb.Length; i++)
			{
				bool found = true;
				for (int j = 0; j < value.Length && i + j < sb.Length; j++)
				{
					char ch;
					if (comparison == StringComparison.CurrentCultureIgnoreCase)
						ch = char.ToLower(sb[i + j]);
					else if (comparison == StringComparison.OrdinalIgnoreCase)
						ch = char.ToLowerInvariant(sb[i + j]);
					else
						ch = sb[i + j];
					if (value[j] != ch)
					{
						found = false;
						break;
					}
				}
				if (found)
					return i;
			}
			return -1;
		}

		public static void Replace(this StringBuilder sb, string oldValue, string newValue, StringComparison comparison)
		{
			if (comparison == StringComparison.Ordinal)
			{
				sb.Replace(oldValue, newValue);
				return;
			}

			int index = sb.IndexOf(oldValue, comparison);
			if (index < 0)
				return;
			sb.Remove(index, oldValue.Length);
			sb.Insert(index, newValue);
		}

		public static void TrimStart(this StringBuilder sb, char[] trim = null)
		{
			int i;
			for (i = 0; i < sb.Length; i++)
			{
				if (!(trim?.Contains(sb[i]) ?? char.IsWhiteSpace(sb[i])))
					break;
			}
			if (i > 0)
				sb.Remove(0, i);
		}

		public static void TrimEnd(this StringBuilder sb, char[] trim = null)
		{
			int i;
			for (i = sb.Length-1; i > -1; i--)
			{
				if (!(trim?.Contains(sb[i]) ?? char.IsWhiteSpace(sb[i])))
					break;
			}
			i++;
			if (i < sb.Length - 1)
				sb.Remove(i, sb.Length - i);
		}

		public static void Trim(this StringBuilder sb, char[] trim = null)
		{
			sb.TrimEnd(trim);
			sb.TrimStart(trim);
		}

		#endregion

		public static string ToStringInvariant<T>(this T obj) where T : IFormattable => obj.ToString(null, CultureInfo.InvariantCulture);

		public static string ToISO8601(this DateTime dt) => $"{dt.Year:D4}-{dt.Month:D2}-{dt.Day:D2}T{dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}";

		public static void AddToList<TKey,TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
		{
			List<TValue> list;
			if (!dict.TryGetValue(key, out list))
			{
				list = new List<TValue>();
				dict.Add(key, list);
			}
			list.Add(value);
		}

		public static void RemoveIf<TKey,TValue>(this IDictionary<TKey,TValue> dict, Predicate<KeyValuePair<TKey,TValue>> predicate)
		{
			List<TKey> remove = new List<TKey>(dict.Count);
			foreach (var item in dict)
			{
				if (predicate(item))
					remove.Add(item.Key);
			}
			foreach (var key in remove)
				dict.Remove(key);
		}

		public static bool IsEven(this int val) => (val & 1) == 0;
		public static bool IsEven(this long val) => (val & 1L) == 0L;
		public static bool IsEven(this uint val) => (val & 1U) == 0;
		public static bool IsEven(this ulong val) => (val & 1UL) == 0L;
		public static bool IsPowerOfTwo(this int val) => (val > 0) && ((val & (val - 1)) == 0);
		public static bool IsPowerOfTwo(this long val) => (val > 0L) && ((val & (val - 1L)) == 0L);
		public static bool IsPowerOfTwo(this uint val) => (val > 0U) && ((val & (val - 1U)) == 0U);
		public static bool IsPowerOfTwo(this ulong val) => (val > 0UL) && ((val & (val - 1UL)) == 0UL);

		public static Delegate Cast(this Delegate @delegate, Type newType)
		{
			if (newType == null)
				throw Tracer.ThrowCritical(new ArgumentNullException(nameof(newType)), typeof(ExtensionMethods));

			var invocationList = @delegate.GetInvocationList();
			Delegate[] newList = new Delegate[invocationList.Length];

			try
			{
				for (int i = 0; i < invocationList.Length; i++)
					newList[i] = invocationList[i].GetMethodInfo().CreateDelegate(newType, invocationList[i].Target);
			}
			catch (ArgumentException exception) when (Tracer.ExceptionInformational(exception, typeof(ExtensionMethods)))
			{
				return null;
			}

			return Delegate.Combine(newList);
		}

		public static TDelegate Cast<TDelegate>(this Delegate @delegate) where TDelegate : class
		{
			return @delegate.Cast(typeof(TDelegate)) as TDelegate;
		}
	}
}