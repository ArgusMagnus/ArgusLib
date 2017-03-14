using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Random
{
	public sealed class UniformInt32Distribution : Distribution
	{
		readonly int _fromInclusive;
		readonly uint _range;
		readonly uint _cutOff;
		int _cachedValue;
		bool _isCached;

		public int FromInclusive => _fromInclusive;
		public int ToInclusive => (int)(_fromInclusive + unchecked(_range - 1u));

		public UniformInt32Distribution(int fromInclusive = 0, int toInclusive = int.MaxValue, IUniformPRNG prng = null)
			:base(prng)
		{
			if (fromInclusive >= toInclusive)
				throw Tracer.ThrowError<UniformInt32Distribution>(new ArgumentOutOfRangeException(nameof(fromInclusive), string.Format(Exceptions.ArgumentOutOfRange_MustBeSmallerThan, $"{nameof(toInclusive)} (={toInclusive})")));

			_fromInclusive = fromInclusive;
			_range = unchecked((uint)(toInclusive - fromInclusive) + 1u);
			_cutOff = _range == 0u ? 0u : (uint.MaxValue / _range) * _range;
		}

		public int Next()
		{
			unchecked
			{
				if (_isCached)
				{
					_isCached = false;
					return _cachedValue;
				}

				uint lw, hw;
				BitConverterEx.UInt64BitsToUInt32(PRNG.NextUInt64(), out lw, out hw);
				if (_range == 0u)
				{
					_isCached = true;
					_cachedValue = (int)((lw % _range) + _fromInclusive);
					return (int)((hw % _range) + _fromInclusive);
				}
				else if (_range.IsPowerOfTwo())
				{
					_isCached = true;
					_cachedValue = (int)(uint)((_range * (ulong)lw) >> 32);
					return (int)(uint)((_range * (ulong)hw) >> 32);
				}

				while (lw >= _cutOff && hw >= _cutOff)
					BitConverterEx.UInt64BitsToUInt32(PRNG.NextUInt64(), out lw, out hw);

				if (lw < _cutOff && hw < _cutOff)
				{
					_isCached = true;
					_cachedValue = (int)((lw % _range) + _fromInclusive);
					return (int)((hw % _range) + _fromInclusive);
				}
				else if (lw < _cutOff)
					return (int)((lw % _range) + _fromInclusive);
				else // if (rword < _cutOff)
					return (int)((hw % _range) + _fromInclusive);
			}
		}
	}
}
