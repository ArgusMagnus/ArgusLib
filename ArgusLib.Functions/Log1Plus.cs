#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/acosh.c?h=v1.1.16
*/
/* double log1p(double x)
 * Return the natural logarithm of 1+x.
 *
 * Method :
 *   1. Argument Reduction: find k and f such that
 *                      1+x = 2^k * (1+f),
 *         where  sqrt(2)/2 < 1+f < sqrt(2) .
 *
 *      Note. If k=0, then f=x is exact. However, if k!=0, then f
 *      may not be representable exactly. In that case, a correction
 *      term is need. Let u=1+x rounded. Let c = (1+x)-u, then
 *      log(1+x) - log(u) ~ c/u. Thus, we proceed to compute log(u),
 *      and add back the correction term c/u.
 *      (Note: when x > 2**53, one can simply return log(x))
 *
 *   2. Approximation of log(1+f): See log.c
 *
 *   3. Finally, log1p(x) = k*ln2 + log(1+f) + c/u. See log.c
 *
 * Special cases:
 *      log1p(x) is NaN with signal if x < -1 (including -INF) ;
 *      log1p(+INF) is +INF; log1p(-1) is -INF with signal;
 *      log1p(NaN) is that NaN with no signal.
 *
 * Accuracy:
 *      according to an error analysis, the error is always less than
 *      1 ulp (unit in the last place).
 *
 * Constants:
 * The hexadecimal values are the intended ones for the following
 * constants. The decimal values may be used, provided that the
 * compiler will convert from decimal to binary accurately enough
 * to produce the hexadecimal values shown.
 *
 * Note: Assuming log() return accurate answer, the following
 *       algorithm can be used to compute log1p(x) to within a few ULP:
 *
 *              u = 1+x;
 *              if(u==1.0) return x ; else
 *                         return log(u)*(x/(u-1.0));
 *
 *       See HP-15C Advanced Functions Handbook, p.193.
 */
#endregion

namespace ArgusLib
{
    public static partial class Functions
	{
		/// <summary>
		/// Returns Log(1+x)
		/// </summary>
		public static double Log1Plus(double x)
		{
			const double ln2_hi = 6.93147180369123816490e-01;	/* 3fe62e42 fee00000 */
			const double ln2_lo = 1.90821492927058770002e-10;	/* 3dea39ef 35793c76 */
			const double Lg1 = 6.666666666666735130e-01;		/* 3FE55555 55555593 */
			const double Lg2 = 3.999999999940941908e-01;		/* 3FD99999 9997FA04 */
			const double Lg3 = 2.857142874366239149e-01;		/* 3FD24924 94229359 */
			const double Lg4 = 2.222219843214978396e-01;		/* 3FCC71C5 1D8E78AF */
			const double Lg5 = 1.818357216161805012e-01;		/* 3FC74664 96CB03DE */
			const double Lg6 = 1.531383769920937332e-01;		/* 3FC39A09 D078C69F */
			const double Lg7 = 1.479819860511658591e-01;        /* 3FC2F112 DF3E5244 */

			Ieee754Double u = new Ieee754Double() { Value = x };
			double hfsq, f, c, s, z, R, w, t1, t2, dk;
			uint hx, hu;
			int k;
			f = 0;
			c = 0;

			hx = (uint)(u.Bits >> 32);
			k = 1;
			if (hx < 0x3fda827a || (hx >> 31) != 0)
			{  /* 1+x < sqrt(2)+ */
				if (hx >= 0xbff00000)
				{  /* x <= -1.0 */
					if (x == -1)
						return x / 0.0; /* log1p(-1) = -inf */
					return (x - x) / 0.0;     /* log1p(x<-1) = NaN */
				}
				if (hx << 1 < 0x3ca00000 << 1)
				{  /* |x| < 2**-53 */
				   /* underflow if subnormal */
					//if ((hx & 0x7ff00000) == 0)
					//	FORCE_EVAL((float)x);
					return x;
				}
				if (hx <= 0xbfd2bec4)
				{  /* sqrt(2)/2- <= 1+x < sqrt(2)+ */
					k = 0;
					c = 0;
					f = x;
				}
			}
			else if (hx >= 0x7ff00000)
				return x;
			if (k != 0)
			{
				u.Value = 1 + x;
				hu = u.HighWord;// (uint)(u.Bits >> 32);
				hu += 0x3ff00000 - 0x3fe6a09e;
				k = (int)(hu >> 20) - 0x3ff;
				/* correction term ~ log(1+x)-log(u), avoid underflow in c/u */
				if (k < 54)
				{
					c = k >= 2 ? 1 - (u.Value - x) : x - (u.Value - 1);
					c /= u.Value;
				}
				else
					c = 0;
				/* reduce u into [sqrt(2)/2, sqrt(2)] */
				hu = (hu & 0x000fffff) + 0x3fe6a09e;
				//u = new Ieee754Double((ulong)hu << 32 | (u.Bits & 0xffffffff));
				u.HighWord = hu;
				u.LowWord = 0xffffffff;
				f = u.Value - 1;
			}
			hfsq = 0.5 * f * f;
			s = f / (2.0 + f);
			z = s * s;
			w = z * z;
			t1 = w * (Lg2 + w * (Lg4 + w * Lg6));
			t2 = z * (Lg1 + w * (Lg3 + w * (Lg5 + w * Lg7)));
			R = t2 + t1;
			dk = k;
			return s * (hfsq + R) + (dk * ln2_lo + c) - hfsq + f + dk * ln2_hi;
		}
	}
}
