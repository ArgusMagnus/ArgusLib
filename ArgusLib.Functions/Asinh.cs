#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/asinh.c?h=v1.1.16
*/
#endregion

namespace ArgusLib
{
    public static partial class Functions
	{
		public static double Asinh(double x)
		{
			ulong bits = BitConverterEx.DoubleToUInt64Bits(x);
			uint e = (uint)(bits >> 52) & 0x7ff;
			uint signBit = (uint)(bits >> 63);

			/* |x| */
			//u.i &= (uint64_t) - 1 / 2;
			//x = u.f;
			x = Abs(x);

			if (e >= 0x3ff + 26)
			{
				/* |x| >= 0x1p26 or inf or nan */
				x = Log(x) + 0.693147180559945309417232121458176568;
			}
			else if (e >= 0x3ff + 1)
			{
				/* |x| >= 2 */
				x = Log(2 * x + 1 / (Sqrt(x * x + 1) + x));
			}
			else if (e >= 0x3ff - 26)
			{
				/* |x| >= 0x1p-26, up to 1.6ulp error in [0.125,0.5] */
				x = Log1Plus(x + x * x / (Sqrt(x * x + 1) + 1));
			}
			else
			{
				/* |x| < 0x1p-26, raise inexact if x != 0 */
				//FORCE_EVAL(x + 0x1p120f);
			}
			return signBit == 1 ? -x : x;
		}
	}
}
