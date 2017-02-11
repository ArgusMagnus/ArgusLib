#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgusLib.Numerics
{
	public delegate bool TryParseHandler<T>(string text, out T value, string format, IFormatProvider formatProvider);

	public interface IParsable<T>
	{
		/// <summary>
		/// The returned <see cref="TryParseHandler{T}"/> should invoke a static TryParse method and must not be <c>null</c>.
		/// </summary>
		TryParseHandler<T> GetTryParseHandler();
	}

	public static class Parser<T>
		where T : IParsable<T>, new()
	{
		static readonly TryParseHandler<T> _parser = new T().GetTryParseHandler();

		public static bool TryParse(string text, out T value, string format = null, IFormatProvider formatProvider = null) => _parser(text, out value, format, formatProvider);

		public static T TryParse(string text, T fallbackValue = default(T), string format = null, IFormatProvider formatProvider = null)
		{
			T val;
			if (_parser(text, out val, format, formatProvider))
				return val;
			return fallbackValue;
		}
	}

	public static class Parser
	{
		public static bool TryParse<T>(string text, out T value, string format = null, IFormatProvider formatProvider = null) where T : IParsable<T>, new() => Parser<T>.TryParse(text, out value, format, formatProvider);

		public static T TryParse<T>(string text, T fallbackValue = default(T), string format = null, IFormatProvider formatProvider = null) where T : IParsable<T>, new() => Parser<T>.TryParse(text, fallbackValue, format, formatProvider);
	}
}
