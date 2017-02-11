using System;
using System.Collections.Generic;
using System.Text;

namespace ArgusLib.Numerics
{
	[Flags]
	public enum OperatorTypes : byte
	{
		UnaryOperatorPrefixed = 1,
		UnaryOperatorSuffixed = 1 << 1,
		BinaryOperator = 1 << 2,
		OpeningBracket = 1 << 3,
		ClosingBracket = 1 << 4
	}

	public struct OperatorDescription
	{
		readonly string _symbol;
		readonly int _priority;
		readonly OperatorTypes _type;

		public string Symbol => _symbol;
		public int Priority => _priority;
		public OperatorTypes Type => _type;

		public OperatorDescription(string symbol, int priority, OperatorTypes type)
		{
			_symbol = symbol;
			_priority = priority;
			_type = type;
		}

		public override string ToString() => $"{{ {_symbol}, {_priority}, {_type}}}";
	}

	public interface IOperatorProvider<T>
	{
		IEnumerable<string> OperatorSymbols { get; }
		OperatorDescription GetOperatorInfo(string operatorSymbol);
		T ApplyUnaryOperator(OperatorDescription op, T operand);
		T ApplyBinaryOperator(OperatorDescription op, T operand1, T operand2);
	}
}
