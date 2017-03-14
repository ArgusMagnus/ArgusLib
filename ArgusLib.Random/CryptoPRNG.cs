using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;
using System.Security.Cryptography;

namespace ArgusLib.Random
{
	/// <summary>
	/// Cryptographically secure pseudorandom number generator.
	/// </summary>
	public sealed class CryptoPRNG : IDisposable, IUniformPRNG
	{
		readonly RandomNumberGenerator _rng;

		bool IUniformPRNG.Supports32Bit => true;

		CryptoPRNG(RandomNumberGenerator rng, int bufferSize)
		{
			_rng = rng ?? throw new ArgumentNullException(nameof(rng));

			if (bufferSize < sizeof(long))
				throw Tracer.ThrowError<CryptoPRNG>(new ArgumentOutOfRangeException(nameof(bufferSize), string.Format(Exceptions.ArgumentOutOfRange_MustBeGreaterThan, sizeof(long) - 1)));

			_buffer = new byte[bufferSize];
		}

		public static CryptoPRNG Create(int bufferSize)
		{
			RandomNumberGenerator rng = ExceptionUtil.Try(() => RandomNumberGenerator.Create());
			if (rng == null)
				return null;
			if (bufferSize < sizeof(long))
				throw Tracer.ThrowError<CryptoPRNG>(new ArgumentOutOfRangeException(nameof(bufferSize), string.Format(Exceptions.ArgumentOutOfRange_MustBeGreaterThan, sizeof(long) - 1)));
			return new CryptoPRNG(rng, bufferSize);
		}

		readonly byte[] _buffer;
		int _index = 0;

		public void GetBytes(byte[] buffer) => _rng.GetBytes(buffer);
		public void Dispose() => _rng.Dispose();

		int IncrementIndex(int requestedBytes)
		{
			if (_index + requestedBytes > _buffer.Length)
			{
				_rng.GetBytes(_buffer);
				_index = 0;
			}
			int retVal = _index;
			_index += requestedBytes;
			return retVal;
		}

		public byte NextByte() => _buffer[IncrementIndex(sizeof(byte))];

		public sbyte NextSByte() => unchecked((sbyte)_buffer[IncrementIndex(sizeof(sbyte))]);

		public short NextInt16() => BitConverter.ToInt16(_buffer, IncrementIndex(sizeof(short)));

		public ushort NextUInt16() => BitConverter.ToUInt16(_buffer, IncrementIndex(sizeof(ushort)));

		public int NextInt32() => BitConverter.ToInt32(_buffer, IncrementIndex(sizeof(int)));

		public uint NextUInt32() => BitConverter.ToUInt32(_buffer, IncrementIndex(sizeof(uint)));

		public long NextInt64() => BitConverter.ToInt64(_buffer, IncrementIndex(sizeof(long)));

		public ulong NextUInt64() => BitConverter.ToUInt64(_buffer, IncrementIndex(sizeof(ulong)));
	}
}
