#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ArgusLib
{
	public static partial class Functions
	{
		public static double Abs(double x) => Math.Abs(x);
		public static double Acos(double x) => Math.Acos(x);
		public static double Asin(double x) => Math.Asin(x);
		public static double Atan(double x) => Math.Atan(x);
		public static double Atan(double y, double x) => Math.Atan2(y, x);
		public static double Ceil(double x) => Math.Ceiling(x);
		public static double Cos(double x) => Math.Cos(x);
		public static double Cosh(double x) => Math.Cosh(x);
		public static double Exp(double x) => Math.Exp(x);
		public static double Floor(double x) => Math.Floor(x);
		public static double IEEERemainder(double x, double y) => Math.IEEERemainder(x, y);
		public static double Log(double x) => Math.Log(x);
		public static double Log10(double x) => Math.Log10(x);
		public static double Max(double a, double b) => Math.Max(a, b);
		public static double Min(double a, double b) => Math.Min(a, b);
		public static double Pow(double b, double exp) => Math.Pow(b, exp);
		public static double Round(double x, int decimalDigits = 0) => Math.Round(x, decimalDigits);
		public static double Sin(double x) => Math.Sin(x);
		public static double Sinh(double x) => Math.Sinh(x);
		public static double Sqrt(double x) => Math.Sqrt(x);
		public static double Tan(double x) => Math.Tan(x);
		public static double Tanh(double x) => Math.Tanh(x);
		public static double Truncate(double x) => Math.Truncate(x);

		/// <summary>
		/// Returns the value of the polynomial p(x) = a0 + a1*x + a2*x^2 + a3*x^3 + ... evaluated at <paramref name="x"/>.
		/// </summary>
		/// <remarks>
		/// This method's implementation uses the Horner's method.
		/// </remarks>
		public static double Polynomial(double x, params double[] a)
		{
			if (a.Length == 0)
				return 0.0;
			if (a.Length == 1)
				return a[0];

			double val = a[a.Length - 1] * x;
			for (int i = a.Length - 2; i > 0; i--)
				val = x * (a[i] + val);
			return val + a[0];
		}

		static unsafe uint GET_LOW_WORD(double x) => ((uint*)&x)[0];
		static unsafe uint GET_HIGH_WORD(double x) => ((uint*)&x)[1];
		static unsafe double SET_LOW_WORD(double x, uint w)
		{
			((uint*)&x)[0] = w;
			return x;
		}
		static unsafe double SET_HIGH_WORD(double x, uint w)
		{
			((uint*)&x)[1] = w;
			return x;
		}
		static double copysign(double abs, double sign) => sign < 0.0 ? -Math.Abs(abs) : Math.Abs(abs);
	}
}
