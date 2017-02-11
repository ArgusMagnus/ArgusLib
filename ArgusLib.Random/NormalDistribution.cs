using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Random
{
	/// <summary>
	/// Pseudo-random numbers generator which generates numbers from a normal (gaussian) distribution using the Box-Muller-Transform.
	/// </summary>
	public sealed class NormalDistribution : Distribution
	{
		readonly double _mean;
		readonly double _stdDev;
		double _cachedValue;
		bool _isCached = false;

		public double Mean => _mean;
		public double StandardDeviation => _stdDev;
		public double Variance => _stdDev * _stdDev;

		public NormalDistribution(double mean = 0, double variance = 1, IUniformPRNG prng = null)
			:base(prng)
		{
			_mean = mean;
			_stdDev = Math.Sqrt(variance);
		}

		/// <summary>
		/// Returns a pseudo-random variable from a normal (gaussian) distribution using the Box-Muller-Transform.
		/// </summary>
		public double Next()
		{
			// Box-Muller Transform:
			// https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform

			if (_isCached)
			{
				_isCached = false;
				return _cachedValue;
			}

			// Polar form
			double u, v, s;
			do
			{
				u = 2.0 * PRNG.NextDouble() - 1.0;
				v = 2.0 * PRNG.NextDouble() - 1.0;
				s = u * u + v * v;
			} while (s == 0.0 || s >= 1.0);

			double factor = Math.Sqrt(-2.0 * Math.Log(s) / s);
			double z1 = u * factor;
			double z2 = v * factor;

			// Basic method
			//double u1 = _uniformGen.NextDouble();
			//double u2 = _uniformGen.NextDouble();
			//double R = Math.Sqrt(-2.0 * Math.Log(u1));
			//double phi = ConstantsD.TwoPi * u2;
			//double z1 = R * Math.Cos(phi);
			//double z2 = R * Math.Sin(phi);

			_cachedValue = _mean + _stdDev * z1;
			_isCached = true;
			return _mean + _stdDev * z2;
		}
	}
}
