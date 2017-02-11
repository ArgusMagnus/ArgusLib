#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/hypot.c?h=v1.1.16
*/
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		public static double Hypotenuse(double x, double y)
		{
			const double Pow2P700 = Constants.Pow2P512 * Constants.Pow2P128 * (1UL << 60);
			const double Pow2M700 = 1.0 / Pow2P700;

			Ieee754Double ux = new Ieee754Double() { Value = x };
			Ieee754Double uy = new Ieee754Double() { Value = y };
			Ieee754Double ut;
			int ex, ey;
			double hx, lx, hy, ly, z;

			/* arrange |x| >= |y| */
			ux.Bits &= unchecked((ulong)-1L) >> 1;
			uy.Bits &= unchecked((ulong)-1L) >> 1;
			if (ux.Bits < uy.Bits)
			{
				ut = ux;
				ux = uy;
				uy = ut;
			}

			/* special cases */
			ex = (int)(ux.Bits >> 52);
			ey = (int)(uy.Bits >> 52);
			x = ux.Value;
			y = uy.Value;
			/* note: hypot(inf,nan) == inf */
			if (ey == 0x7ff)
				return y;
			if (ex == 0x7ff || uy.Bits == 0)
				return x;
			/* note: hypot(x,y) ~= x + y*y/x/2 with inexact for small y/x */
			/* 64 difference is enough for ld80 double_t */
			if (ex - ey > 64)
				return x + y;

			/* precise sqrt argument in nearest rounding mode without overflow */
			/* xh*xh must not overflow and xl*xl must not underflow in sq */
			z = 1;
			if (ex > 0x3ff + 510)
			{
				z = Pow2P700;
				x *= Pow2M700;
				y *= Pow2M700;
			}
			else if (ey < 0x3ff - 450)
			{
				z = Pow2M700;
				x *= Pow2P700;
				y *= Pow2P700;
			}
			sq(out hx, out lx, x);
			sq(out hy, out ly, y);
			return z * Sqrt(ly + lx + hy + hx);
		}

		static void sq(out double hi, out double lo, double x)
		{
			const double SPLIT = (1UL << 27) + 1UL;
			double xh, xl, xc;

			xc = x * SPLIT;
			xh = x - xc + xc;
			xl = x - xh;
			hi = x * x;
			lo = xh * xh - hi + 2 * xh * xl + xl * xl;
		}
	}
}
