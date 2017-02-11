using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib.Random
{
	public sealed class XorShift128PlusPRNG : IUniformPRNG
	{
		ulong _state0;
		ulong _state1;

		/// <summary>
		/// Instantiates a <see cref="XorShift128PlusPRNG"/>. If any of the arguments is 0 or otherwise invalid, its value is
		/// replaced with a value acquired with <see cref="UniformPRNG.GetAdditionalSeedsUInt64(object)"/>.
		/// </summary>
		public XorShift128PlusPRNG(ulong seed1 = 0, ulong seed2 = 0)
		{
			var seeds = UniformPRNG.GetAdditionalSeedsUInt64(this, 2).GetEnumerator();
			while (seed1 == 0ul && seeds.MoveNext())
				seed1 = seeds.Current;
			while (seed2 == 0ul && seeds.MoveNext())
				seed2 = seeds.Current;

			_state0 = seed1;
			_state1 = seed2;
		}

		public ulong NextUInt64()
		{
			ulong x = _state0;
			ulong y = _state1;
			_state0 = y;
			x ^= x << 23; // a
			_state1 = x ^ y ^ (x >> 17) ^ (y >> 26); // b, c
			return _state1 + y;
		}

		bool IUniformPRNG.Supports32Bit => false;
		uint IUniformPRNG.NextUInt32() { throw new NotSupportedException(); }
	}
}
