#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArgusLib
{
	namespace Internal
	{
		public abstract class EnumExCore<T> where T : class
		{
			internal protected EnumExCore() { }

			public static bool HasFlag<TEnum>(TEnum value, TEnum flag)
				where TEnum : struct, T
			{
				return Integer<TEnum>.HasFlag(value, flag);
			}

			public static TEnum RemoveFlag<TEnum>(TEnum value, TEnum flag)
				where TEnum : struct, T
			{
				return Integer<TEnum>.RemoveFlag(value, flag);
			}

			public static TEnum SetFlag<TEnum>(TEnum value, TEnum flag)
				where TEnum : struct, T
			{
				return Integer<TEnum>.SetFlag(value, flag);
			}

			public static int ToInt32<TEnum>(TEnum value)
				where TEnum : struct, T
			{
				return Integer<TEnum>.ToInt32(value);
			}

			public static long ToInt64<TEnum>(TEnum value)
				where TEnum : struct, T
			{
				return Integer<TEnum>.ToInt64(value);
			}

			public static TEnum FromInt32<TEnum>(int value)
				where TEnum : struct, T
			{
				return Integer<TEnum>.FromInt32(value);
			}

			public static TEnum FromInt64<TEnum>(long value)
				where TEnum : struct, T
			{
				return Integer<TEnum>.FromInt64(value);
			}

			public static Dictionary<string, TEnum> GetConstants<TEnum>()
				where TEnum : struct, T
			{
				string[] names = Enum.GetNames(typeof(TEnum));
				TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
				Dictionary<string, TEnum> RetVal = new Dictionary<string, TEnum>(names.Length);
				for (int i = 0; i < names.Length; i++)
					RetVal.Add(names[i], values[i]);
				return RetVal;
			}

			public static TEnum Parse<TEnum>(string value, bool ignoreCase = false)
				where TEnum : struct, T
			{
				return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
			}

			public static bool TryParse<TEnum>(string value, out TEnum result, bool ignoreCase = false)
				where TEnum : struct, T
			{
				return Enum.TryParse(value, ignoreCase, out result);
			}

			public static bool IsDefined<TEnum>(object value)
				where TEnum : struct, T
			{
				return Enum.IsDefined(typeof(TEnum), value);
			}

			public static string GetName<TEnum>(object value)
				where TEnum : struct, T
			{
				return Enum.GetName(typeof(TEnum), value);
			}

			public static Type GetUnderlyingType<TEnum>()
				where TEnum : struct, T
			{
				return Enum.GetUnderlyingType(typeof(TEnum));
			}

			public static TEnum FromValue<TEnum>(object value)
				where TEnum : struct, T
			{
				return (TEnum)Enum.ToObject(typeof(TEnum), value);
			}
		}
	}

	/// <summary>
	/// This class provides static helper methods for <c>enum</c> types.
	/// </summary>
	public sealed class EnumEx : Internal.EnumExCore<Enum>
	{
		EnumEx() { }
	}
}
