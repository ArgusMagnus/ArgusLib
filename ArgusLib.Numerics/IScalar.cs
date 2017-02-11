using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;

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

	public static class Scalar<T> where T : struct, IScalar<T>
	{
		public static readonly T Zero = new T().Zero;
		public static readonly T One = Zero.One;
	}
}
