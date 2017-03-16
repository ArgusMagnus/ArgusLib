using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using ArgusLib;
using ArgusLib.Numerics;
using ArgusLib.Diagnostics.Tracing;
using System.Reflection;
using System.Numerics;

namespace SamplesConsole
{
	enum MyEnum
	{
		Bla = 2
	}

	class Program
	{
		static void Main(string[] args)
		{
			MyEnum v = BitSet.FromInt32<MyEnum>(2);
			Console.WriteLine(v.ContainsFlag(MyEnum.Bla));
			while (true)
			{
				string expression = Console.ReadLine();
				if (MathExpression.TryEvaluate(expression, out decimal result))
					Console.WriteLine($"= {result}");
				else
					Console.WriteLine("Expression could not be evaluated");
			}
			
			//var a = Scalar<Complex>.Zero;
			//var b = Scalar<Complex>.One;
			//Console.WriteLine(Scalar.Add(a, b));
			//Console.ReadKey();
		}
	}
}
