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
	public struct Angle : IEquatable<Angle>, IFormattable
	{
		readonly Normalized _value;

		Angle(Normalized value)
		{
			_value = value;
		}

		/// <summary>
		/// Instantiates a new <see cref="Angle"/> with the same value as <paramref name="value"/>
		/// but makes sure that the value is in the range [0, 1] (e.g. 3.62 -> 0.62)
		/// </summary>
		public Angle(double value)
		{
			_value = Normalized.WrapAround(value);
		}

		/// <summary>
		/// Instantiates a new <see cref="Angle"/> with the same value as <paramref name="degrees"/>
		/// but makes sure that the value is in the range [0°, 360°] (e.g. 450 -> 90°)
		/// </summary>
		public static Angle FromDegrees(double degrees) => new Angle(degrees * Constants.Q_1_360);

		/// <summary>
		/// Instantiates a new <see cref="Angle"/> with the same value as <paramref name="radians"/>
		/// but makes sure that the value is in the range [0, <see cref="ConstantsD.TwoPi"/>] (e.g. 3*Pi -> Pi)
		/// </summary>
		public static Angle FromRadians(double radians) => new Angle(radians * Constants.TwoPi_R);

		public double Sin() => Math.Sin(this.ToRadians());
		public double Cos() => Math.Cos(this.ToRadians());
		public double Tan() => Math.Tan(this.ToRadians());
		public double Sinh() => Math.Sinh(this.ToRadians());
		public double Cosh() => Math.Cosh(this.ToRadians());
		public double Tanh() => Math.Tanh(this.ToRadians());

		public static Angle Asin(double value) => Angle.FromRadians(Math.Asin(value));
		public static Angle Acos(double value) => Angle.FromRadians(Math.Acos(value));
		public static Angle Atan(double value) => Angle.FromRadians(Math.Atan(value));
		public static Angle Atan(double y, double x) => Angle.FromRadians(Math.Atan2(y, x));

		public double ToDegrees() => (double)_value * 360.0;
		public double ToRadians() => (double)_value * Constants.TwoPi;
		public double ToNormalized() => (double)_value;
		public static explicit operator Normalized(Angle angle) => angle._value;
		public static explicit operator Angle(Normalized value) => new Angle(value);

		public static Angle operator +(Angle a, Angle b) => new Angle((double)a._value + (double)b._value);
		public static Angle operator -(Angle a, Angle b) => new Angle((double)a._value - (double)b._value);
		public static Angle operator -(Angle a) => new Angle((double)a._value + 0.5);
		public static Angle operator *(Angle a, double b) => new Angle((double)a._value * b);
		public static Angle operator *(double a, Angle b) => new Angle(a * (double)b._value);
		public static Angle operator /(Angle a, double b) => new Angle((double)a._value / b);
		public static bool operator ==(Angle a, Angle b)
		{
			if (a._value == b._value)
				return true;
			if ((a._value == Normalized.Zero || a._value == Normalized.One) && (b._value == Normalized.Zero || b._value == Normalized.One))
				return true;
			return false;
		}
		public static bool operator !=(Angle a, Angle b) => !(a == b);

		public bool Equals(Angle value) => this == value;
		public override bool Equals(object obj) => obj is Angle ? (Angle)obj == this : false;
		public override int GetHashCode() => _value.GetHashCode();
		public override string ToString() => ToString("F2");
		public string ToString(string format, IFormatProvider provider = null)
		{
			if (string.IsNullOrEmpty(format))
				format = Formats.Degrees;
			else
				format = format.ToUpperInvariant();

			if (format.StartsWith(Formats.General))
				return $"{this.ToDegrees().ToString("F0")}°";
			if (format.StartsWith(Formats.Degrees))
				return $"{this.ToDegrees().ToString(format.Substring(Formats.Degrees.Length))}°";
			if (format.StartsWith(Formats.Radians))
				return this.ToRadians().ToString(format.Substring(Formats.Radians.Length));
			if (format.StartsWith(Formats.MultipleOfPi))
				return $"{((double)this._value * 2.0).ToString(format.Substring(Formats.MultipleOfPi.Length))} Pi";
			if (format.StartsWith(Formats.Normalized))
				return this._value.ToString(format.Substring(Formats.Normalized.Length));

			throw new FormatException($"Invalid format: {format}");
		}

		public static class Formats
		{
			public const string General = "G";
			public const string Degrees = "D";
			public const string Radians = "R";
			public const string MultipleOfPi = "P";
			public const string Normalized = "N";
		}
	}
}
