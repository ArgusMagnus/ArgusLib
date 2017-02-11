#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib
{
	public static partial class Functions
	{
		public static double Gaussian(double x, double mean = 0.0, double variance = 1.0)
		{
			if (variance < 0)
				throw new ArgumentException("Variance must be non-negative.", nameof(variance));

			double sc = Constants.SqrtTwoPi_R;
			if (variance != 1)
				sc /= Math.Sqrt(variance);

			double x1 = x - mean;
			return sc * Math.Exp(-0.5 * x1 * x1 / variance);
		}

		public static Func<double, double, double, double> GetDerivativeOfGaussian(int order)
		{
			if (order < 0)
				throw new ArgumentException("Order must be non-negative.", nameof(order));
			if (order == 0)
				return Functions.Gaussian;

			double[] a = GetDoGCoefficients(order);
			int iStart = order.IsEven() ? 0 : 1;
			return (x, mean, variance) =>
			{
				double sum = 0.0;
				double xm = x - mean;
				for (int i = iStart; i <= order; i += 2)
					sum += a[i] * Math.Pow(xm, i) * Math.Pow(variance, -(order + i) / 2);
				return sum * Functions.Gaussian(x, mean, variance);
			};
		}

		/// <summary>
		/// Returns the coefficients a[i] for the n-th order derivative of a gaussian where
		/// d^n/dx^2 f(x) = sum(i=0..n, a[i]*xm^i/var^k * f(x)) where xm = (x - mean),
		/// k = floor((n+i)/2)
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public static double[] GetDoGCoefficients(int order)
		{
			if (order < 0)
				throw new ArgumentException("Order must be non-negative.", nameof(order));

			if (order == 0)
				return new double[] { 1.0 };
			if (order == 1)
				return new double[] { 0.0, -1.0 };
			if (order == 2)
				return new double[] { -1.0, 0.0, 1.0 };
			if (order == 3)
				return new double[] { 0.0, 3.0, 0.0, -1.0 };
			if (order == 4)
				return new double[] { 3.0, 0.0, -6.0, 0.0, 1.0 };

			double[] result = new double[order + 1];
			// n == 4
			result[0] = 3.0;
			result[2] = 6.0;
			result[4] = 1.0;
			for (int n = 5; n <= order; n++)
			{
				result[n] = 1.0;
				if (n.IsEven())
				{
					result[0] = result[1];
					for (int i = 2; i < n; i += 2)
						result[i] = result[i - 1] + (i + 1) * result[i + 1];
				}
				else
				{
					for (int i = 1; i < n; i += 2)
						result[i] = result[i - 1] + (i + 1) * result[i + 1];
				}
			}
			int start;
			if (order.IsEven())
			{
				for (int i = 1; i < order; i += 2)
					result[i] = 0.0;
				start = (order >> 1).IsEven() ? 2 : 0;
			}
			else
			{
				for (int i = 0; i < order; i += 2)
					result[i] = 0.0;
				start = (order >> 1).IsEven() ? 1 : 3;
			}
			for (int i = start; i < result.Length; i += 4)
				result[i] = -result[i];
			return result;
		}

		public static double CdfOfGaussian(double x, double mean = 0.0, double variance = 1.0) => 0.5 * (1.0 + Functions.Erf((x - mean) / (Math.Sqrt(variance) * Constants.Sqrt2)));
	}
}
