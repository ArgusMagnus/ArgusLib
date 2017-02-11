using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;
using ArgusLib.Resources;

namespace ArgusLib.Random
{
	/// <summary>
	/// Currently uses the naive approach, should probably use approximations for larger n
	/// </summary>
	public sealed class BinomialDistribution :Distribution
	{
		readonly uint _n;
		readonly double _p;
		readonly double[] _cfd;

		public int N => unchecked((int)_n);
		public double SuccessProbability => _p;

		public BinomialDistribution(int n, double p = 0.5, IUniformPRNG prng = null)
			:base(prng)
		{
			if (n < 1)
				throw Tracer.ThrowError<BinomialDistribution>(new ArgumentOutOfRangeException(nameof(n), string.Format(Exceptions.ArgumentOutOfRange_MustBeGreaterThan, 0)));
			if (p < 0.0 || p > 1.0)
				throw Tracer.ThrowError<BinomialDistribution>(new ArgumentOutOfRangeException(nameof(p), string.Format(Exceptions.ArgumentOutOfRange_MustBeInRange, "[0, 1]")));

			const int NWarningThreshold = 512;
			if (n > NWarningThreshold)
				Tracer.WriteWarning<BinomialDistribution>($"This class is not designed to handle large {nameof(n)} (= {n}). It may use a lot of memory (~ {nameof(n)} * 8 bytes) and be slow.");

			_n = (uint)n;
			_p = p;
			_cfd = new double[_n];
			InitializeCDF();
		}

		double GetBinomialCoefficient(uint k, double kMinus1)
		{
			if (k >= _n)
				return 1.0;
			if (k <= 0u)
				return 1.0;

			return (double)(_n + 1u - k) / k * kMinus1;
		}

		double GetPTimesQ(uint k, double kMinus1, double Q_p_q)
		{
			if (k <= 0u)
				return Math.Pow((1.0 - _p), _n);
			return kMinus1 * Q_p_q;
		}

		void InitializeCDF()
		{
			double Q_p_q = _p / (1.0 - _p);
			double b = 0;
			double pq = 0;
			double cdf = 0;
			for (uint i = 0; i < _cfd.Length; i++)
			{
				b = GetBinomialCoefficient(i, b);
				pq = GetPTimesQ(i, pq, Q_p_q);
				cdf += b * pq;
				_cfd[i] = cdf;
			}
		}

		public int Next()
		{
			double u = PRNG.NextDouble();
			int index = Array.BinarySearch(_cfd, u);
			if (index < 0)
				index = ~index;
			return index;
		}
	}
}
