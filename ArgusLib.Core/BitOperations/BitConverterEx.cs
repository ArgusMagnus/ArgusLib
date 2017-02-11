#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;

namespace ArgusLib
{
	public static partial class BitConverterEx
	{
		public static unsafe int SingleToInt32Bits(float val) => *(int*)&val;
		public static unsafe float Int32BitsToSingle(int bits) => *(float*)&bits;
		public static unsafe uint SingleToUInt32Bits(float val) => *(uint*)&val;
		public static unsafe float UInt32BitsToSingle(uint bits) => *(float*)&bits;
		public static unsafe ulong DoubleToUInt64Bits(double val) => *(ulong*)&val;
		public static unsafe double UInt64BitsToDouble(ulong bits) => *(double*)&bits;

		public static unsafe void UInt64BitsToUInt32(ulong bits, out uint lowWord, out uint highWord)
		{
			uint* ptr = (uint*)&bits;
			lowWord = ptr[0];
			highWord = ptr[1];
		}

		public static unsafe ulong UInt32BitsToUInt64(uint lowWord, uint highWord)
		{
			ulong val = 0ul;
			uint* ptr = (uint*)&val;
			ptr[0] = lowWord;
			ptr[1] = highWord;
			return val;
		}
	}
}
