#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using ArgusLib.Diagnostics.Tracing;
using System;
using System.Linq;

namespace ArgusLib
{
	/// <summary>
	/// Represents a number in the range [0, 1]. Over-/underflow of arithmetic operations
	/// will always result in saturation (e.g. 0.6 + 0.7 = 1, 0.2 - 0.8 = 0, etc.).
	/// </summary>
	public struct Normalized : IFormattable, IEquatable<Normalized>, IComparable<Normalized>
	{
		#region Fields

		const uint MaxValue = uint.MaxValue;
		public static Normalized Zero => new Normalized(0u);
		public static Normalized One => new Normalized(MaxValue);

		readonly uint _val;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/>.
		/// </summary>
		/// <param name="val">A value in the range [0.0, 1.0].</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="val"/> must be in the range [0.0, 1.0].</exception>
		public Normalized(double val)
		{
			if (val < 0.0 || val > 1.0)
				throw Tracer.ThrowError<Normalized>(new ArgumentOutOfRangeException(nameof(val), string.Format(Exceptions.ArgumentOutOfRange_MustBeInRange, "[0.0, 1.0].")));
			_val = (uint)Math.Round(val * MaxValue);
		}

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/>. <paramref name="val"/> is wrapped around
		/// to be in the range [0.0, 1.0], meaning that only the fractional part of val is used
		/// (e.g. 234.678 -> 0.678).
		/// </summary>
		/// <param name="val">Any real number</param>
		/// <exception cref="ArgumentException"><paramref name="val"/> must not be either positive or negative infinity or NaN.</exception>
		public static Normalized WrapAround(double val)
		{
			if (double.IsInfinity(val) || double.IsNaN(val))
				throw Tracer.ThrowError<Normalized>(new ArgumentException($"Must not be positive or negative infinity or NaN."));

			if (val < 0.0)
			{
				val = -val;
				val = 1.0 - (val - Math.Floor(val));
			}
			else if (val > 1.0)
				val -= Math.Floor(val);
			return new Normalized(val);
		}

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/> from a <see cref="byte"/> value.
		/// The full range of <see cref="byte"/> is used, so that the returned <see cref="Normalized"/>
		/// represents the value <c><paramref name="val"/> / <see cref="byte.MaxValue"/></c>.
		/// </summary>
		public Normalized(byte val) { _val = ((uint)val << 24); }

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/> from a <see cref="ushort"/> value.
		/// The full range of <see cref="ushort"/> is used, so that the returned <see cref="Normalized"/>
		/// represents the value <c><paramref name="val"/> / <see cref="ushort.MaxValue"/></c>.
		/// </summary>
		public Normalized(ushort val) { _val = ((uint)val << 16); }

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/> from a <see cref="uint"/> value.
		/// The full range of <see cref="uint"/> is used, so that the returned <see cref="Normalized"/>
		/// represents the value <c><paramref name="val"/> / <see cref="uint.MaxValue"/></c>.
		/// </summary>
		public Normalized(uint val) { _val = val; }

		/// <summary>
		/// Instantiates a new <see cref="Normalized"/> from a <see cref="int"/> value.
		/// The full non-negative range of <see cref="int"/> is used, so that the returned <see cref="Normalized"/>
		/// represents the value <c><paramref name="val"/> / <see cref="int.MaxValue"/></c>.
		/// </summary>
		public Normalized(int val)
		{
			if (val < 0)
				throw Tracer.ThrowError<Normalized>(new ArgumentOutOfRangeException(nameof(val), Exceptions.ArgumentOutOfRange_MustNotBeNegative));
			_val = (uint)val << 1;
		}

		#endregion

		#region Casting / Conversion

		const float scSingle = 1.0f / MaxValue;
		const double scDouble = 1.0 / MaxValue;

		public static implicit operator float(Normalized r) => r._val * scSingle;
		public static implicit operator double(Normalized r) => r._val * scDouble;
		public static explicit operator Normalized(float r) => new Normalized(r);
		public static explicit operator Normalized(double r) => new Normalized(r);

		/// <summary>
		/// Returns a <see cref="byte"/> representing the <see cref="Normalized"/>
		/// with its full range: <c>byte val = normalizedValue * <see cref="byte.MaxValue"/></c>.
		/// </summary>
		/// <returns></returns>
		public byte ToByte() => (byte)(_val >> 24);

		/// <summary>
		/// Returns a <see cref="ushort"/> representing the <see cref="Normalized"/>
		/// with its full range: <c>ushort val = normalizedValue * <see cref="ushort.MaxValue"/></c>.
		/// </summary>
		/// <returns></returns>
		public ushort ToUInt16() => (ushort)(_val >> 16);

		/// <summary>
		/// Returns a <see cref="uint"/> representing the <see cref="Normalized"/>
		/// with its full range: <c>uint val = normalizedValue * <see cref="uint.MaxValue"/></c>.
		/// </summary>
		/// <returns></returns>
		public uint ToUInt32() => _val;

		/// <summary>
		/// Returns a <see cref="int"/> representing the <see cref="Normalized"/>
		/// with its full non-negative range: <c>int val = normalizedValue * <see cref="int.MaxValue"/></c>.
		/// </summary>
		/// <returns></returns>
		public int ToInt32() => (int)(_val >> 1);

		#endregion

		#region Arithmetic Operators

		public static Normalized operator +(Normalized a, Normalized b)
		{
			uint val = unchecked(a._val + b._val);
			if (val < a._val) // Overflow
				return Normalized.One;
			return new Normalized(val);
		}

		public static Normalized operator -(Normalized a, Normalized b)
		{
			uint val = unchecked(a._val - b._val);
			if (val > a._val) // Underflow
				return Normalized.Zero;
			return new Normalized(val);
		}

		public static Normalized operator *(Normalized a, int b)
		{
			if (b <= 0)
				return Normalized.Zero;
			ulong val = a._val * (ulong)b;
			if (val > MaxValue)
				return Normalized.One;
			return new Normalized((uint)val);
		}

		public static Normalized operator *(int a, Normalized b) => b * a;

		public static Normalized operator /(Normalized a, int b) => new Normalized((a._val) / b);

		public static Normalized operator *(Normalized a, double b)
		{
			const double max = MaxValue;

			if (b <= 0)
				return Normalized.Zero;

			double val = a._val * b;
			if (val > max)
				return Normalized.One;

			return new Normalized((uint)Math.Round(val));
		}

		public static Normalized operator *(double a, Normalized b) => b * a;

		public static Normalized operator *(Normalized a, Normalized b) => new Normalized((uint)(((ulong)a._val * (ulong)b._val) >> 32));

		public static Normalized operator /(Normalized a, Normalized b)
		{
			const ulong max = MaxValue;

			if (a >= b)
				return Normalized.One;

			ulong val = a._val * max / b._val;
			if (val > MaxValue)
				return Normalized.One;
			return new Normalized((uint)val);
		}

		#endregion

		#region Overrides / Interface Implementations

		public override string ToString() => ToString("F7");
		public string ToString(string format, IFormatProvider provider = null) => ((double)this).ToString(format, provider);

		public static bool operator ==(Normalized a, Normalized b) => a._val == b._val;
		public static bool operator !=(Normalized a, Normalized b) => a._val != b._val;
		public static bool operator <(Normalized a, Normalized b) => a._val < b._val;
		public static bool operator <=(Normalized a, Normalized b) => a._val <= b._val;
		public static bool operator >(Normalized a, Normalized b) => a._val > b._val;
		public static bool operator >=(Normalized a, Normalized b) => a._val >= b._val;

		public bool Equals(Normalized other) => this._val == other._val;
		public override bool Equals(object obj) => obj is Normalized ? (Normalized)obj == this : false;
		public override int GetHashCode() => this._val.GetHashCode();
		int IComparable<Normalized>.CompareTo(Normalized other) => this._val.CompareTo(other._val);

		#endregion

		#region Static Methods

		/// <summary>
		/// Returns the minimum <see cref="Normalized"/> in <paramref name="values"/>. 
		/// </summary>
		public static Normalized Min(params Normalized[] values) => values.Min();
		/// <summary>
		/// Returns the maximum <see cref="Normalized"/> in <paramref name="values"/>. 
		/// </summary>
		public static Normalized Max(params Normalized[] valuse) => valuse.Max();

		/// <summary>
		/// Adds two <see cref="Normalized"/> values, but in contrast to the default add operation,
		/// when overflow occurs, a wrapped around value is returned instead of a saturated one
		/// (e.g. AddWrapAround(0.6, 0.7) == 0.3 vs. 0.6 + 0.7 == 1.0).
		/// </summary>
		public static Normalized AddWrapAround(Normalized a, Normalized b) => new Normalized(unchecked(a._val + b._val));

		/// <summary>
		/// Adds two <see cref="Normalized"/> values, but in contrast to the default subtract operation,
		/// when underflow occurs, a wrapped around value is returned instead of a saturated one
		/// (e.g. SubtractWrapAround(0.6, 0.7) == 0.9 vs. 0.6 - 0.7 == 0.0).
		/// </summary>
		public static Normalized SubtractWrapAround(Normalized a, Normalized b) => new Normalized(unchecked(a._val - b._val));

		/// <summary>
		/// Returns the difference between two <see cref="Normalized"/> values as an absolute value.
		/// </summary>
		public static Normalized Difference(Normalized a, Normalized b) => new Normalized(a._val < b._val ? b._val - a._val : a._val - b._val);

		/// <summary>
		/// Adds two <see cref="Normalized"/> values, but in contrast to the default add operation,
		/// when overflow occurs, a value reflected on 1.0 is returned instead of a saturated one
		/// (e.g. AddReflect(0.6, 0.6) == 0.8 vs. 0.6 + 0.6 == 1.0).
		/// </summary>
		public static Normalized AddReflect(Normalized a, Normalized b)
		{
			uint val = unchecked(a._val + b._val);
			if (val < a._val) // Overflow
				val = MaxValue - val;
			return new Normalized(val);
		}

		#endregion
	}
}