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

namespace SamplesConsole
{
	public struct Test
	{
		public int Value { get; }

		public Test(int value)
		{
			Value = value;
		}

		public static Test operator +(Test a, Test b) => new Test(a.Value + b.Value);
		public static Test operator -(Test a, Test b) => new Test(a.Value - b.Value);

		public override string ToString() => Value.ToString();

		public static explicit operator Test(int i)=> new Test(i);
	}

	class Program
	{
		public static event Action<object,string> Event;

		static void Main(string[] args)
		{
			Test a = Scalar<Test>.One;
			Test b = Scalar<Test>.Zero;
			Console.WriteLine(Scalar<Test>.Add(a, b));
			Console.ReadKey();
		}
	}
}
