#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/acosh.c?h=v1.1.16
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgusLib
{
    public static partial class Functions
	{
		/// <summary>
		/// acosh(x) = log(x + sqrt(x*x-1))
		/// </summary>
		public static double Acosh(double x)
		{
			uint e = (uint)(BitConverterEx.DoubleToUInt64Bits(x) >> 52) & 0x7FFu;

			/* x < 1 domain error is handled in the called functions */

			if (e < 0x3ff + 1)
				/* |x| < 2, up to 2ulp error in [1,1.125] */
				return Log1Plus(x - 1 + Sqrt((x - 1) * (x - 1) + 2 * (x - 1)));
			if (e < 0x3ff + 26)
				/* |x| < 0x1p26 */
				return Log(2 * x - 1 / (x + Sqrt(x * x - 1)));
			/* |x| >= 0x1p26 or nan */
			return Log(x) + 0.693147180559945309417232121458176568;
		}
	}
}
