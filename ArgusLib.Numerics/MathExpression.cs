using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Numerics
{
	public static class MathExpression
	{
		public static bool TryEvaluate<TParsable, TOpProvider>(string expression, out TParsable result)
			where TOpProvider : IOperatorProvider<TParsable>, new()
		{
			return MathExpression<TParsable, TOpProvider>.TryEvaluate(expression, out result);
		}

		public static bool TryEvaluate<TParsableScalar>(string expression, out TParsableScalar result)
		{
			return MathExpression<TParsableScalar, ScalarOperatorProvider<TParsableScalar>>.TryEvaluate(expression, out result);
		}
	}

	static class MathExpression<TParsable, TOpProvider>
		where TOpProvider : IOperatorProvider<TParsable>, new()
	{
		struct IndexedOpInfo
		{
			public int Index { get; set; }
			public OperatorDescription OpInfo { get; set; }
		}

		public static bool TryEvaluate(string expression, out TParsable result)
		{
			if (expression == null)
				throw Tracer.ThrowError(new ArgumentNullException(nameof(expression)), typeof(MathExpression<TParsable, TOpProvider>));

			result = default(TParsable);
			Queue<TParsable> operandQueue;
			Queue<OperatorDescription> operatorQueue;
			if (!GetOperatorsAndOperands(expression, out operatorQueue, out operandQueue))
				return false;
			if (operatorQueue.Count == 0)
			{
				result = operandQueue.Dequeue();
				return true;
			}

			var opProvider = new TOpProvider();
			Stack<TParsable> operandStack = new Stack<TParsable>(operandQueue.Count);
			operandStack.Push(operandQueue.Dequeue());
			Stack<OperatorDescription> operatorStack = new Stack<OperatorDescription>(operatorQueue.Count);
			operatorStack.Push(operatorQueue.Dequeue());
			if (operatorStack.Peek().Type == OperatorTypes.BinaryOperator)
				operandStack.Push(operandQueue.Dequeue());

			while (operatorStack.Count > 0)
			{
				var op1 = operatorStack.Peek();
				bool operatorQueueIsEmpty = operatorQueue.Count == 0;
				var op2 = operatorQueueIsEmpty ? op1 : operatorQueue.Peek();

				if (op1.Type == OperatorTypes.OpeningBracket && op2.Type == OperatorTypes.ClosingBracket)
				{
					operatorStack.Pop();
					operatorQueue.Dequeue();
					if (operatorStack.Count == 0 && operatorQueue.Count > 0)
					{
						op2 = operatorQueue.Dequeue();
						operatorStack.Push(op2);
						if (op2.Type == OperatorTypes.BinaryOperator)
							operandStack.Push(operandQueue.Dequeue());
					}
				}
				else if (op1.Type == OperatorTypes.OpeningBracket)
				{
					operatorStack.Push(operatorQueue.Dequeue());
					if (op2.Type == OperatorTypes.BinaryOperator)
						operandStack.Push(operandQueue.Dequeue());
				}
				else
				{
					bool op1IsUnary = op1.Type == OperatorTypes.UnaryOperatorPrefixed || op1.Type == OperatorTypes.UnaryOperatorSuffixed;
					bool op2IsUnary = op2.Type == OperatorTypes.UnaryOperatorPrefixed || op2.Type == OperatorTypes.UnaryOperatorSuffixed;
					bool op1IsBinary = op1.Type == OperatorTypes.BinaryOperator;
					bool op2IsBinary = op2.Type == OperatorTypes.BinaryOperator;

					if ((op2.Type == OperatorTypes.ClosingBracket) ||
						(op1IsUnary && op2.Type == OperatorTypes.BinaryOperator) ||
						(op1IsUnary && op2IsUnary && op1.Priority >= op2.Priority) ||
						(op1IsBinary && op2IsBinary && op1.Priority >= op2.Priority))
					{
						// Operator 1 has priority
						if (op1IsBinary)
						{
							var operand2 = operandStack.Pop();
							var operand1 = operandStack.Pop();
							var res = opProvider.ApplyBinaryOperator(op1, operand1, operand2);
							operandStack.Push(res);
						}
						else if (op1IsUnary)
						{
							var operand = operandStack.Pop();
							var res = opProvider.ApplyUnaryOperator(op1, operand);
							operandStack.Push(res);
						}
						else
							throw Tracer.ThrowLogAlways(new BugException($"Expression: '{expression}'"), typeof(MathExpression<TParsable, TOpProvider>));

						operatorStack.Pop();
					}

					if (!operatorQueueIsEmpty && op2.Type != OperatorTypes.ClosingBracket)
					{
						operatorStack.Push(operatorQueue.Dequeue());
						if (op2.Type == OperatorTypes.BinaryOperator)
							operandStack.Push(operandQueue.Dequeue());
					}
				}
			}

			if (operandStack.Count != 1)
				throw Tracer.ThrowLogAlways(new BugException($"Expression: '{expression}'"), typeof(MathExpression<TParsable, TOpProvider>));

			result = operandStack.Pop();
			return true;
		}

		static bool GetOperatorsAndOperands(string expression, out Queue<OperatorDescription> operatorQueue, out Queue<TParsable> operandQueue)
		{
			var opProvider = new TOpProvider();
			expression = expression.Replace(" ", "");
			if (string.IsNullOrEmpty(expression))
			{
				operatorQueue = null;
				operandQueue = null;
				Tracer.WriteVerbose($"Expression '{expression}' is empty or consists only of spaces.", typeof(MathExpression<TParsable, TOpProvider>));
				return false;
			}

			List<IndexedOpInfo> ops = new List<IndexedOpInfo>();
			foreach (string opSymbol in opProvider.OperatorSymbols)
			{
				OperatorDescription opInfo = opProvider.GetOperatorInfo(opSymbol);
				ops.AddRange(expression.IndicesOf(opSymbol).Select((i) => new IndexedOpInfo() { Index = i, OpInfo = opInfo }));
			}
			ops.Sort((op1, op2) => op1.Index - op2.Index);

			operandQueue = new Queue<TParsable>(ops.Count + 1);
			operatorQueue = new Queue<OperatorDescription>(ops.Count);

			int noExpectedOperands = 1;
			int bracketBalance = 0;
			string strOperand1 = ops.Count > 0 ? expression.Substring(0, ops[0].Index) : expression;
			TParsable operand;
			if (strOperand1.Length > 0)
			{
				if (!Parser.TryParse(strOperand1, out operand))
				{
					Tracer.WriteVerbose($"Expression '{expression}' (Index: {0}): Operand '{strOperand1}' could not be parsed.", typeof(MathExpression<TParsable, TOpProvider>));
					return false;
				}
				operandQueue.Enqueue(operand);
			}

			for (int i = 0; i < ops.Count; i++)
			{
				var opInfo = ops[i].OpInfo;
				int idxOperand2 = ops[i].Index + opInfo.Symbol.Length;
				int idxNextOperator = i < ops.Count - 1 ? ops[i + 1].Index : expression.Length;
				string strOperand2 = expression.Substring(idxOperand2, idxNextOperator - idxOperand2);

				if (EnumEx.HasFlag(opInfo.Type, OperatorTypes.OpeningBracket))
				{
					if (ops[i].Index > 0 && !string.IsNullOrEmpty(strOperand1))
					{
						Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): An opening bracket can only be preceded by an operator or an empty string.", typeof(MathExpression<TParsable, TOpProvider>));
						return false;
					}
					opInfo = new OperatorDescription(opInfo.Symbol, opInfo.Priority, OperatorTypes.OpeningBracket);
					bracketBalance++;
				}
				else if (EnumEx.HasFlag(opInfo.Type, OperatorTypes.ClosingBracket))
				{
					if (idxOperand2 < expression.Length - 1 && !string.IsNullOrEmpty(strOperand2))
					{
						Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): An closing bracket can only precede an operator or an empty string.", typeof(MathExpression<TParsable, TOpProvider>));
						return false;
					}
					bracketBalance--;
					if (bracketBalance < 0)
					{
						Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): Unbalanced brackets.", typeof(MathExpression<TParsable, TOpProvider>));
						return false;
					}
					opInfo = new OperatorDescription(opInfo.Symbol, opInfo.Priority, OperatorTypes.ClosingBracket);
				}
				else
				{
					bool op1IsValid = !string.IsNullOrEmpty(strOperand1) || (i > 0 && EnumEx.HasFlag(ops[i - 1].OpInfo.Type, OperatorTypes.ClosingBracket));
					bool op2IsValid = !string.IsNullOrEmpty(strOperand2) || (i < ops.Count - 1 && EnumEx.HasFlag(ops[i + 1].OpInfo.Type, OperatorTypes.OpeningBracket));

					if (!op1IsValid && !op2IsValid)
					{
						Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): Missing operands.", typeof(MathExpression<TParsable, TOpProvider>));
						return false;
					}

					if (!op1IsValid)
					{
						if (EnumEx.HasFlag(opInfo.Type, OperatorTypes.UnaryOperatorPrefixed))
							opInfo = new OperatorDescription(opInfo.Symbol, opInfo.Priority, OperatorTypes.UnaryOperatorPrefixed);
						else
						{
							Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): Missing operand.", typeof(MathExpression<TParsable, TOpProvider>));
							return false;
						}
					}
					else if (!op2IsValid)
					{
						if (EnumEx.HasFlag(opInfo.Type, OperatorTypes.UnaryOperatorSuffixed))
							opInfo = new OperatorDescription(opInfo.Symbol, opInfo.Priority, OperatorTypes.UnaryOperatorSuffixed);
						else
						{
							Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): Missing operand.", typeof(MathExpression<TParsable, TOpProvider>));
							return false;
						}
					}
					else
					{
						if (EnumEx.HasFlag(opInfo.Type, OperatorTypes.BinaryOperator))
						{
							opInfo = new OperatorDescription(opInfo.Symbol, opInfo.Priority, OperatorTypes.BinaryOperator);
							noExpectedOperands++;
						}
						else
						{
							Tracer.WriteVerbose($"Expression '{expression}' (Index: {ops[i].Index}): Missing operator.", typeof(MathExpression<TParsable, TOpProvider>));
							return false;
						}
					}
				}

				if (strOperand2.Length > 0)
				{
					if (!Parser.TryParse(strOperand2, out operand))
					{
						Tracer.WriteVerbose($"Expression '{expression}': Operand '{strOperand2}' could not be parsed.", typeof(MathExpression<TParsable, TOpProvider>));
						return false;
					}
					operandQueue.Enqueue(operand);
				}
				strOperand1 = strOperand2;
				operatorQueue.Enqueue(opInfo);
			}

			if (operandQueue.Count != noExpectedOperands || bracketBalance != 0)
			{
				Tracer.WriteVerbose($"Expression '{expression}': Wrong number of operands or unbalanced brackets.", typeof(MathExpression<TParsable, TOpProvider>));
				return false;
			}
			return true;
		}
	}
}
