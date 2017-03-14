using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;
using System.Security.Cryptography;
using DefaultPRNG = ArgusLib.Random.PcgPRNG;

namespace ArgusLib.Random
{
	/// <summary>
	/// Interface for a pseudo random number generator capable of generating
	/// uniformly distributed pseudo random numbers in the range [0, <see cref="ulong.MaxValue"/>].
	/// </summary>
	public interface IUniformPRNG
	{
		/// <summary>
		/// Implementors must return a uniformly distributed pseudo random number in the range [0, <see cref="ulong.MaxValue"/>].
		/// </summary>
		ulong NextUInt64();

		/// <summary>
		/// Implementors must return a value indicating wheter the <see cref="NextUInt32"/> method is supported.
		/// </summary>
		bool Supports32Bit { get; }

		/// <summary>
		/// Implementors must return a uniformly distributed pseudo random number in the range [0, <see cref="uint.MaxValue"/>]
		/// or throw a <see cref="NotSupportedException"/> if <see cref="Supports32Bit"/> returns <c>false</c>.
		/// </summary>
		uint NextUInt32();
	}

	public static class UniformPRNG
	{
		public static IUniformPRNG GetDefault() => new DefaultPRNG();

		public static double NextDouble<TUniformPRNG>(this TUniformPRNG prng) where TUniformPRNG : IUniformPRNG
		{
			// return Numerics.Functions.Ldexp(prng.NextUInt64(), -64);
			return prng.NextUInt64() * (1.0 / ulong.MaxValue);
		}

		public static Normalized NextNormalized<TUniformPRNG>(this TUniformPRNG prng) where TUniformPRNG : IUniformPRNG
		{
			if (prng.Supports32Bit)
				return new Normalized(prng.NextUInt32());
			else
				return new Normalized((uint)(prng.NextUInt64() >> 32));
		}

		static PcgPRNG GetFallbackPRNG(object seedObj)
		{
			ulong seed1 = (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
			ulong seq1 = (uint)GC.CollectionCount(0);
			if (seedObj != null)
				seq1 |= (ulong)(uint)seedObj.GetHashCode() << 32;
			ulong seed2 = (ulong)GC.GetTotalMemory(false);
			ulong seq2 = (uint)System.Environment.TickCount;
			return new PcgPRNG(seed1, seed2, seq1, seq2);
		}

		/// <summary>
		/// Returns additional seed values for pseudorandom number generators.
		/// If available (<see cref="CryptoPRNG.IsAvailable"/> is <c>true</c>) uses a cryptographically secure
		/// PRNG returned by <see cref="CryptoPRNG.GetInstance(int)"/> to generate the additional seed values,
		/// otherwise returns pseudorandom numbers generated with a PRNG seeded with values
		/// which are highly unlikely to ever be the same (a combination of high-resolution timestamp,
		/// time since system start, memory adress of <paramref name="seedObj"/>, total used heap memory and
		/// number of GC runs.
		/// </summary>
		public static IEnumerable<uint> GetAdditionalSeedsUInt32(object seedObj, int maxSeeds)
		{
			unchecked
			{
				using (CryptoPRNG rng = CryptoPRNG.Create(maxSeeds * sizeof(uint)))
				{
					if (rng != null)
					{
						while (true)
							yield return rng.NextUInt32();
					}
					else
					{
						var prng = GetFallbackPRNG(seedObj);
						while (true)
							yield return prng.NextUInt32();
					}
				}
			}
		}

		/// <summary>
		/// Returns additional seed values for pseudorandom number generators.
		/// If available (<see cref="CryptoPRNG.IsAvailable"/> is <c>true</c>) uses a cryptographically secure
		/// PRNG returned by <see cref="CryptoPRNG.GetInstance(int)"/> to generate the additional seed values,
		/// otherwise returns pseudorandom numbers generated with <see cref="PcgPRNG"/> seeded with values
		/// which are highly unlikely to ever be the same (a combination of high-resolution timestamp,
		/// time since system start, memory adress of <paramref name="seedObj"/>, total used heap memory and
		/// number of GC runs.
		/// </summary>
		public static IEnumerable<ulong> GetAdditionalSeedsUInt64(object seedObj, int maxSeeds)
		{
			unchecked
			{
				using (CryptoPRNG rng = CryptoPRNG.Create(maxSeeds * sizeof(ulong)))
				{
					if (rng != null)
					{
						while (true)
							yield return rng.NextUInt64();
					}
					else
					{
						var prng = GetFallbackPRNG(seedObj);
						while (true)
							yield return prng.NextUInt64();
					}
				}
			}
		}
	}
}
