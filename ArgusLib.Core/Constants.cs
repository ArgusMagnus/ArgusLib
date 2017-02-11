#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

namespace ArgusLib
{
	/// <summary>
	/// Some often used constants
	/// </summary>
	/// <remarks>
	/// Naming:
	/// - Fields postfixed with an "_R" contain the reciprocal (1 divided by) value.
	/// - Fields with a name of the form "Q_a_b" denote the quotient of a and b (a / b).
	/// </remarks>
	public static partial class Constants
	{
		/// <summary>
		/// Smallest such that 1.0 + <see cref="MaschineEpsilon"/> !=1.0
		/// </summary>
		public const double MaschineEpsilon = 1.0 / (1UL << 52);

		public const double Pow2P32 = 1UL << 32;

		public const double Pow2P64 = Pow2P32 * Pow2P32;

		public const double Pow2P128 = Pow2P64 * Pow2P64;

		public const double Pow2P256 = Pow2P128 * Pow2P128;

		public const double Pow2P512 = Pow2P256 * Pow2P256;

		public const double Pow2P992 = Pow2P512 * Pow2P256 * Pow2P128 * Pow2P64 * Pow2P32;

		public const double Pow2P1023 = Pow2P992 * (1UL << 31);

		public const double Pow2M1022 = 1.0 / (Pow2P992 * (1UL << 30));
	}
}
