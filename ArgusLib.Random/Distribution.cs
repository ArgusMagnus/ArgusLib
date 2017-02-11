using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib.Random
{
	public abstract class Distribution
	{
		readonly IUniformPRNG _prng;
		protected IUniformPRNG PRNG => _prng;

		protected Distribution(IUniformPRNG prng)
		{
			_prng = prng ?? UniformPRNG.GetDefault();
		}
	}
}
