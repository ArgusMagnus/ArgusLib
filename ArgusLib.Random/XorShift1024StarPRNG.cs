using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib.Random
{
	public sealed class XorShift1024StarPRNG : IUniformPRNG
	{
		readonly ulong[] _state = new ulong[16];
		int _index = 0;

		/// <summary>
		/// Instantiates a <see cref="XorShift1024StarPRNG"/> with the first 16 non-zero elements of <paramref name="seeds"/>.
		/// If <paramref name="seeds"/> is <c>null</c> or has less than 16 non-zero elements, additional values
		/// acquired with <see cref="UniformPRNG.GetAdditionalSeedsUInt64(object)"/> are used.
		/// </summary>
		public XorShift1024StarPRNG(params long[] seeds)
		{
			int i = 0;
			if (seeds != null && seeds.Any((s) => s != 0L))
			{
				unchecked
				{
					foreach (long s in seeds)
					{
						if (i >= _state.Length)
							break;
						if (s == 0L)
							continue;
						_state[i++] = (ulong)s;
					}
				}
			}
			foreach (ulong addSeed in UniformPRNG.GetAdditionalSeedsUInt64(_state, _state.Length - i))
			{
				if (i >= _state.Length)
					break;
				_state[i++] = addSeed;
			}
		}

		public ulong NextUInt64()
		{
			unchecked
			{
				ulong s0 = _state[_index];
				_index = (_index + 1) & 15;
				ulong s1 = _state[_index];
				s1 ^= s1 << 31; // a
				_state[_index] = s1 ^ s0 ^ (s1 >> 11) ^ (s0 >> 30); // b, c
				return _state[_index] * 1181783497276652981ul;
			}
		}

		bool IUniformPRNG.Supports32Bit => false;
		uint IUniformPRNG.NextUInt32() { throw new NotSupportedException(); }
	}
}
