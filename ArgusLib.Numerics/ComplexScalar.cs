using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;

namespace ArgusLib.Numerics
{
	public struct ComplexScalar : IScalar<ComplexScalar>
	{
		readonly Complex _value;

		public ComplexScalar(Complex value)
		{
			_value = value;
		}

		public static implicit operator ComplexScalar(Complex value) => new ComplexScalar(value);
		public static implicit operator Complex(ComplexScalar value) => value._value;

		ComplexScalar IScalar<ComplexScalar>.Add(ComplexScalar value) => this._value + value._value;
		ComplexScalar IScalar<ComplexScalar>.Subtract(ComplexScalar value) => this._value - value._value;
		ComplexScalar IScalar<ComplexScalar>.Multiply(ComplexScalar value) => this._value * value._value;
		ComplexScalar IScalar<ComplexScalar>.Divide(ComplexScalar value) => this._value / value._value;
		ComplexScalar IScalar<ComplexScalar>.Negate() => -this._value;
		public bool Equals(ComplexScalar value) => this._value == value._value;
		ComplexScalar IScalar<ComplexScalar>.Zero { get { return Complex.Zero; } }
		ComplexScalar IScalar<ComplexScalar>.One { get { return Complex.One; } }
		bool IScalar<ComplexScalar>.IsZero { get { return this._value == Complex.Zero; } }
		bool IScalar<ComplexScalar>.IsOne { get { return this._value == Complex.One; } }
		ComplexScalar IScalar<ComplexScalar>.Abs { get { return new Complex(this._value.Magnitude, 0); } }
		int IComparable<ComplexScalar>.CompareTo(ComplexScalar other) => this._value.Magnitude.CompareTo(other._value.Magnitude);

		public static ComplexScalar operator +(ComplexScalar a, ComplexScalar b) => a._value + b._value;
		public static ComplexScalar operator -(ComplexScalar a, ComplexScalar b) => a._value - b._value;
		public static ComplexScalar operator *(ComplexScalar a, ComplexScalar b) => a._value * b._value;
		public static ComplexScalar operator /(ComplexScalar a, ComplexScalar b) => a._value / b._value;
		public static ComplexScalar operator -(ComplexScalar scalar) => -scalar._value;
		public static bool operator ==(ComplexScalar a, ComplexScalar b) => a._value == b._value;
		public static bool operator !=(ComplexScalar a, ComplexScalar b) => a._value != b._value;

		public override bool Equals(object obj) => obj is ComplexScalar ? ((ComplexScalar)obj)._value == this._value : false;
		public override int GetHashCode() => this._value.GetHashCode();
		public override string ToString() => this._value.ToString();

		public string ToString(string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? Formats.General;
			if (format.StartsWith(Formats.General))
				return _value.ToString(format.Substring(Formats.General.Length), formatProvider);
			if (format.StartsWith(Formats.Cartesian))
			{
				format = format.Substring(Formats.Cartesian.Length);
				string re = _value.Real.ToString(format, formatProvider);
				string im = _value.Imaginary.ToString(format, formatProvider);
				return $"{re} + i*{im}";
			}
			if (format.StartsWith(Formats.Polar))
			{
				format = format.Substring(Formats.Polar.Length);
				string R = _value.Magnitude.ToString(format, formatProvider);
				string phi = _value.Phase.ToString(format, formatProvider);
				return $"{R}*exp(i*{phi})";
			}
			throw new FormatException($"Invalid format: {format}");
		}

		TryParseHandler<ComplexScalar> IParsable<ComplexScalar>.GetTryParseHandler() => TryParse;

		public static bool TryParse(string text, out ComplexScalar value, string format = null, IFormatProvider formatProvider = null)
		{
			format = format?.ToUpperInvariant() ?? Formats.General;
			if (format.StartsWith(Formats.General))
				return TryParseGeneral(text, out value, formatProvider);
			if (format.StartsWith(Formats.Cartesian))
				return TryParseCartesian(text, out value, formatProvider);
			if (format.StartsWith(Formats.Polar))
				return TryParsePolar(text, out value, formatProvider);
			throw new FormatException($"Invalid format: {format}");
		}

		static bool TryParseGeneral(string text, out ComplexScalar value, IFormatProvider formatProvider)
		{
			value = default(ComplexScalar);
			string[] parts = text.Split(',');
			if (parts.Length != 2)
				return false;
			parts[0] = parts[0].TrimStart().TrimStart('(');
			parts[1] = parts[1].TrimEnd().TrimEnd(')');
			double re;
			if (!double.TryParse(parts[0], NumberStyles.Any, formatProvider, out re))
				return false;
			double im;
			if (!double.TryParse(parts[1], NumberStyles.Any, formatProvider, out im))
				return false;
			value = new Complex(re, im);
			return true;
		}

		static bool TryParseCartesian(string text, out ComplexScalar value, IFormatProvider formatProvider)
		{
			value = default(ComplexScalar);
			NumberFormatInfo nfi = formatProvider?.GetFormat(typeof(double)) as NumberFormatInfo ?? NumberFormatInfo.CurrentInfo;
			text = text.Filter();
			int idxI = text.IndexOf('i');
			double re, im;
			if (idxI < 0)
			{
				bool success = double.TryParse(text, NumberStyles.Any, formatProvider, out re);
				value = new Complex(re, 0);
				return success;
			}
			int idxSign = Math.Max(text.LastIndexOf(nfi.NegativeSign, idxI), text.LastIndexOf(nfi.PositiveSign, idxI));
			string sRe = text.Substring(0, idxSign);
			idxI++;
			if (text[idxI] == '*')
				idxI++;
			string sIm = text.Substring(idxI);

			if (!double.TryParse(sRe, NumberStyles.Any, formatProvider, out re))
				return false;
			if (!double.TryParse(sIm, NumberStyles.Any, formatProvider, out im))
				return false;
			value = new Complex(re, im);
			return true;
		}

		static bool TryParsePolar(string text, out ComplexScalar value, IFormatProvider formatProvider)
		{
			const string exp = "*exp(i*";
			text = text.Filter().ToLowerInvariant();
			int idx = text.IndexOf(exp);
			double mag, phi;
			if (idx < 0)
			{
				if (!double.TryParse(text, NumberStyles.Any, formatProvider, out mag))
					return false;
				value = new Complex(mag, 0);
				return true;
			}
			string sMag = text.Substring(0, idx);
			string sPhi = text.Substring(idx + exp.Length).TrimEnd().TrimEnd(')');
			if (!double.TryParse(sMag, NumberStyles.Any, formatProvider, out mag))
				return false;
			if (!double.TryParse(sPhi, NumberStyles.Any, formatProvider, out phi))
				return false;
			value = Complex.FromPolarCoordinates(mag, phi);
			return true;
		}

		public static class Formats
		{
			/// <summary>
			/// Format: (Real, Imaginary)
			/// </summary>
			public const string General = "G";
			/// <summary>
			/// Format: Real + i*Imaginary
			/// </summary>
			public const string Cartesian = "C";
			/// <summary>
			/// Format: Magnitude*exp(i*Phase)
			/// </summary>
			public const string Polar = "P";
		}
	}
}
