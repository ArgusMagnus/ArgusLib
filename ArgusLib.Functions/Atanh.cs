#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/atanh.c?h=v1.1.16
*/
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		public static double Atanh(double x)
		{
			Ieee754Double u = new Ieee754Double() { Value = x };
			uint e = (uint)(u.Bits >> 52) & 0x7ff;
			double y;

			/* |x| */
			//u.i &= (uint64_t) - 1 / 2;
			//y = u.f;
			y = Abs(x);

			if (e < 0x3ff - 1)
			{
				if (e < 0x3ff - 32)
				{
					/* handle underflow */
					//if (e == 0)
					//	FORCE_EVAL((float)y);
				}
				else
				{
					/* |x| < 0.5, up to 1.7ulp error */
					y = 0.5 * Log1Plus(2 * y + 2 * y * y / (1 - y));
				}
			}
			else
			{
				/* avoid overflow */
				y = 0.5 * Log1Plus(2 * (y / (1 - y)));
			}
			return u.SignBit ? -y : y;
		}
	}
}
