using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Numerics
{
	public struct ScalarOperatorProvider<T> : IOperatorProvider<T> where T : IScalar<T>
	{
		IEnumerable<string> IOperatorProvider<T>.OperatorSymbols
		{
			get
			{
				yield return "+";
				yield return "-";
				yield return "*";
				yield return "/";
				yield return "(";
				yield return ")";
			}
		}

		OperatorDescription IOperatorProvider<T>.GetOperatorInfo(string symbol)
		{
			if (symbol == "*" || symbol == "/")
				return new OperatorDescription(symbol, 1, OperatorTypes.BinaryOperator);
			if (symbol == "+")
				return new OperatorDescription(symbol, 0, OperatorTypes.BinaryOperator);
			if (symbol == "-")
				return new OperatorDescription(symbol, 0, OperatorTypes.BinaryOperator | OperatorTypes.UnaryOperatorPrefixed);
			if (symbol == "(")
				return new OperatorDescription(symbol, 0, OperatorTypes.OpeningBracket);
			if (symbol == ")")
				return new OperatorDescription(symbol, 0, OperatorTypes.ClosingBracket);

			throw Tracer.ThrowCritical<ScalarOperatorProvider<T>>(new ArgumentException(string.Format(Exceptions.OperatorProvider_UnsupportedOperator, symbol), nameof(symbol)));
		}

		T IOperatorProvider<T>.ApplyUnaryOperator(OperatorDescription op, T operand)
		{
			if (op.Symbol == "-")
				return operand.Negate();
			throw Tracer.ThrowCritical<ScalarOperatorProvider<T>>(new ArgumentException(string.Format(Exceptions.OperatorProvider_UnsupportedOperator, op.Symbol), nameof(op)));
		}

		T IOperatorProvider<T>.ApplyBinaryOperator(OperatorDescription op, T operand1, T operand2)
		{
			if (op.Symbol == "+")
				return operand1.Add(operand2);
			if (op.Symbol == "-")
				return operand1.Subtract(operand2);
			if (op.Symbol == "*")
				return operand1.Multiply(operand2);
			if (op.Symbol == "/")
				return operand1.Divide(operand2);
			throw Tracer.ThrowCritical<ScalarOperatorProvider<T>>(new ArgumentException(string.Format(Exceptions.OperatorProvider_UnsupportedOperator, op.Symbol), nameof(op)));
		}
	}
}
