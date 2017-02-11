#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/nextafter.c?h=v1.1.16
*/
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		/// <summary>
		/// Returns the next representable value after <paramref name="x"/> in the direction of <paramref name="y"/>.
		/// </summary>
		public static double NextAfter(double x, double y)
		{
			Ieee754Double ux = new Ieee754Double() { Value = x };
			Ieee754Double uy = new Ieee754Double() { Value = y };
			ulong ax, ay;
			int e;

			if (double.IsNaN(x) || double.IsNaN(y))
				return x + y;
			if (ux.Bits == uy.Bits)
				return y;
			ax = ux.Bits & unchecked((ulong)-1) / 2;
			ay = uy.Bits & unchecked((ulong)-1) / 2;
			if (ax == 0)
			{
				if (ay == 0)
					return y;
				ux.Bits = (uy.Bits & (1UL << 63)) | 1;
			}
			else if (ax > ay || ((ux.Bits ^ uy.Bits) & (1UL << 63)) != 0)
				ux.Bits--;
			else
				ux.Bits++;

			//e = (int)(ux.Bits >> 52) & 0x7ff;
			///* raise overflow if ux.f is infinite and x is finite */
			//if (e == 0x7ff)
			//	FORCE_EVAL(x + x);
			///* raise underflow if ux.f is subnormal or zero */
			//if (e == 0)
			//	FORCE_EVAL(x * x + ux.f * ux.f);
			return ux.Value;
		}
	}
}
