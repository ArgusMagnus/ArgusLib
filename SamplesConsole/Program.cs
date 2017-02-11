using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib;

namespace SamplesConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			double x = Constants.Pow2P1023;
			double y = Constants.Pow2M1022;
			Console.WriteLine(Constants.Pow2P1023);
			Console.WriteLine(x);
			Console.WriteLine(Constants.Pow2P1023);
			Console.WriteLine(y);
			Console.ReadKey();
		}
	}
}
