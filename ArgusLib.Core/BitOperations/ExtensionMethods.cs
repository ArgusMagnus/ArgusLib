#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ArgusLib.BitOperations
{
	//public static class ExtensionMethods
	//{
	//	public static BigInteger GetSignificant(this decimal d)
	//	{
	//		int[] bits = decimal.GetBits(d);
	//		int exp = (bits[3] >> 16) & 0xFF;
	//		byte[] num = new byte[12];
	//		Buffer.BlockCopy(bits, 0, num, 0, num.Length);
	//		BigInteger significand = new BigInteger(num);
	//		if (d < 0m)
	//			significand *= BigInteger.MinusOne;
	//		return significand;
	//	}

	//	public static int GetExponent(this decimal d)
	//	{
	//		int[] bits = decimal.GetBits(d);
	//		int exp = (bits[3] >> 16) & 0xFF;
	//		return exp;
	//	}
	//}
}
