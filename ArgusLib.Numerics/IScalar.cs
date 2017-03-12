using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Numerics
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Note to implementors:
	/// - The member of this interface should be implemented explicitly,
	///   they are not intended to be called from user code directly but
	///   by <see cref="Vector{T, Dim}"/>, <see cref="Matrix{T, ColDim, RowDim}"/>, etc.
	/// - If the implementing type is a struct or has a parameterless constructor,
	///   an instance of the type obtained with the parameterless constructor should be equal to <see cref="IScalar{T}.Zero"/>.
	/// </remarks>
	public interface IScalar<T> : IEquatable<T>, IParsable<T>, IFormattable, IComparable<T>
	{
		T Add(T value);
		T Subtract(T value);
		T Multiply(T value);
		T Divide(T value);
		T Negate();
		T Abs { get; }
		bool IsZero { get; }
		bool IsOne { get; }

		T Zero { get; }
		T One { get; }
	}

	//public static class Scalar<T> where T : struct, IScalar<T>
	//{
	//	public static readonly T Zero = new T().Zero;
	//	public static readonly T One = Zero.One;
	//}

	public static class Scalar<T>
	{
		static Func<T, T, T> _add;
		static Func<T, T, T> _subtract;
		static Func<T, T, T> _multiply;
		static Func<T, T, T> _divide;
		static Func<T, T> _negate;
		static Func<T, T> _abs;
		static Func<T> _getZero;
		static Func<T> _getOne;
		static Func<T, T, bool> _areEqual;

		public static T Add(T summand1, T summand2)
		{
			if (_add == null)
				_add = Initialize();
			return _add(summand1, summand2);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(summand1));
				var par2 = Expression.Parameter(typeof(T), nameof(summand2));
				BinaryExpression op;
				try { op = Expression.Add(par1, par2); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T Subtract(T minuend, T subtrahend)
		{
			if (_subtract == null)
				_subtract = Initialize();
			return _subtract(minuend, subtrahend);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(minuend));
				var par2 = Expression.Parameter(typeof(T), nameof(subtrahend));
				BinaryExpression op;
				try { op = Expression.Subtract(par1, par2); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T Multiply(T factor1, T factor2)
		{
			if (_multiply == null)
				_multiply = Initialize();
			return _multiply(factor1, factor2);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(factor1));
				var par2 = Expression.Parameter(typeof(T), nameof(factor2));
				BinaryExpression op;
				try { op = Expression.Multiply(par1, par2); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T Divide(T dividend, T divisor)
		{
			if (_divide == null)
				_divide = Initialize();
			return _divide(dividend, divisor);

			Func<T, T, T> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(dividend));
				var par2 = Expression.Parameter(typeof(T), nameof(divisor));
				BinaryExpression op;
				try { op = Expression.Divide(par1, par2); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, T>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T Negate(T value)
		{
			if (_negate == null)
				_negate = Initialize();
			return _negate(value);

			Func<T, T> Initialize()
			{
				var par = Expression.Parameter(typeof(T), nameof(value));
				UnaryExpression op;
				try { op = Expression.Negate(par); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T>>(op, par);
				return expr.Compile();
			}
		}

		public static bool AreEqual(T value1, T value2)
		{
			if (_areEqual == null)
				_areEqual = Initialize();
			return _areEqual(value1, value2);

			Func<T, T, bool> Initialize()
			{
				var par1 = Expression.Parameter(typeof(T), nameof(value1));
				var par2 = Expression.Parameter(typeof(T), nameof(value2));
				BinaryExpression op;
				try { op = Expression.Equal(par1, par2); }
				catch (InvalidOperationException exception) when (Tracer.ExceptionCritical(exception, typeof(Scalar<T>), catchException: false)) { return null; }
				var expr = Expression.Lambda<Func<T, T, bool>>(op, par1, par2);
				return expr.Compile();
			}
		}

		public static T Zero
		{
			get
			{
				if (_getZero == null)
					_getZero = Initialize();
				return _getZero();

				Func<T> Initialize()
				{
					var propOrFieldAccess = GetPropertyOrFieldAccess(nameof(Zero));
					if (propOrFieldAccess != null)
						return propOrFieldAccess;

					T value;
					try
					{
						var expr = Expression.Lambda<Func<T>>(Expression.Convert(Expression.Constant(0), typeof(T)));
						var func = expr.Compile();
						value = func();
					}
					catch
					{
						throw Tracer.ThrowCritical(new InvalidOperationException($"{typeof(T).FullName} does not declare a public static field or property named '{nameof(Zero)}' which returns an instance of type '{typeof(T).FullName}' and the attempt to cast an integer to the target type failed."), typeof(Scalar<T>));
					}
					return () => value;
				}
			}
		}

		public static T One
		{
			get
			{
				if (_getOne == null)
					_getOne = Initialize();
				return _getOne();

				Func<T> Initialize()
				{
					var propOrFieldAccess = GetPropertyOrFieldAccess(nameof(One));
					if (propOrFieldAccess != null)
						return propOrFieldAccess;

					T value;
					try
					{
						var expr = Expression.Lambda<Func<T>>(Expression.Convert(Expression.Constant(1), typeof(T)));
						var func = expr.Compile();
						value = func();
					}
					catch (InvalidCastException)
					{
						throw Tracer.ThrowCritical(new InvalidOperationException($"{typeof(T).FullName} does not declare a public static field or property named '{nameof(One)}' which returns an instance of type '{typeof(T).FullName}' and the attempt to cast an integer to the target type failed."), typeof(Scalar<T>));
					}
					return () => value;
				}
			}
		}

		static Func<T> GetPropertyOrFieldAccess(string name)
		{
			var typeInfo = typeof(T).GetTypeInfo();

			var mPropZero = typeInfo.GetDeclaredProperty(name);
			if (mPropZero != null && mPropZero.GetMethod.IsStatic && typeInfo.IsAssignableFrom(mPropZero.PropertyType.GetTypeInfo()))
				return mPropZero.GetMethod.CreateDelegate(typeof(Func<T>)) as Func<T>;

			var mFieldZero = typeInfo.GetDeclaredField(name);
			if (mFieldZero != null && mFieldZero.IsStatic && typeInfo.IsAssignableFrom(mFieldZero.FieldType.GetTypeInfo()))
				return Expression.Lambda<Func<T>>(Expression.Field(null, mFieldZero)).Compile();

			return null;
		}
	}
}
