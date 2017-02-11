using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace ArgusLib.Random
{
	/// <summary>
	/// WELL512 Pseudorandom Number Generator
	/// </summary>
	public sealed class Well512PRNG : IUniformPRNG
	{
		readonly uint[] _state = new uint[16];
		uint _index = 0;

		/// <summary>
		/// Instantiates a <see cref="Well512PRNG"/> with default seeds acquired with <see cref="UniformPRNG.GetAdditionalSeedsUInt32(object)"/>.
		/// </summary>
		public Well512PRNG()
			: this(null) { }

		/// <summary>
		/// Instantiates a <see cref="Well512PRNG"/> with the first 16 non-zero elements of <paramref name="seed"/>.
		/// If <paramref name="seed"/> is <c>null</c> or has less than 16 non-zero elements, additional values
		/// acquired with <see cref="UniformPRNG.GetAdditionalSeedsUInt32(object)"/> are used.
		/// </summary>
		public Well512PRNG(params int[] seed)
		{
			int i = 0;
			if (seed != null && seed.Any((s) => s != 0))
			{
				unchecked
				{
					foreach (int s in seed)
					{
						if (i >= _state.Length)
							break;
						if (s == 0)
							continue;
						_state[i++] = (uint)s;
					}
				}
			}
			foreach (uint addSeed in UniformPRNG.GetAdditionalSeedsUInt32(_state, _state.Length - i))
			{
				if (i >= _state.Length)
					break;
				_state[i++] = addSeed;
			}
		}

		//static uint MAT0POS(int t, uint v) => (v ^ (v >> t));
		//static uint MAT0NEG(int t, uint v) => (v ^ (v << (-(t))));
		//static uint MAT3NEG(int t, uint v) => (v << (-(t)));
		//static uint MAT4NEG(int t, uint b, uint v) => (v ^ ((v << (-(t))) & b));

		/// <license>
		/// This method was adapted to C# from Chris Lomont's public domain code:
		/// http://lomont.org/Math/Papers/2008/Lomont_PRNG_2008.pdf
		/// </license>
		public uint NextUInt32()
		{
			//const int M1 = 13;
			//const int M2 = 9;
			//const int M3 = 5;

			//uint z0 = _state[(_index + 15) & 0x0000000fU];
			//uint z1 = MAT0NEG(-16, _state[_index]) ^ MAT0NEG(-15, _state[(_index + M1) & 0x0000000fU]);
			//uint z2 = MAT0POS(11, _state[(_index + M2) & 0x0000000fU]);
			//_state[_index] = z1 ^ z2;
			//_state[(_index + 15) & 0x0000000fU] = MAT0NEG(-2, z0) ^ MAT0NEG(-18, z1) ^ MAT3NEG(-28, z2) ^ MAT4NEG(-5, 0xDA442D24u, _state[_index]);
			//_index = (_index + 15) & 0x0000000fU;
			//return _state[_index];

			uint a = _state[_index];
			uint c = _state[(_index + 13u) & 15u];
			uint b = a ^ c ^ (a << 16) ^ (c << 15);
			c = _state[(_index + 9u) & 15u];
			c ^= (c >> 11);
			a = _state[_index] = b ^ c;
			uint d = a ^ ((a << 5) & 0xDA442D24u);
			_index = (_index + 15u) & 15u;
			a = _state[_index];
			_state[_index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);
			return _state[_index];
		}

		public ulong NextUInt64() => BitConverterEx.UInt32BitsToUInt64(NextUInt32(), NextUInt32());

		bool IUniformPRNG.Supports32Bit => true;
	}
}
