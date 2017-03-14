using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Random
{
	public sealed class BernoulliDistribution :Distribution
	{
		readonly double _p;
		public double SuccessProbability => _p;

		public BernoulliDistribution(double p = 0.5, IUniformPRNG prng = null)
			:base(prng)
		{
			if (p < 0.0 || p > 1.0)
				throw Tracer.ThrowError<BernoulliDistribution>(new ArgumentOutOfRangeException(nameof(p), string.Format(Exceptions.ArgumentOutOfRange_MustBeInRange, "[0, 1]")));

			_p = p;
		}

		public int Next() => PRNG.NextDouble() < _p ? 1 : 0;
	}
}
