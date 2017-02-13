#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/j0.c?h=v1.1.16
*/
/* origin: FreeBSD /usr/src/lib/msun/src/e_j0.c */
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
/* j0(x), y0(x)
 * Bessel function of the first and second kinds of order zero.
 * Method -- j0(x):
 *      1. For tiny x, we use j0(x) = 1 - x^2/4 + x^4/64 - ...
 *      2. Reduce x to |x| since j0(x)=j0(-x),  and
 *         for x in (0,2)
 *              j0(x) = 1-z/4+ z^2*R0/S0,  where z = x*x;
 *         (precision:  |j0-1+z/4-z^2R0/S0 |<2**-63.67 )
 *         for x in (2,inf)
 *              j0(x) = sqrt(2/(pi*x))*(p0(x)*cos(x0)-q0(x)*sin(x0))
 *         where x0 = x-pi/4. It is better to compute sin(x0),cos(x0)
 *         as follow:
 *              cos(x0) = cos(x)cos(pi/4)+sin(x)sin(pi/4)
 *                      = 1/sqrt(2) * (cos(x) + sin(x))
 *              sin(x0) = sin(x)cos(pi/4)-cos(x)sin(pi/4)
 *                      = 1/sqrt(2) * (sin(x) - cos(x))
 *         (To avoid cancellation, use
 *              sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
 *          to compute the worse one.)
 *
 *      3 Special cases
 *              j0(nan)= nan
 *              j0(0) = 1
 *              j0(inf) = 0
 *
 * Method -- y0(x):
 *      1. For x<2.
 *         Since
 *              y0(x) = 2/pi*(j0(x)*(ln(x/2)+Euler) + x^2/4 - ...)
 *         therefore y0(x)-2/pi*j0(x)*ln(x) is an even function.
 *         We use the following function to approximate y0,
 *              y0(x) = U(z)/V(z) + (2/pi)*(j0(x)*ln(x)), z= x^2
 *         where
 *              U(z) = u00 + u01*z + ... + u06*z^6
 *              V(z) = 1  + v01*z + ... + v04*z^4
 *         with absolute approximation error bounded by 2**-72.
 *         Note: For tiny x, U/V = u0 and j0(x)~1, hence
 *              y0(tiny) = u0 + (2/pi)*ln(tiny), (choose tiny<2**-27)
 *      2. For x>=2.
 *              y0(x) = sqrt(2/(pi*x))*(p0(x)*cos(x0)+q0(x)*sin(x0))
 *         where x0 = x-pi/4. It is better to compute sin(x0),cos(x0)
 *         by the method mentioned above.
 *      3. Special cases: y0(0)=-inf, y0(x<0)=NaN, y0(inf)=0.
 */
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		/// <summary>
		/// Returns the value of the zeroth order bessel function of the first kind at <paramref name="x"/>.
		/// </summary>
		public static double BesselJ0(double x)
		{
			/* R0/S0 on [0, 2.00] */
			const double
				R02 = 1.56249999999999947958e-02, /* 0x3F8FFFFF, 0xFFFFFFFD */
				R03 = -1.89979294238854721751e-04, /* 0xBF28E6A5, 0xB61AC6E9 */
				R04 = 1.82954049532700665670e-06, /* 0x3EBEB1D1, 0x0C503919 */
				R05 = -4.61832688532103189199e-09, /* 0xBE33D5E7, 0x73D63FCE */
				S01 = 1.56191029464890010492e-02, /* 0x3F8FFCE8, 0x82C8C2A4 */
				S02 = 1.16926784663337450260e-04, /* 0x3F1EA6D2, 0xDD57DBF4 */
				S03 = 5.13546550207318111446e-07, /* 0x3EA13B54, 0xCE84D5A9 */
				S04 = 1.16614003333790000205e-09; /* 0x3E1408BC, 0xF4745D8F */

			double z, r, s;
			uint ix = GET_HIGH_WORD(x);
			ix &= 0x7fffffff;

			/* j0(+-inf)=0, j0(nan)=nan */
			if (ix >= 0x7ff00000)
				return 1 / (x * x);
			x = Abs(x);

			if (ix >= 0x40000000)
			{  /* |x| >= 2 */
			   /* large ulp error near zeros: 2.4, 5.52, 8.6537,.. */
				return Bessel0Common(ix, x, 0);
			}

			/* 1 - x*x/4 + x*x*R(x^2)/S(x^2) */
			if (ix >= 0x3f200000)
			{  /* |x| >= 2**-13 */
			   /* up to 4ulp error close to 2 */
				z = x * x;
				r = z * (R02 + z * (R03 + z * (R04 + z * R05)));
				s = 1 + z * (S01 + z * (S02 + z * (S03 + z * S04)));
				return (1 + x / 2) * (1 - x / 2) + z * (r / s);
			}

			/* 1 - x*x/4 */
			/* prevent underflow */
			/* inexact should be raised when x!=0, this is not done correctly */
			if (ix >= 0x38000000)  /* |x| >= 2**-127 */
				x = 0.25 * x * x;
			return 1 - x;
		}

		/// <summary>
		/// Returns the value of the zeroth order bessel function of the second kind at <paramref name="x"/>.
		/// </summary>
		public static double BesselY0(double x)
		{
			const double
				u00 = -7.38042951086872317523e-02, /* 0xBFB2E4D6, 0x99CBD01F */
				u01 = 1.76666452509181115538e-01, /* 0x3FC69D01, 0x9DE9E3FC */
				u02 = -1.38185671945596898896e-02, /* 0xBF8C4CE8, 0xB16CFA97 */
				u03 = 3.47453432093683650238e-04, /* 0x3F36C54D, 0x20B29B6B */
				u04 = -3.81407053724364161125e-06, /* 0xBECFFEA7, 0x73D25CAD */
				u05 = 1.95590137035022920206e-08, /* 0x3E550057, 0x3B4EABD4 */
				u06 = -3.98205194132103398453e-11, /* 0xBDC5E43D, 0x693FB3C8 */
				v01 = 1.27304834834123699328e-02, /* 0x3F8A1270, 0x91C9C71A */
				v02 = 7.60068627350353253702e-05, /* 0x3F13ECBB, 0xF578C6C1 */
				v03 = 2.59150851840457805467e-07, /* 0x3E91642D, 0x7FF202FD */
				v04 = 4.41110311332675467403e-10; /* 0x3DFE5018, 0x3BD6D9EF */

			const double tpi = 6.36619772367581382433e-01;

			double z, u, v;
			uint ix, lx;
			BitConverterEx.UInt64BitsToUInt32(BitConverterEx.DoubleToUInt64Bits(x), out lx, out ix);
			// EXTRACT_WORDS(ix, lx, x);

			/* y0(nan)=nan, y0(<0)=nan, y0(0)=-inf, y0(inf)=0 */
			if ((ix << 1 | lx) == 0)
				return -1 / 0.0;
			if ((ix >> 31) != 0)
				return 0 / 0.0;
			if (ix >= 0x7ff00000)
				return 1 / x;

			if (ix >= 0x40000000)
			{  /* x >= 2 */
			   /* large ulp errors near zeros: 3.958, 7.086,.. */
				return Bessel0Common(ix, x, 1);
			}

			/* U(x^2)/V(x^2) + (2/pi)*j0(x)*log(x) */
			if (ix >= 0x3e400000)
			{  /* x >= 2**-27 */
			   /* large ulp error near the first zero, x ~= 0.89 */
				z = x * x;
				u = u00 + z * (u01 + z * (u02 + z * (u03 + z * (u04 + z * (u05 + z * u06)))));
				v = 1.0 + z * (v01 + z * (v02 + z * (v03 + z * v04)));
				return u / v + tpi * (BesselJ0(x) * Log(x));
			}
			return u00 + tpi * Log(x);
		}

		/// <summary>
		/// Common method when |x|>=2
		/// </summary>
		static double Bessel0Common(uint ix, double x, int y0)
		{
			double s, c, ss, cc, z;

			/*
			 * j0(x) = sqrt(2/(pi*x))*(p0(x)*cos(x-pi/4)-q0(x)*sin(x-pi/4))
			 * y0(x) = sqrt(2/(pi*x))*(p0(x)*sin(x-pi/4)+q0(x)*cos(x-pi/4))
			 *
			 * sin(x-pi/4) = (sin(x) - cos(x))/sqrt(2)
			 * cos(x-pi/4) = (sin(x) + cos(x))/sqrt(2)
			 * sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
			 */
			s = Sin(x);
			c = Cos(x);
			if (y0 != 0)
				c = -c;
			cc = s + c;
			/* avoid overflow in 2*x, big ulp error when x>=0x1p1023 */
			if (ix < 0x7fe00000)
			{
				ss = s - c;
				z = -Cos(2 * x);
				if (s * c < 0)
					cc = z / ss;
				else
					ss = z / cc;
				if (ix < 0x48000000)
				{
					if (y0 != 0)
						ss = -ss;
					cc = pzero(x) * cc - qzero(x) * ss;
				}
			}
			return Constants.SqrtPi_R * cc / Sqrt(x);
		}

		static double pzero(double x)
		{
			var tables = Functions.Tables.Get();
			double[] p, q;
			double z, r, s;
			uint ix = GET_HIGH_WORD(x);
			ix &= 0x7fffffff;
			if (ix >= 0x40200000) { p = tables.Bessel0_pR8; q = tables.Bessel0_pS8; }
			else if (ix >= 0x40122E8B) { p = tables.Bessel0_pR5; q = tables.Bessel0_pS5; }
			else if (ix >= 0x4006DB6D) { p = tables.Bessel0_pR3; q = tables.Bessel0_pS3; }
			else /*ix >= 0x40000000*/ { p = tables.Bessel0_pR2; q = tables.Bessel0_pS2; }
			z = 1.0 / (x * x);
			r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
			s = 1.0 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * q[4]))));
			return 1.0 + r / s;
		}

		static double qzero(double x)
		{
			var tables = Functions.Tables.Get();
			double[] p, q;
			double s, r, z;
			uint ix = GET_HIGH_WORD(x);
			ix &= 0x7fffffff;
			if (ix >= 0x40200000) { p = tables.Bessel0_qR8; q = tables.Bessel0_qS8; }
			else if (ix >= 0x40122E8B) { p = tables.Bessel0_qR5; q = tables.Bessel0_qS5; }
			else if (ix >= 0x4006DB6D) { p = tables.Bessel0_qR3; q = tables.Bessel0_qS3; }
			else /*ix >= 0x40000000*/ { p = tables.Bessel0_qR2; q = tables.Bessel0_qS2; }
			z = 1.0 / (x * x);
			r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
			s = 1.0 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * (q[4] + z * q[5])))));
			return (-.125 + r / s) / x;
		}
	}
}
