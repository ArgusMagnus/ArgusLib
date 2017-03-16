using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using ArgusLib.Diagnostics.Tracing;
using System.Reflection;

namespace ArgusLib
{
	public static class BitSet
	{
		public static bool HasFlag<T>(T bitSet, T flag) where T : struct, IConvertible => BitSet<T>.HasFlag(bitSet, flag);
		public static T SetFlag<T>(T bitSet, T flag) where T : struct, IConvertible => BitSet<T>.SetFlag(bitSet, flag);
		public static T ClearFlag<T>(T bitSet, T flag) where T : struct, IConvertible => BitSet<T>.ClearFlag(bitSet, flag);
		public static int ToInt32<T>(T bitSet) where T : struct, IConvertible => BitSet<T>.ToInt32(bitSet);
		public static long ToInt64<T>(T bitSet) where T : struct, IConvertible => BitSet<T>.ToInt64(bitSet);
		public static T FromInt32<T>(int bitSet) where T : struct, IConvertible => BitSet<T>.FromInt32(bitSet);
		public static T FromInt64<T>(long bitSet) where T : struct, IConvertible => BitSet<T>.FromInt64(bitSet);
	}

	static class BitSet<T> where T : struct, IConvertible
	{
		static Func<T, T, bool> _hasFlag;
		static Func<T, T, T> _setFlag;
		static Func<T, T, T> _clearFlag;
		static Func<T, int> _toInt32;
		static Func<T, long> _toInt64;
		static Func<int, T> _fromInt32;
		static Func<long, T> _fromInt64;

		public static bool HasFlag(T bitSet, T flag)
		{
			if (_hasFlag == null)
				_hasFlag = Initialize();
			return _hasFlag(bitSet, flag);

			Func<T, T, bool> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(bitSet));
				var par2 = Expression.Parameter(typeof(T), nameof(flag));
				Expression op;
				try
				{
					var val1 = ConvertToUnderlyingType(par1);
					var val2 = ConvertToUnderlyingType(par2);
					op = ConvertFromUnderlyingType(Expression.And(val1, val2));
				}
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(op, par2), par1, par2);
				return expr.Compile();
			}
		}

		public static T SetFlag(T bitSet, T flag)
		{
			if (_setFlag == null)
				_setFlag = Initialize();
			return _setFlag(bitSet, flag);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(bitSet));
				var par2 = Expression.Parameter(typeof(T), nameof(flag));
				Expression op;
				try
				{
					var val1 = ConvertToUnderlyingType(par1);
					var val2 = ConvertToUnderlyingType(par2);
					op = ConvertFromUnderlyingType(Expression.Or(val1, val2));
				}
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T ClearFlag(T bitSet, T flag)
		{
			if (_clearFlag == null)
				_clearFlag = Initialize();
			return _clearFlag(bitSet, flag);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(bitSet));
				var par2 = Expression.Parameter(typeof(T), nameof(flag));
				Expression op;
				try
				{
					var val1 = ConvertToUnderlyingType(par1);
					var val2 = ConvertToUnderlyingType(par2);
					op = ConvertFromUnderlyingType(Expression.And(val1, Expression.Not(val2)));
				}
				catch (Exception exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static int ToInt32(T bitSet)
		{
			if (_toInt32 == null)
				_toInt32 = Initialize();
			return _toInt32(bitSet);

			Func<T, int> Initialize()
			{
				var par = Expression.Parameter(typeof(T), nameof(bitSet));
				UnaryExpression op;
				try { op = Expression.Convert(par, typeof(int)); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, int>>(op, par);
				return expr.Compile();
			}
		}

		public static long ToInt64(T bitSet)
		{
			if (_toInt64 == null)
				_toInt64 = Initialize();
			return _toInt64(bitSet);

			Func<T, long> Initialize()
			{
				var par = Expression.Parameter(typeof(T), nameof(bitSet));
				UnaryExpression op;
				try { op = Expression.Convert(par, typeof(long)); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, long>>(op, par);
				return expr.Compile();
			}
		}

		public static T FromInt32(int bitSet)
		{
			if (_fromInt32 == null)
				_fromInt32 = Initialize();
			return _fromInt32(bitSet);

			Func<int, T> Initialize()
			{
				var par = Expression.Parameter(typeof(int), nameof(bitSet));
				UnaryExpression op;
				try { op = Expression.Convert(par, typeof(T)); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<int, T>>(op, par);
				return expr.Compile();
			}
		}

		public static T FromInt64(long bitSet)
		{
			if (_fromInt64 == null)
				_fromInt64 = Initialize();
			return _fromInt64(bitSet);

			Func<long, T> Initialize()
			{
				var par = Expression.Parameter(typeof(long), nameof(bitSet));
				UnaryExpression op;
				try { op = Expression.Convert(par, typeof(T)); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(BitSet<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<long, T>>(op, par);
				return expr.Compile();
			}
		}

		static Expression ConvertToUnderlyingType(Expression expr)
		{
			if (!typeof(T).GetTypeInfo().IsEnum)
				return expr;
			return Expression.Convert(expr, Enum.GetUnderlyingType(typeof(T)));
		}

		static Expression ConvertFromUnderlyingType(Expression expr)
		{
			if (!typeof(T).GetTypeInfo().IsEnum)
				return expr;
			return Expression.Convert(expr, typeof(T));
		}
	}
}