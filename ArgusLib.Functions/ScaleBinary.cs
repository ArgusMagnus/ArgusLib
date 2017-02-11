#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)

Ported to C# from musl libc source:
http://git.musl-libc.org/cgit/musl/tree/src/math/scalbn.c?h=v1.1.16
*/
#endregion

namespace ArgusLib
{
	public static partial class Functions
	{
		/// <summary>
		/// Scales significand using base 2 exponent
		/// </summary>
		/// <returns><paramref name="x"/> * 2 ^ <paramref name="n"/></returns>
		public static double ScaleBinary(double x, int n)
		{
			if (n > 1023)
			{
				x *= Constants.Pow2P1023;
				n -= 1023;
				if (n > 1023)
				{
					x *= Constants.Pow2P1023;
					n -= 1023;
					if (n > 1023)
						n = 1023;
				}
			}
			else if (n < -1022)
			{
				x *= Constants.Pow2M1022;
				n += 1022;
				if (n < -1022)
				{
					x *= Constants.Pow2M1022;
					n += 1022;
					if (n < -1022)
						n = -1022;
				}
			}
			x *= BitConverterEx.UInt64BitsToDouble((ulong)(0x3ff + n) << 52);
			return x;
		}
	}
}
