using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Random
{
	public abstract class InverseCdfDistribution : Distribution
	{
		protected internal InverseCdfDistribution(IUniformPRNG prng = null)
		: base(prng) { }

		protected internal abstract double InverseCdf(double u);

		public double Next() => InverseCdf(PRNG.NextDouble());
	}

	public sealed class CauchyDistribution : InverseCdfDistribution
	{
		readonly double _x0;
		readonly double _gamma;

		public double X0 => _x0;
		public double Gamma => _gamma;

		public CauchyDistribution(double x0 = 0, double gamma = 1, IUniformPRNG prng = null)
			:base(prng)
		{
			_x0 = x0;
			_gamma = gamma;
		}

		protected internal override double InverseCdf(double u)
		{
			return _x0 + _gamma * Math.Tan(Math.PI * (u - 0.5));
		}
	}
}
