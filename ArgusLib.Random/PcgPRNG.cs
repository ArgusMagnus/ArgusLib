using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib.Random
{
	/// <summary>
	/// 64bit PCG pseudorandom number generator.
	/// </summary>
	/// <remarks>
	/// Period: 2^64
	/// </remarks>
	/// <license>
	/// (c) 2016 Tobias Meyer (argusmagnus@outlook.com)
	/// Licensed under Apache License 2.0
	/// 
	/// Adapted to C# from the minimal C implementation
	/// by M.E. O'Neill (pcg-random.org)
	/// </license>
	public sealed class PcgPRNG : IUniformPRNG
	{
		readonly ulong _inc1;
		readonly ulong _inc2;
		ulong _state1;
		ulong _state2;

		/// <summary>
		/// Instantiates a <see cref="PcgPRNG"/>. If any of the arguments is 0 or otherwise invalid, its value is
		/// replaced with a value acquired with <see cref="UniformPRNG.GetAdditionalSeedsUInt64(object)"/>.
		/// </summary>
		public PcgPRNG(ulong seed1 = 0, ulong seed2 = 0, ulong sequence1 = 0, ulong sequence2 = 0)
		{
			const ulong seqMask = ~0ul >> 1;

			var seeds = UniformPRNG.GetAdditionalSeedsUInt64(this, 4).GetEnumerator();
			while (seed1 == 0ul && seeds.MoveNext())
				seed1 = seeds.Current;
			while ((seed2 == 0ul || seed2 == seed1) && seeds.MoveNext())
				seed2 = seeds.Current;
			while ((sequence1 & seqMask) == 0ul && seeds.MoveNext())
				sequence1 = seeds.Current;
			while (((sequence2 & seqMask) == 0ul || (sequence2 & seqMask) == (sequence1 & seqMask)) && seeds.MoveNext())
				sequence2 = seeds.Current;

			_state1 = 0ul;
			_state2 = 0ul;
			_inc1 = (sequence1 << 1) | 1ul;
			_inc2 = (sequence2 << 1) | 1ul;
			NextUInt32Internal(ref _state1, _inc1);
			NextUInt32Internal(ref _state2, _inc2);
			unchecked
			{
				_state1 += sequence1;
				_state2 += sequence2;
			}
			NextUInt32Internal(ref _state1, _inc1);
			NextUInt32Internal(ref _state2, _inc2);
		}

		static uint NextUInt32Internal(ref ulong state, ulong inc)
		{
			unchecked
			{
				ulong oldState = state;
				state = oldState * 6364136223846793005ul + inc;
				uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
				int rot = (int)(uint)(oldState >> 59);
				return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
			}
		}

		public uint NextUInt32() => NextUInt32Internal(ref _state1, _inc1);

		public ulong NextUInt64() => BitConverterEx.UInt32BitsToUInt64(NextUInt32Internal(ref _state1, _inc1), NextUInt32Internal(ref _state2, _inc2));

		public int NextInt32() => unchecked((int)NextUInt32());
		public long NextInt64() => unchecked((long)NextUInt64());

		bool IUniformPRNG.Supports32Bit => true;
	}
}
