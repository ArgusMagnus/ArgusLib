#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/tgamma.c?h=v1.1.16
*/
/*
"A Precision Approximation of the Gamma Function" - Cornelius Lanczos (1964)
"Lanczos Implementation of the Gamma Function" - Paul Godfrey (2001)
"An Analysis of the Lanczos Gamma Approximation" - Glendon Ralph Pugh (2004)

approximation method:

                        (x - 0.5)         S(x)
Gamma(x) = (x + g - 0.5)         *  ----------------
                                    exp(x + g - 0.5)

with
                 a1      a2      a3            aN
S(x) ~= [ a0 + ----- + ----- + ----- + ... + ----- ]
               x + 1   x + 2   x + 3         x + N

with a0, a1, a2, a3,.. aN constants which depend on g.

for x < 0 the following reflection formula is used:

Gamma(x)*Gamma(-x) = -pi/(x sin(pi x))

most ideas and constants are from boost and python
*/
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		public static double Gamma(double x)
		{
			const double p2p1023 = 8.9884656743115795386465259539451e+307;
			const double gmhalf = 5.524680040776729583740234375;

			double absx, y;
			double dy, z, r;
			Ieee754Double u = new Ieee754Double() { Value = x };
			uint ix = u.HighWord & 0x7fffffff;

			/* special cases */
			if (ix >= 0x7ff00000)
				/* tgamma(nan)=nan, tgamma(inf)=inf, tgamma(-inf)=nan with invalid */
				return x + double.PositiveInfinity;
			if (ix < (0x3ff - 54) << 20)
				/* |x| < 2^-54: tgamma(x) ~ 1/x, +-0 raises div-by-zero */
				return 1 / x;

			/* integer arguments */
			/* raise inexact when non-integer */
			if (x == Floor(x))
			{
				if (u.SignBit)
					return 0 / 0.0;

				// n! for small integer n
				switch (x)
				{
					case 1: return 1.0;
					case 2: return 1.0;
					case 3: return 2.0;
					case 4: return 6.0;
					case 5: return 24.0;
					case 6: return 120.0;
					case 7: return 720.0;
					case 8: return 5040.0;
					case 9: return 40320.0;
					case 10: return 362880.0;
					case 11: return 3628800.0;
					case 12: return 39916800.0;
					case 13: return 479001600.0;
					case 14: return 6227020800.0;
					case 15: return 87178291200.0;
					case 16: return 1307674368000.0;
					case 17: return 20922789888000.0;
					case 18: return 355687428096000.0;
					case 19: return 6402373705728000.0;
					case 20: return 121645100408832000.0;
					case 21: return 2432902008176640000.0;
					case 22: return 51090942171709440000.0;
					case 23: return 1124000727777607680000.0;
				}
			}

			/* x >= 172: tgamma(x)=inf with overflow */
			/* x =< -184: tgamma(x)=+-0 with underflow */
			if (ix >= 0x40670000)
			{ /* |x| >= 184 */
				if (u.SignBit)
				{
					//FORCE_EVAL((float)(0x1p - 126 / x));
					if (Floor(x) * 0.5 == Floor(x * 0.5))
						return 0;
					return -0.0;
				}
				x *= p2p1023;
				return x;
			}

			absx = u.SignBit ? -x : x;

			/* handle the error of x + g - 0.5 */
			y = absx + gmhalf;
			if (absx > gmhalf)
			{
				dy = y - absx;
				dy -= gmhalf;
			}
			else
			{
				dy = y - gmhalf;
				dy -= absx;
			}

			z = absx - 0.5;
			r = S(absx) * Exp(-y);
			if (x < 0)
			{
				/* reflection formula for negative x */
				/* sinpi(absx) is not 0, integers are already handled */
				r = -Constants.Pi / (sinpi(absx) * absx * r);
				dy = -dy;
				z = -z;
			}
			r += dy * (gmhalf + 0.5) * r / y;
			z = Pow(y, 0.5 * z);
			y = r * z * z;
			return y;
		}

		/// <summary>
		/// sin(pi x) with x > 0x1p-100, if sin(pi*x)==0 the sign is arbitrary
		/// </summary>
		static double sinpi(double x)
		{
			int n;

			/* argument reduction: x = |x| mod 2 */
			/* spurious inexact when x is odd int */
			x = x * 0.5;
			x = 2.0 * (x - Floor(x));

			/* reduce x into [-.25,.25] */
			n = (int)(4 * x);
			n = (n + 1) / 2;
			x -= n * 0.5;

			x *= Constants.Pi;
			switch (n)
			{
				default: /* case 4 */
				case 0:
					return KernelSin(x, 0, 0);
				case 1:
					return KernelCos(x, 0);
				case 2:
					return KernelSin(-x, 0, 0);
				case 3:
					return -KernelCos(x, 0);
			}
		}

		/// <summary>
		/// S(x) rational function for positive x
		/// </summary>
		static double S(double x)
		{
			const double
				Snum0 = 23531376880.410759688572007674451636754734846804940,
				Snum1 = 42919803642.649098768957899047001988850926355848959,
				Snum2 = 35711959237.355668049440185451547166705960488635843,
				Snum3 = 17921034426.037209699919755754458931112671403265390,
				Snum4 = 6039542586.3520280050642916443072979210699388420708,
				Snum5 = 1439720407.3117216736632230727949123939715485786772,
				Snum6 = 248874557.86205415651146038641322942321632125127801,
				Snum7 = 31426415.585400194380614231628318205362874684987640,
				Snum8 = 2876370.6289353724412254090516208496135991145378768,
				Snum9 = 186056.26539522349504029498971604569928220784236328,
				Snum10 = 8071.6720023658162106380029022722506138218516325024,
				Snum11 = 210.82427775157934587250973392071336271166969580291,
				Snum12 = 2.5066282746310002701649081771338373386264310793408,
				Sden0 = 0,
				Sden1 = 39916800,
				Sden2 = 120543840,
				Sden3 = 150917976,
				Sden4 = 105258076,
				Sden5 = 45995730,
				Sden6 = 13339535,
				Sden7 = 2637558,
				Sden8 = 357423,
				Sden9 = 32670,
				Sden10 = 1925,
				Sden11 = 66,
				Sden12 = 1;

			double num = 0, den = 0;

			/* to avoid overflow handle large x differently */
			if (x < 8)
			{
				//for (i = N; i >= 0; i--)
				//{
				//	num = num * x + Snum[i];
				//	den = den * x + Sden[i];
				//}
				num = num * x + Snum12;
				den = den * x + Sden12;
				num = num * x + Snum11;
				den = den * x + Sden11;
				num = num * x + Snum10;
				den = den * x + Sden10;
				num = num * x + Snum9;
				den = den * x + Sden9;
				num = num * x + Snum8;
				den = den * x + Sden8;
				num = num * x + Snum7;
				den = den * x + Sden7;
				num = num * x + Snum6;
				den = den * x + Sden6;
				num = num * x + Snum5;
				den = den * x + Sden5;
				num = num * x + Snum4;
				den = den * x + Sden4;
				num = num * x + Snum3;
				den = den * x + Sden3;
				num = num * x + Snum2;
				den = den * x + Sden2;
				num = num * x + Snum1;
				den = den * x + Sden1;
				num = num * x + Snum0;
				den = den * x + Sden0;
			}
			else
			{
				//for (i = 0; i <= N; i++)
				//{
				//	num = num / x + Snum[i];
				//	den = den / x + Sden[i];
				//}
				num = num / x + Snum0;
				den = den / x + Sden0;
				num = num / x + Snum1;
				den = den / x + Sden1;
				num = num / x + Snum2;
				den = den / x + Sden2;
				num = num / x + Snum3;
				den = den / x + Sden3;
				num = num / x + Snum4;
				den = den / x + Sden4;
				num = num / x + Snum5;
				den = den / x + Sden5;
				num = num / x + Snum6;
				den = den / x + Sden6;
				num = num / x + Snum7;
				den = den / x + Sden7;
				num = num / x + Snum8;
				den = den / x + Sden8;
				num = num / x + Snum9;
				den = den / x + Sden9;
				num = num / x + Snum10;
				den = den / x + Sden10;
				num = num / x + Snum11;
				den = den / x + Sden11;
				num = num / x + Snum12;
				den = den / x + Sden12;
			}
			return num / den;
		}
	}
}
