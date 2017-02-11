using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Random
{
	public class PrbsGenerator
	{
		public const ulong PRBS31 = (1ul << 31) | (1ul << 28) | 1ul;
		public const ulong PRBS63 = (1ul << 63) | (1ul << 62) | 1ul;
		const ulong DefaultPolynomial = PRBS63;

		ulong _state;
		readonly ulong _poly;

		public PrbsGenerator(ulong polynomial = DefaultPolynomial)
			: this((unchecked((ulong)System.Environment.TickCount)), polynomial) { }

		public PrbsGenerator(long seed, ulong polynomial = DefaultPolynomial)
			: this((unchecked((ulong)seed)), polynomial) { }

		PrbsGenerator(ulong seed, ulong polynomial)
		{
			if (seed == 0)
				throw Tracer.ThrowCritical<PrbsGenerator>(new ArgumentException(Exceptions.PrbsGenerator_SeedMustNotBeZero, nameof(seed)));
			if ((polynomial & 1ul) != 1ul)
				throw Tracer.ThrowCritical<PrbsGenerator>(new ArgumentException(string.Format(Exceptions.PrbsGenerator_InvalidPolynomial, polynomial.ToString("X")), nameof(polynomial)));
			_state = seed;
			_poly = polynomial;
		}

		public bool NextBit()
		{
			bool result = (_state & 1ul) == 1ul;
			_state >>= 1;
			if (result)
				_state ^= _poly;
			return result;
		}

		public uint Next32Bits()
		{
			uint val = 0u;
			for (int i = 0; i < 32; i++)
			{
				val <<= 1;
				if (NextBit())
					val |= 1u;
			}
			return val;
		}

		public ulong Next64Bits()
		{
			ulong val = 0ul;
			for (int i = 0; i < 64; i++)
			{
				val <<= 1;
				if (NextBit())
					val |= 1ul;
			}
			return val;
		}
	}
}
