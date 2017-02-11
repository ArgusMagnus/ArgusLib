#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System.Runtime.InteropServices;

namespace ArgusLib
{
	public static partial class Functions
	{
		/// <summary>
		/// This evil mutable struct with public fields is intended only for
		/// bit manipulation of doubles in the <see cref="Functions"/> class
		/// method implementations.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		struct Ieee754Double
		{
			const ulong SignificandMask = ~(ulong.MaxValue << 52);
			const ulong SignMask = 1UL << 63;
			const ulong ExponentMask = ~(ulong.MaxValue << 11) << 52;
			public const int ExponentBias = 1023;

			[FieldOffset(0)]
			public double Value;

			[FieldOffset(0)]
			public ulong Bits;

			[FieldOffset(0)]
			public uint LowWord;

			[FieldOffset(sizeof(uint))]
			public uint HighWord;

			public Ieee754Double(int sign, ulong significand, ushort exponent)
				: this()
			{
				Bits = 0UL;
				if (sign < 0)
					Bits |= SignMask;
				ulong exp = (ulong)exponent;
				Bits |= (significand & SignificandMask);
				Bits |= (exp << 52) & ExponentMask;
			}

			public int Sign
			{
				get { return SignBit ? -1 : 1; }
			}

			public bool SignBit
			{
				get { return (Bits & SignMask) == SignMask; }
				set
				{
					if (value)
						Integer<ulong>.SetFlag(Bits, SignMask);
					else
						Integer<ulong>.RemoveFlag(Bits, SignMask);
				}
			}

			public ulong Significand
			{
				get { return ((Bits & SignificandMask)); }
				set { Bits = (Bits & ~SignificandMask) | (value & SignificandMask); }
			}

			public ushort Exponent
			{
				get { return (ushort)((Bits & ExponentMask) >> 52); }
				set { Bits = (Bits & ~ExponentMask) | (((ulong)value << 52) & ExponentMask); }
			}

			#region C++ Constants

			/// <summary>
			/// Number of decimal digits of precision
			/// </summary>
			public const int DBL_DIG = 15;

			/// <summary>
			/// Smallest such that 1.0 + <see cref="DBL_EPSILON"/> !=1.0
			/// </summary>
			public const double DBL_EPSILON = Constants.MaschineEpsilon;

			/// <summary>
			/// Number of bits in mantissa
			/// </summary>
			public const int DBL_MANT_DIG = 53;

			/// <summary>
			/// Maximum value
			/// </summary>
			public const double DBL_MAX = double.MaxValue;

			/// <summary>
			/// Maximum decimal exponent
			/// </summary>
			public const int DBL_MAX_10_EXP = 308;

			/// <summary>
			/// Maximum binary exponent
			/// </summary>
			public const int DBL_MAX_EXP = 1024;

			/// <summary>
			/// Minimum positive value
			/// </summary>
			public const double DBL_MIN = 2.2250738585072013830902327173324e-308; // 2 ** -1022;

			/// <summary>
			/// Minimum decimal exponent
			/// </summary>
			public const int DBL_MIN_10_EXP = -307;

			/// <summary>
			/// Minimum binary exponent
			/// </summary>
			public const int DBL_MIN_EXP = -1021;

			#endregion
		}
	}
}
