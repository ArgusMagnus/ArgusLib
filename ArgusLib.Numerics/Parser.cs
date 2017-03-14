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
using System.Reflection;
using System.Linq;
using ArgusLib.Diagnostics.Tracing;

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
	{
		static TryParseHandler<T> _parser;

		public static bool TryParse(string text, out T value, string format = null, IFormatProvider formatProvider = null)
		{
			if (_parser == null)
				_parser = Initialize();
			return _parser(text, out value, format, formatProvider);

			TryParseHandler<T> Initialize()
			{
				var implementedInterfaces = typeof(T).GetTypeInfo().ImplementedInterfaces;

				if (implementedInterfaces.Contains(typeof(IParsable<T>)))
					return (Activator.CreateInstance<T>() as IParsable<T>).GetTryParseHandler();
				if (implementedInterfaces.Contains(typeof(IConvertible)))
					return ConvertibleTryParseHandler;

				throw Tracer.ThrowCritical(new GenericTypeParameterNotSupportetException<T>(), typeof(Parser<T>));

				bool ConvertibleTryParseHandler(string str, out T v, string fmt, IFormatProvider fmtProvider)
				{
					v = default(T);
					try { v = (T)Convert.ChangeType(str, typeof(T), fmtProvider); }
					catch (Exception exception) when (Tracer.ExceptionInformational(exception, typeof(Parser))) { return false; }
					return true;
				}
			}
		}

		public static T TryParse(string text, T fallbackValue = default(T), string format = null, IFormatProvider formatProvider = null)
		{
			if (TryParse(text, out var val, format, formatProvider))
				return val;
			return fallbackValue;
		}
	}

	public static class Parser
	{
		public static bool TryParse<T>(string text, out T value, string format = null, IFormatProvider formatProvider = null) => Parser<T>.TryParse(text, out value, format, formatProvider);

		public static T TryParse<T>(string text, T fallbackValue = default(T), string format = null, IFormatProvider formatProvider = null) => Parser<T>.TryParse(text, fallbackValue, format, formatProvider);
	}
}
