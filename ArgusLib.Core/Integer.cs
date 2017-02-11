#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;

namespace ArgusLib
{
	/// <summary>
	/// This class can only be used with primitive integer types and enums.
	/// </summary>
	/// <typeparam name="T">A primitive integer type or an enum.</typeparam>
	public static class Integer<T>
	{
		public static readonly Func<T, T, bool> HasFlag = Integer.GetHasFlagMethod<T>();
		public static readonly Func<T, T, T> RemoveFlag = Integer.GetRemoveFlagMethod<T>();
		public static readonly Func<T, T, T> SetFlag = Integer.GetSetFlagMethod<T>();
		public static readonly Func<T, int> ToInt32 = Integer.GetToInt32Method<T>();
		public static readonly Func<T, long> ToInt64 = Integer.GetToInt64Method<T>();
		public static readonly Func<int, T> FromInt32 = Integer.GetFromInt32Method<T>();
		public static readonly Func<long, T> FromInt64 = Integer.GetFromInt64Method<T>();
	}
}
