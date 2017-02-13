#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/j1.c?h=v1.1.16
*/
/* origin: FreeBSD /usr/src/lib/msun/src/e_j1.c */
/*
 * ====================================================
 * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
 *
 * Developed at SunSoft, a Sun Microsystems, Inc. business.
 * Permission to use, copy, modify, and distribute this
 * software is freely granted, provided that this notice
 * is preserved.
 * ====================================================
 */
/* j1(x), y1(x)
 * Bessel function of the first and second kinds of order one.
 * Method -- j1(x):
 *      1. For tiny x, we use j1(x) = x/2 - x^3/16 + x^5/384 - ...
 *      2. Reduce x to |x| since j1(x)=-j1(-x),  and
 *         for x in (0,2)
 *              j1(x) = x/2 + x*z*R0/S0,  where z = x*x;
 *         (precision:  |j1/x - 1/2 - R0/S0 |<2**-61.51 )
 *         for x in (2,inf)
 *              j1(x) = sqrt(2/(pi*x))*(p1(x)*cos(x1)-q1(x)*sin(x1))
 *              y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x1)+q1(x)*cos(x1))
 *         where x1 = x-3*pi/4. It is better to compute sin(x1),cos(x1)
 *         as follow:
 *              cos(x1) =  cos(x)cos(3pi/4)+sin(x)sin(3pi/4)
 *                      =  1/sqrt(2) * (sin(x) - cos(x))
 *              sin(x1) =  sin(x)cos(3pi/4)-cos(x)sin(3pi/4)
 *                      = -1/sqrt(2) * (sin(x) + cos(x))
 *         (To avoid cancellation, use
 *              sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
 *          to compute the worse one.)
 *
 *      3 Special cases
 *              j1(nan)= nan
 *              j1(0) = 0
 *              j1(inf) = 0
 *
 * Method -- y1(x):
 *      1. screen out x<=0 cases: y1(0)=-inf, y1(x<0)=NaN
 *      2. For x<2.
 *         Since
 *              y1(x) = 2/pi*(j1(x)*(ln(x/2)+Euler)-1/x-x/2+5/64*x^3-...)
 *         therefore y1(x)-2/pi*j1(x)*ln(x)-1/x is an odd function.
 *         We use the following function to approximate y1,
 *              y1(x) = x*U(z)/V(z) + (2/pi)*(j1(x)*ln(x)-1/x), z= x^2
 *         where for x in [0,2] (abs err less than 2**-65.89)
 *              U(z) = U0[0] + U0[1]*z + ... + U0[4]*z^4
 *              V(z) = 1  + v0[0]*z + ... + v0[4]*z^5
 *         Note: For tiny x, 1/x dominate y1 and hence
 *              y1(tiny) = -2/pi/tiny, (choose tiny<2**-54)
 *      3. For x>=2.
 *              y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x1)+q1(x)*cos(x1))
 *         where x1 = x-3*pi/4. It is better to compute sin(x1),cos(x1)
 *         by method mentioned above.
 */
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		/// <summary>
		/// Returns the value of the first order bessel function of the first kind at <paramref name="x"/>.
		/// </summary>
		public static double BesselJ1(double x)
		{
			/* R0/S0 on [0,2] */
			const double
				r00 = -6.25000000000000000000e-02, /* 0xBFB00000, 0x00000000 */
				r01 = 1.40705666955189706048e-03, /* 0x3F570D9F, 0x98472C61 */
				r02 = -1.59955631084035597520e-05, /* 0xBEF0C5C6, 0xBA169668 */
				r03 = 4.96727999609584448412e-08, /* 0x3E6AAAFA, 0x46CA0BD9 */
				s01 = 1.91537599538363460805e-02, /* 0x3F939D0B, 0x12637E53 */
				s02 = 1.85946785588630915560e-04, /* 0x3F285F56, 0xB9CDF664 */
				s03 = 1.17718464042623683263e-06, /* 0x3EB3BFF8, 0x333F8498 */
				s04 = 5.04636257076217042715e-09, /* 0x3E35AC88, 0xC97DFF2C */
				s05 = 1.23542274426137913908e-11; /* 0x3DAB2ACF, 0xCFB97ED8 */

			double z, r, s;
			uint ix;
			int signBit;

			ix = GET_HIGH_WORD(x);
			signBit = (int)(ix >> 31);
			ix &= 0x7fffffff;
			if (ix >= 0x7ff00000)
				return 1 / (x * x);
			if (ix >= 0x40000000)  /* |x| >= 2 */
				return Bessel1Common(ix, Abs(x), 0, signBit);
			if (ix >= 0x38000000)
			{  /* |x| >= 2**-127 */
				z = x * x;
				r = z * (r00 + z * (r01 + z * (r02 + z * r03)));
				s = 1 + z * (s01 + z * (s02 + z * (s03 + z * (s04 + z * s05))));
				z = r / s;
			}
			else
				/* avoid underflow, raise inexact if x!=0 */
				z = x;
			return (0.5 + z) * x;
		}

		/// <summary>
		/// Returns the value of the first order bessel function of the second kind at <paramref name="x"/>.
		/// </summary>
		public static double BesselY1(double x)
		{
			const double
				U00 = -1.96057090646238940668e-01, /* 0xBFC91866, 0x143CBC8A */
				U01 = 5.04438716639811282616e-02, /* 0x3FA9D3C7, 0x76292CD1 */
				U02 = -1.91256895875763547298e-03, /* 0xBF5F55E5, 0x4844F50F */
				U03 = 2.35252600561610495928e-05, /* 0x3EF8AB03, 0x8FA6B88E */
				U04 = -9.19099158039878874504e-08; /* 0xBE78AC00, 0x569105B8 */

			const double
				V00 = 1.99167318236649903973e-02, /* 0x3F94650D, 0x3F4DA9F0 */
				V01 = 2.02552581025135171496e-04, /* 0x3F2A8C89, 0x6C257764 */
				V02 = 1.35608801097516229404e-06, /* 0x3EB6C05A, 0x894E8CA6 */
				V03 = 6.22741452364621501295e-09, /* 0x3E3ABF1D, 0x5BA69A86 */
				V04 = 1.66559246207992079114e-11; /* 0x3DB25039, 0xDACA772A */

			const double tpi = 6.36619772367581382433e-01; /* 0x3FE45F30, 0x6DC9C883 */
			double z, u, v;
			uint ix, lx;

			//EXTRACT_WORDS(ix, lx, x);
			BitConverterEx.UInt64BitsToUInt32(BitConverterEx.DoubleToUInt64Bits(x), out lx, out ix);
			/* y1(nan)=nan, y1(<0)=nan, y1(0)=-inf, y1(inf)=0 */
			if ((ix << 1 | lx) == 0)
				return -1 / 0.0;
			if ((ix >> 31) != 0)
				return 0 / 0.0;
			if (ix >= 0x7ff00000)
				return 1 / x;

			if (ix >= 0x40000000)  /* x >= 2 */
				return Bessel1Common(ix, x, 1, 0);
			if (ix < 0x3c900000)  /* x < 2**-54 */
				return -tpi / x;
			z = x * x;
			u = U00 + z * (U01 + z * (U02 + z * (U03 + z * U04)));
			v = 1 + z * (V00 + z * (V01 + z * (V02 + z * (V03 + z * V04))));
			return x * (u / v) + tpi * (BesselJ1(x) * Log(x) - 1 / x);
		}

		static double Bessel1Common(uint ix, double x, int y1, int signBit)
		{
			double z, s, c, ss, cc;

			/*
			 * j1(x) = sqrt(2/(pi*x))*(p1(x)*cos(x-3pi/4)-q1(x)*sin(x-3pi/4))
			 * y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x-3pi/4)+q1(x)*cos(x-3pi/4))
			 *
			 * sin(x-3pi/4) = -(sin(x) + cos(x))/sqrt(2)
			 * cos(x-3pi/4) = (sin(x) - cos(x))/sqrt(2)
			 * sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
			 */
			s = Sin(x);
			if (y1 != 0)
				s = -s;
			c = Cos(x);
			cc = s - c;
			if (ix < 0x7fe00000)
			{
				/* avoid overflow in 2*x */
				ss = -s - c;
				z = Cos(2 * x);
				if (s * c > 0)
					cc = z / ss;
				else
					ss = z / cc;
				if (ix < 0x48000000)
				{
					if (y1 != 0)
						ss = -ss;
					cc = pone(x) * cc - qone(x) * ss;
				}
			}
			if (signBit != 0)
				cc = -cc;
			return Constants.SqrtPi_R * cc / Sqrt(x);
		}

		static double pone(double x)
		{
			var tables = Functions.Tables.Get();
			double[] p, q;
			double z, r, s;
			uint ix = GET_HIGH_WORD(x);
			ix &= 0x7fffffff;
			if (ix >= 0x40200000) { p = tables.Bessel1_pr8; q = tables.Bessel1_ps8; }
			else if (ix >= 0x40122E8B) { p = tables.Bessel1_pr5; q = tables.Bessel1_ps5; }
			else if (ix >= 0x4006DB6D) { p = tables.Bessel1_pr3; q = tables.Bessel1_ps3; }
			else /*ix >= 0x40000000*/ { p = tables.Bessel1_pr2; q = tables.Bessel1_ps2; }
			z = 1.0 / (x * x);
			r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
			s = 1.0 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * q[4]))));
			return 1.0 + r / s;
		}

		static double qone(double x)
		{
			var tables = Functions.Tables.Get();
			double[] p, q;
			double s, r, z;
			uint ix = GET_HIGH_WORD(x);
			ix &= 0x7fffffff;
			if (ix >= 0x40200000) { p = tables.Bessel1_qr8; q = tables.Bessel1_qs8; }
			else if (ix >= 0x40122E8B) { p = tables.Bessel1_qr5; q = tables.Bessel1_qs5; }
			else if (ix >= 0x4006DB6D) { p = tables.Bessel1_qr3; q = tables.Bessel1_qs3; }
			else /*ix >= 0x40000000*/ { p = tables.Bessel1_qr2; q = tables.Bessel1_qs2; }
			z = 1.0 / (x * x);
			r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
			s = 1.0 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * (q[4] + z * q[5])))));
			return (.375 + r / s) / x;
		}
	}
}
