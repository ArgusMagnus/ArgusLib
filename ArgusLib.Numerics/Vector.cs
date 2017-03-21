using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgusLib.Numerics
{
	public interface IDimensionProvider
	{
		int Dimension { get; }
	}

	public static class Vector<Dim> where Dim:IDimensionProvider,new()
	{
		public static readonly int Dimension = new Dim().Dimension;
	}

	public struct Vector<T, Dim> : IEquatable<Vector<T, Dim>>, IEnumerable<T>
		where Dim : IDimensionProvider, new()
	{
		public static Vector<T, Dim> Zero { get { return new Vector<T, Dim>(); } }
		public static int Dimension => Vector<Dim>.Dimension;
		readonly T[] _elements;

		public bool IsZero { get { return _elements == null; } }

		Vector(T[] elements, bool copy)
		{
			if (elements != null)
			{
				if (elements.Length != Dimension)
					throw new ArgumentException("Wrong number of elements.");
				if (elements.All((e) => Scalar<T>.AreEqual(e, Scalar<T>.Zero)))
					_elements = null;
				{
					if (copy)
					{
						_elements = new T[Dimension];
						elements.CopyTo(_elements, 0);
					}
					else
						_elements = elements;
				}
			}
			else
				_elements = null;
		}

		public Vector(params T[] elements)
			: this(elements, true) { }

		public T this[int index] { get { return _elements != null ? _elements[index] : Scalar<T>.Zero; } }

		public T[] ToArray()
		{
			T[] RetVal = new T[Dimension];
			if (_elements != null)
				_elements.CopyTo(RetVal, 0);
			else
			{
				for (int i = 0; i < Dimension; i++)
					RetVal[i] = Scalar<T>.Zero;
			}
			return RetVal;
		}

		public static Vector<T, Dim> operator +(Vector<T, Dim> a, Vector<T, Dim> b)
		{
			if (a._elements == null)
				return b;
			if (b._elements == null)
				return a;
			T[] elements = new T[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = Scalar<T>.Add(a._elements[i], b._elements[i]);
			return new Vector<T, Dim>(elements, false);
		}

		public static Vector<T, Dim> operator -(Vector<T, Dim> a, Vector<T, Dim> b)
		{
			if (a._elements == null)
				return -b;
			if (b._elements == null)
				return a;
			T[] elements = new T[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = Scalar<T>.Subtract(a._elements[i], b._elements[i]);
			return new Vector<T, Dim>(elements, false);
		}

		public static Vector<T, Dim> operator -(Vector<T, Dim> vector)
		{
			if (vector._elements == null)
				return vector;
			T[] elements = new T[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = Scalar<T>.Negate(vector._elements[i]);
			return new Vector<T, Dim>(elements, false);
		}

		public static T operator *(Vector<T, Dim> a, Vector<T, Dim> b)
		{
			T result = Scalar<T>.Zero;
			if (b._elements == null || b._elements == null)
				return result;
			for (int i = 0; i < Dimension; i++)
				result = Scalar<T>.Add(result, Scalar<T>.Multiply(a._elements[i], b._elements[i]));
			return result;
		}

		public static Vector<T, Dim> operator *(T scalar, Vector<T, Dim> vector)
		{
			if (vector._elements == null)
				return vector;
			T[] elements = new T[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = Scalar<T>.Multiply(scalar, vector._elements[i]);
			return new Vector<T, Dim>(elements, false);
		}

		public static Vector<T, Dim> operator *(Vector<T, Dim> vector, T scalar) => scalar * vector;

		public static Vector<T, Dim> operator /(Vector<T, Dim> vector, T scalar)
		{
			if (vector._elements == null)
				return vector;
			T[] elements = new T[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = Scalar<T>.Divide(vector._elements[i], scalar);
			return new Vector<T, Dim>(elements, false);
		}

		public static bool operator ==(Vector<T, Dim> a, Vector<T, Dim> b)
		{
			if (object.ReferenceEquals(a._elements, b._elements))
				return true;
			if (a._elements == null || b._elements == null)
			{
				T[] elements = a._elements ?? b._elements;
				for (int i = 0; i < Dimension; i++)
				{
					if (!Scalar<T>.AreEqual(elements[i], Scalar<T>.Zero))
						return false;
				}
			}
			else
			{
				for (int i = 0; i < Dimension; i++)
				{
					if (!Scalar<T>.AreEqual(a._elements[i], b._elements[i]))
						return false;
				}
			}
			return true;
		}

		public static bool operator !=(Vector<T, Dim> a, Vector<T, Dim> b) => !(a == b);

		public bool Equals(Vector<T, Dim> other) => this == other;
		public override bool Equals(object obj) => obj is Vector<T, Dim> ? (Vector<T, Dim>)obj == this : false;
		public override int GetHashCode() => _elements?.GetHashCode() ?? 0;

		public Vector<T, SameDim> As<SameDim>() where SameDim : IDimensionProvider, new()
		{
			if (Vector<Dim>.Dimension != Vector<SameDim>.Dimension)
				throw new InvalidOperationException("Dimension mismatch");
			return new Vector<T, SameDim>(_elements, false);
		}

		public Vector<TOther, Dim> Cast<TOther>(Func<T, TOther> cast)
		{
			if (_elements == null)
				return new Vector<TOther, Dim>();

			TOther[] elements = new TOther[Dimension];
			for (int i = 0; i < Dimension; i++)
				elements[i] = cast(_elements[i]);
			return new Vector<TOther, Dim>(elements, false);
		}

		public override string ToString()
		{
			const string del = ", ";
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			if (_elements == null)
			{
				for (int i = 0; i < Dimension - 1; i++)
				{
					sb.Append(Scalar<T>.Zero.ToString());
					sb.Append(del);
				}
				sb.Append(Scalar<T>.Zero.ToString());
			}
			else
			{
				for (int i = 0; i < Dimension - 1; i++)
				{
					sb.Append(_elements[i].ToString());
					sb.Append(del);
				}
				sb.Append(_elements[_elements.Length - 1].ToString());
			}
			sb.Append("]");
			return sb.ToString();
		}

		public Matrix<T, Dim, D1> ToOneColumnMatrix()
		{
			if (_elements == null)
				return new Matrix<T, Dim, D1>();
			var builder = new Matrix<T, Dim, D1>.Builder();
			for (int i = 0; i < Dimension; i++)
				builder[i, 0] = _elements[i];
			return builder.ToMatrix();
		}

		public Matrix<T, D1, Dim> ToOneRowMatrix()
		{
			if (_elements == null)
				return new Matrix<T, D1, Dim>();
			var builder = new Matrix<T, D1, Dim>.Builder();
			for (int i = 0; i < Dimension; i++)
				builder[0, i] = _elements[i];
			return builder.ToMatrix();
		}

		//public static Vector<T, Dim> SetNoiseToZero(T[] values, int startIndex = 0, int count = 0)
		//{
		//	if (values == null)
		//		return new Vector<T, Dim>();
		//	T[] result = Vector.SetNoiseToZero<T>(values, startIndex, count);
		//	return new Vector<T, Dim>(result, false);
		//}

		//public Vector<T, Dim> SetNoiseToZero() => SetNoiseToZero(_elements);

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			foreach (var element in _elements)
				yield return element;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var element in _elements)
				yield return element;
		}

		public class Builder
		{
			Vector<T, Dim> _vector;
			T[] _elements = new T[Dimension];

			public T this[int index]
			{
				get { VerifyModifiable(); return _elements[index]; }
				set { VerifyModifiable(); _elements[index] = value; }
			}

			void VerifyModifiable()
			{
				if (_elements == null)
					throw new InvalidOperationException($"{nameof(ToVector)} has already been called, this {nameof(Vector<T, Dim>.Builder)} is no longer modifiable.");
			}

			public bool IsModifiable { get { return _elements != null; } }

			/// <summary>
			/// Resets the <see cref="Vector{T, Dim}.Builder"/> to its initial state,
			/// all changes are lost.
			/// </summary>
			public void Reset() => _elements = new T[Dimension];

			/// <summary>
			/// Resets the <see cref="Vector{T, Dim}.Builder"/> to its initial state,
			/// but only if this instance is not modifiable anymore (that is, <see cref="ToVector"/> has already
			/// been called on it).
			/// </summary>
			public void Reuse() => _elements = _elements ?? new T[Dimension];

			public Vector<T, Dim> ToVector()
			{
				if (_elements != null)
				{
					_vector = new Vector<T, Dim>(_elements, false);
					_elements = null;
				}
				return _vector;
			}

			public void CopyFrom(T[] array, int index = 0)
			{
				if (array == null)
					throw new ArgumentNullException(nameof(array));
				if (array.Length - index < Dimension)
					throw new ArgumentException("Insufficient number of elements", nameof(array));

				array.CopyTo(_elements, index);
			}
		}
	}

	public struct D1 : IDimensionProvider { public int Dimension { get { return 1; } } }
	public struct D2 : IDimensionProvider { public int Dimension { get { return 2; } } }
	public struct D3 : IDimensionProvider { public int Dimension { get { return 3; } } }
	public struct D4 : IDimensionProvider { public int Dimension { get { return 4; } } }
	public struct D5 : IDimensionProvider { public int Dimension { get { return 5; } } }
	public struct D6 : IDimensionProvider { public int Dimension { get { return 6; } } }
	public struct D7 : IDimensionProvider { public int Dimension { get { return 7; } } }
	public struct D8 : IDimensionProvider { public int Dimension { get { return 8; } } }
	public struct D9 : IDimensionProvider { public int Dimension { get { return 9; } } }
	public struct D10 : IDimensionProvider { public int Dimension { get { return 10; } } }

	public static class Vector
	{
		public static T CrossProduct<T>(Vector<T, D2> a, Vector<T, D2> b)
		{
			return Scalar<T>.Subtract(Scalar<T>.Multiply(a[0], b[1]), Scalar<T>.Multiply(a[1], b[0]));
		}

		public static Vector<T, D3> CrossProduct<T>(Vector<T, D3> a, Vector<T, D3> b)
		{
			Vector<T, D3>.Builder x = new Vector<T, D3>.Builder();
			x[0] = Scalar<T>.Subtract(Scalar<T>.Multiply(a[1], b[2]), Scalar<T>.Multiply(a[2], b[1]));
			x[1] = Scalar<T>.Subtract(Scalar<T>.Multiply(a[2], b[0]), Scalar<T>.Multiply(a[0], b[2]));
			x[2] = Scalar<T>.Subtract(Scalar<T>.Multiply(a[0], b[1]), Scalar<T>.Multiply(a[1], b[0]));
			return x.ToVector();
		}

		public static Matrix<T, D3, D3> ToSkewSymmetricMatrix<T>(this Vector<T, D3> v)
		{
			var result = new Matrix<T, D3, D3>.Builder();
			result[0, 1] = Scalar<T>.Negate(v[2]);
			result[0, 2] = v[1];
			result[1, 0] = v[2];
			result[1, 2] = Scalar<T>.Negate(v[0]);
			result[2, 0] = Scalar<T>.Negate(v[1]);
			result[2, 1] = v[0];
			return result.ToMatrix();
		}

		public static double GetLength<Dim>(this Vector<int, Dim> vector) where Dim : IDimensionProvider, new() => Math.Sqrt(vector * vector);
		public static double GetLength<Dim>(this Vector<long, Dim> vector) where Dim : IDimensionProvider, new() => Math.Sqrt(vector * vector);
		public static double GetLength<Dim>(this Vector<float, Dim> vector) where Dim : IDimensionProvider, new() => Math.Sqrt(vector * vector);
		public static double GetLength<Dim>(this Vector<double, Dim> vector) where Dim : IDimensionProvider, new() => Math.Sqrt(vector * vector);

		public static Vector<double, D3> RotateX(this Vector<double, D3> v, Angle phi)
		{
			double cos = phi.Cos();
			double sin = phi.Sin();
			var builder = new Vector<double, D3>.Builder();
			builder[0] = v[0];
			builder[1] = cos * v[1] - sin * v[2];
			builder[2] = sin * v[1] + cos * v[2];
			return builder.ToVector();
		}

		public static Vector<double, D3> RotateY(this Vector<double, D3> v, Angle phi)
		{
			double cos = phi.Cos();
			double sin = phi.Sin();
			var builder = new Vector<double, D3>.Builder();
			builder[0] = cos * v[0] + sin * v[2];
			builder[1] = v[1];
			builder[2] = -sin * v[0] + cos * v[2];
			return builder.ToVector();
		}

		public static Vector<double, D3> RotateZ(this Vector<double, D3> v, Angle phi)
		{
			double cos = phi.Cos();
			double sin = phi.Sin();
			var builder = new Vector<double, D3>.Builder();
			builder[0] = cos * v[0] - sin * v[1];
			builder[1] = sin * v[0] + cos * v[1];
			builder[2] = v[2];
			return builder.ToVector();
		}

		public static Vector<double, Dim> ToUnit<Dim>(this Vector<double, Dim> v)
			where Dim : IDimensionProvider, new()
		{
			if (v.IsZero)
				throw new ArgumentException($"{nameof(v)} must have at least one element that is not equal to zero.");
			return v / v.GetLength();
		}

		public static Matrix<T, Dim1, Dim2> TensorProduct<T, Dim1, Dim2>(Vector<T, Dim1> u, Vector<T, Dim2> v)
			where Dim1 : IDimensionProvider, new()
			where Dim2 : IDimensionProvider, new()
		{
			if (u.IsZero || v.IsZero)
				return new Matrix<T, Dim1, Dim2>();
			var builder = new Matrix<T, Dim1, Dim2>.Builder();
			for (int row = 0; row < Vector<T, Dim1>.Dimension; row++)
			{
				for (int col = 0; col < Vector<T, Dim2>.Dimension; col++)
					builder[row, col] = Scalar<T>.Multiply(u[row], v[col]);
			}
			return builder.ToMatrix();
		}

		/// <summary>
		/// Sets all elements which have no effect when added to the element with the maximum absolute value
		/// (<see cref="IScalar{T}.Abs"/>): <c>output[i] = max.Add(input[i]).Equals(max) ? <see cref="Scalar{T}.Zero"/> : input[i]</c>.
		/// Supports inplace modification (input == output).
		/// </summary>
		//public static void SetNoiseToZero<T>(T[] input, T[] output, int indexInput = 0, int indexOutput = 0, int count = 0)
		//	where T : struct, IScalar<T>, IComparable<T>
		//{
		//	if (input == null)
		//		throw new ArgumentNullException(nameof(input));
		//	if (output == null)
		//		throw new ArgumentNullException(nameof(output));
		//	if (count < 1)
		//		count = input.Length;
		//	else if (input.Length - indexInput < count)
		//		throw new ArgumentException($"{nameof(input.Length)} - {nameof(indexInput)} must be equal to or greater than {nameof(count)}");
		//	if (output.Length-indexOutput < count)
		//		throw new ArgumentException($"{nameof(output.Length)} - {nameof(indexOutput)} must be equal to or greater than {nameof(count)}");

		//	if (count < 2)
		//		return;

		//	T max = input[indexInput].Abs;
		//	for (int i = indexInput+1; i < indexInput+count; i++)
		//	{
		//		T val = input[i].Abs;
		//		if (max.CompareTo(val) > 0)
		//			max = val;
		//	}

		//	for (int i = indexInput; i < indexInput + count; i++)
		//	{
		//		if (max.Add(input[i]).Equals(max))
		//			output[i] = Scalar<T>.Zero;
		//		else if (input != output)
		//			output[i] = input[i];
		//	}
		//}

		//public static T[] SetNoiseToZero<T>(T[] values, int startIndex = 0, int count = 0)
		//	where T : struct, IScalar<T>
		//{
		//	if (count < 1)
		//		count = values.Length;
		//	T[] output = new T[count];
		//	SetNoiseToZero(values, output, startIndex, 0, count);
		//	return output;
		//}
	}
}
