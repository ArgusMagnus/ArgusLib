namespace ArgusLib.Numerics
{
	using System;
	using System.Numerics;
	using System.Globalization;

	public struct Int32Scalar : IScalar<Int32Scalar> , IComparable<Int32Scalar>
	{
		readonly Int32 _value;

		public Int32Scalar(Int32 value)
		{
			_value = value;
		}

		public static implicit operator Int32Scalar(Int32 value) => new Int32Scalar(value);
		public static implicit operator Int32(Int32Scalar value) => value._value;

		Int32Scalar IScalar<Int32Scalar>.Add(Int32Scalar value) => this._value + value._value;
		Int32Scalar IScalar<Int32Scalar>.Subtract(Int32Scalar value) => this._value - value._value;
		Int32Scalar IScalar<Int32Scalar>.Multiply(Int32Scalar value) => this._value * value._value;
		Int32Scalar IScalar<Int32Scalar>.Divide(Int32Scalar value) => this._value / value._value;
		Int32Scalar IScalar<Int32Scalar>.Negate() => -this._value;
		Int32Scalar IScalar<Int32Scalar>.Zero { get { return default(Int32); } }
		Int32Scalar IScalar<Int32Scalar>.One { get { return (Int32)1; } }
		bool IScalar<Int32Scalar>.IsZero { get { return this._value == default(Int32); } }
		bool IScalar<Int32Scalar>.IsOne { get { return this._value == 1; } }
		Int32Scalar IScalar<Int32Scalar>.Abs { get { return Math.Abs(this._value); } }
		bool IEquatable<Int32Scalar>.Equals(Int32Scalar value) => this._value == value._value;
		int IComparable<Int32Scalar>.CompareTo(Int32Scalar value) => this._value == value._value ? 0 : (this._value < value._value ? -1 : 1);

		public static Int32Scalar operator +(Int32Scalar a, Int32Scalar b) => a._value + b._value;
		public static Int32Scalar operator -(Int32Scalar a, Int32Scalar b) => a._value - b._value;
		public static Int32Scalar operator *(Int32Scalar a, Int32Scalar b) => a._value * b._value;
		public static Int32Scalar operator /(Int32Scalar a, Int32Scalar b) => a._value / b._value;
		public static Int32Scalar operator -(Int32Scalar scalar) => -scalar._value;
		public static bool operator ==(Int32Scalar a, Int32Scalar b) => a._value == b._value;
		public static bool operator !=(Int32Scalar a, Int32Scalar b) => a._value != b._value;
		public static bool operator <(Int32Scalar a, Int32Scalar b) => a._value < b._value;
		public static bool operator >(Int32Scalar a, Int32Scalar b) => a._value > b._value;
		public static bool operator <=(Int32Scalar a, Int32Scalar b) => a._value <= b._value;
		public static bool operator >=(Int32Scalar a, Int32Scalar b) => a._value >= b._value;

		public override bool Equals(object obj) => obj is Int32Scalar ? ((Int32Scalar)obj)._value == this._value : false;
		public override int GetHashCode() => this._value.GetHashCode();
		public override string ToString() => this._value.ToString();
		public string ToString(string format = null, IFormatProvider formatProvider = null) => this._value.ToString(format, formatProvider);

		public static bool TryParse(string text, out Int32Scalar value, string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? "G";
			NumberStyles numStyle = format.StartsWith("X") ? NumberStyles.HexNumber : NumberStyles.Any;
			Int32 temp;
			bool success = Int32.TryParse(text, numStyle, formatProvider, out temp);
			value = temp;
			return success;
		}

		TryParseHandler<Int32Scalar> IParsable<Int32Scalar>.GetTryParseHandler() => TryParse;
	}

	public struct Int64Scalar : IScalar<Int64Scalar> , IComparable<Int64Scalar>
	{
		readonly Int64 _value;

		public Int64Scalar(Int64 value)
		{
			_value = value;
		}

		public static implicit operator Int64Scalar(Int64 value) => new Int64Scalar(value);
		public static implicit operator Int64(Int64Scalar value) => value._value;

		Int64Scalar IScalar<Int64Scalar>.Add(Int64Scalar value) => this._value + value._value;
		Int64Scalar IScalar<Int64Scalar>.Subtract(Int64Scalar value) => this._value - value._value;
		Int64Scalar IScalar<Int64Scalar>.Multiply(Int64Scalar value) => this._value * value._value;
		Int64Scalar IScalar<Int64Scalar>.Divide(Int64Scalar value) => this._value / value._value;
		Int64Scalar IScalar<Int64Scalar>.Negate() => -this._value;
		Int64Scalar IScalar<Int64Scalar>.Zero { get { return default(Int64); } }
		Int64Scalar IScalar<Int64Scalar>.One { get { return (Int64)1; } }
		bool IScalar<Int64Scalar>.IsZero { get { return this._value == default(Int64); } }
		bool IScalar<Int64Scalar>.IsOne { get { return this._value == 1; } }
		Int64Scalar IScalar<Int64Scalar>.Abs { get { return Math.Abs(this._value); } }
		bool IEquatable<Int64Scalar>.Equals(Int64Scalar value) => this._value == value._value;
		int IComparable<Int64Scalar>.CompareTo(Int64Scalar value) => this._value == value._value ? 0 : (this._value < value._value ? -1 : 1);

		public static Int64Scalar operator +(Int64Scalar a, Int64Scalar b) => a._value + b._value;
		public static Int64Scalar operator -(Int64Scalar a, Int64Scalar b) => a._value - b._value;
		public static Int64Scalar operator *(Int64Scalar a, Int64Scalar b) => a._value * b._value;
		public static Int64Scalar operator /(Int64Scalar a, Int64Scalar b) => a._value / b._value;
		public static Int64Scalar operator -(Int64Scalar scalar) => -scalar._value;
		public static bool operator ==(Int64Scalar a, Int64Scalar b) => a._value == b._value;
		public static bool operator !=(Int64Scalar a, Int64Scalar b) => a._value != b._value;
		public static bool operator <(Int64Scalar a, Int64Scalar b) => a._value < b._value;
		public static bool operator >(Int64Scalar a, Int64Scalar b) => a._value > b._value;
		public static bool operator <=(Int64Scalar a, Int64Scalar b) => a._value <= b._value;
		public static bool operator >=(Int64Scalar a, Int64Scalar b) => a._value >= b._value;

		public override bool Equals(object obj) => obj is Int64Scalar ? ((Int64Scalar)obj)._value == this._value : false;
		public override int GetHashCode() => this._value.GetHashCode();
		public override string ToString() => this._value.ToString();
		public string ToString(string format = null, IFormatProvider formatProvider = null) => this._value.ToString(format, formatProvider);

		public static bool TryParse(string text, out Int64Scalar value, string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? "G";
			NumberStyles numStyle = format.StartsWith("X") ? NumberStyles.HexNumber : NumberStyles.Any;
			Int64 temp;
			bool success = Int64.TryParse(text, numStyle, formatProvider, out temp);
			value = temp;
			return success;
		}

		TryParseHandler<Int64Scalar> IParsable<Int64Scalar>.GetTryParseHandler() => TryParse;
	}

	public struct SingleScalar : IScalar<SingleScalar> , IComparable<SingleScalar>
	{
		readonly Single _value;

		public SingleScalar(Single value)
		{
			_value = value;
		}

		public static implicit operator SingleScalar(Single value) => new SingleScalar(value);
		public static implicit operator Single(SingleScalar value) => value._value;

		SingleScalar IScalar<SingleScalar>.Add(SingleScalar value) => this._value + value._value;
		SingleScalar IScalar<SingleScalar>.Subtract(SingleScalar value) => this._value - value._value;
		SingleScalar IScalar<SingleScalar>.Multiply(SingleScalar value) => this._value * value._value;
		SingleScalar IScalar<SingleScalar>.Divide(SingleScalar value) => this._value / value._value;
		SingleScalar IScalar<SingleScalar>.Negate() => -this._value;
		SingleScalar IScalar<SingleScalar>.Zero { get { return default(Single); } }
		SingleScalar IScalar<SingleScalar>.One { get { return (Single)1; } }
		bool IScalar<SingleScalar>.IsZero { get { return this._value == default(Single); } }
		bool IScalar<SingleScalar>.IsOne { get { return this._value == 1; } }
		SingleScalar IScalar<SingleScalar>.Abs { get { return Math.Abs(this._value); } }
		bool IEquatable<SingleScalar>.Equals(SingleScalar value) => this._value == value._value;
		int IComparable<SingleScalar>.CompareTo(SingleScalar value) => this._value == value._value ? 0 : (this._value < value._value ? -1 : 1);

		public static SingleScalar operator +(SingleScalar a, SingleScalar b) => a._value + b._value;
		public static SingleScalar operator -(SingleScalar a, SingleScalar b) => a._value - b._value;
		public static SingleScalar operator *(SingleScalar a, SingleScalar b) => a._value * b._value;
		public static SingleScalar operator /(SingleScalar a, SingleScalar b) => a._value / b._value;
		public static SingleScalar operator -(SingleScalar scalar) => -scalar._value;
		public static bool operator ==(SingleScalar a, SingleScalar b) => a._value == b._value;
		public static bool operator !=(SingleScalar a, SingleScalar b) => a._value != b._value;
		public static bool operator <(SingleScalar a, SingleScalar b) => a._value < b._value;
		public static bool operator >(SingleScalar a, SingleScalar b) => a._value > b._value;
		public static bool operator <=(SingleScalar a, SingleScalar b) => a._value <= b._value;
		public static bool operator >=(SingleScalar a, SingleScalar b) => a._value >= b._value;

		public override bool Equals(object obj) => obj is SingleScalar ? ((SingleScalar)obj)._value == this._value : false;
		public override int GetHashCode() => this._value.GetHashCode();
		public override string ToString() => this._value.ToString();
		public string ToString(string format = null, IFormatProvider formatProvider = null) => this._value.ToString(format, formatProvider);

		public static bool TryParse(string text, out SingleScalar value, string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? "G";
			NumberStyles numStyle = format.StartsWith("X") ? NumberStyles.HexNumber : NumberStyles.Any;
			Single temp;
			bool success = Single.TryParse(text, numStyle, formatProvider, out temp);
			value = temp;
			return success;
		}

		TryParseHandler<SingleScalar> IParsable<SingleScalar>.GetTryParseHandler() => TryParse;
	}

	public struct DoubleScalar : IScalar<DoubleScalar> , IComparable<DoubleScalar>
	{
		readonly Double _value;

		public DoubleScalar(Double value)
		{
			_value = value;
		}

		public static implicit operator DoubleScalar(Double value) => new DoubleScalar(value);
		public static implicit operator Double(DoubleScalar value) => value._value;

		DoubleScalar IScalar<DoubleScalar>.Add(DoubleScalar value) => this._value + value._value;
		DoubleScalar IScalar<DoubleScalar>.Subtract(DoubleScalar value) => this._value - value._value;
		DoubleScalar IScalar<DoubleScalar>.Multiply(DoubleScalar value) => this._value * value._value;
		DoubleScalar IScalar<DoubleScalar>.Divide(DoubleScalar value) => this._value / value._value;
		DoubleScalar IScalar<DoubleScalar>.Negate() => -this._value;
		DoubleScalar IScalar<DoubleScalar>.Zero { get { return default(Double); } }
		DoubleScalar IScalar<DoubleScalar>.One { get { return (Double)1; } }
		bool IScalar<DoubleScalar>.IsZero { get { return this._value == default(Double); } }
		bool IScalar<DoubleScalar>.IsOne { get { return this._value == 1; } }
		DoubleScalar IScalar<DoubleScalar>.Abs { get { return Math.Abs(this._value); } }
		bool IEquatable<DoubleScalar>.Equals(DoubleScalar value) => this._value == value._value;
		int IComparable<DoubleScalar>.CompareTo(DoubleScalar value) => this._value == value._value ? 0 : (this._value < value._value ? -1 : 1);

		public static DoubleScalar operator +(DoubleScalar a, DoubleScalar b) => a._value + b._value;
		public static DoubleScalar operator -(DoubleScalar a, DoubleScalar b) => a._value - b._value;
		public static DoubleScalar operator *(DoubleScalar a, DoubleScalar b) => a._value * b._value;
		public static DoubleScalar operator /(DoubleScalar a, DoubleScalar b) => a._value / b._value;
		public static DoubleScalar operator -(DoubleScalar scalar) => -scalar._value;
		public static bool operator ==(DoubleScalar a, DoubleScalar b) => a._value == b._value;
		public static bool operator !=(DoubleScalar a, DoubleScalar b) => a._value != b._value;
		public static bool operator <(DoubleScalar a, DoubleScalar b) => a._value < b._value;
		public static bool operator >(DoubleScalar a, DoubleScalar b) => a._value > b._value;
		public static bool operator <=(DoubleScalar a, DoubleScalar b) => a._value <= b._value;
		public static bool operator >=(DoubleScalar a, DoubleScalar b) => a._value >= b._value;

		public override bool Equals(object obj) => obj is DoubleScalar ? ((DoubleScalar)obj)._value == this._value : false;
		public override int GetHashCode() => this._value.GetHashCode();
		public override string ToString() => this._value.ToString();
		public string ToString(string format = null, IFormatProvider formatProvider = null) => this._value.ToString(format, formatProvider);

		public static bool TryParse(string text, out DoubleScalar value, string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? "G";
			NumberStyles numStyle = format.StartsWith("X") ? NumberStyles.HexNumber : NumberStyles.Any;
			Double temp;
			bool success = Double.TryParse(text, numStyle, formatProvider, out temp);
			value = temp;
			return success;
		}

		TryParseHandler<DoubleScalar> IParsable<DoubleScalar>.GetTryParseHandler() => TryParse;
	}

}