using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;
using ArgusLib.Resources;

namespace ArgusLib.Random
{
	public sealed class UniformDoubleDistribution : Distribution
	{
		readonly double _fromInclusive;
		readonly double _scale;

		public double FromInclusive => _fromInclusive;
		public double ToInclusive => _fromInclusive + (_scale * ulong.MaxValue);

		public UniformDoubleDistribution(double fromInclusiv = 0, double toInclusive = 1, IUniformPRNG prng = null)
			: base(prng)
		{
			if (double.IsNaN(fromInclusiv))
				throw Tracer.ThrowError<UniformDoubleDistribution>(new ArgumentException(Exceptions.Argument_NotNaN, nameof(fromInclusiv)));
			if (double.IsNaN(toInclusive))
				throw Tracer.ThrowError<UniformDoubleDistribution>(new ArgumentException(Exceptions.Argument_NotNaN, nameof(toInclusive)));
			if (double.IsInfinity(fromInclusiv))
				throw Tracer.ThrowError<UniformDoubleDistribution>(new ArgumentOutOfRangeException(nameof(fromInclusiv), Exceptions.ArgumentOutOfRange_MustBeFinite));
			if (double.IsInfinity(toInclusive))
				throw Tracer.ThrowError<UniformDoubleDistribution>(new ArgumentOutOfRangeException(nameof(toInclusive), Exceptions.ArgumentOutOfRange_MustBeFinite));
			if (fromInclusiv >= toInclusive)
				throw Tracer.ThrowError<UniformDoubleDistribution>(new ArgumentOutOfRangeException(nameof(fromInclusiv), string.Format(Exceptions.ArgumentOutOfRange_MustBeSmallerThan, $"'{nameof(toInclusive)}' (= {toInclusive}")));

			_fromInclusive = fromInclusiv;
			_scale = (toInclusive - fromInclusiv) / ulong.MaxValue;
		}

		public double Next() => PRNG.NextUInt64() * _scale + _fromInclusive;
	}
}
