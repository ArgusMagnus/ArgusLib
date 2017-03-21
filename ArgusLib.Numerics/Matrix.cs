using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib.Numerics
{
	public enum MatrixTypes
	{
		Default,
		Diagonal,
		UpperTriangleDiagonalInclusive,
		UpperTriangleDiagonalExclusive,
		LowerTriangleDiagonalInclusive,
		LowerTriangleDiagonalExclusive
	}

	public static class Matrix<ColDim, RowDim>
		where ColDim : IDimensionProvider, new()
		where RowDim : IDimensionProvider, new()
	{
		public static readonly int ColumnDimension = new ColDim().Dimension;
		public static readonly int RowDimension = new RowDim().Dimension;
		public static int RowCount => ColumnDimension;
		public static int ColumnCount => RowDimension;

		public static int GetElementCount(MatrixTypes matrixType)
		{
			switch (matrixType)
			{
				case MatrixTypes.Diagonal:
					return Math.Min(RowDimension, ColumnDimension);
				case MatrixTypes.LowerTriangleDiagonalExclusive:
					return RowCount <= ColumnCount ? (RowCount * RowCount + RowCount) / 2 : GetElementCount(MatrixTypes.Default) - GetElementCount(MatrixTypes.UpperTriangleDiagonalInclusive);
				case MatrixTypes.LowerTriangleDiagonalInclusive:
					return RowCount <= ColumnCount ? (RowCount * RowCount + RowCount) / 2 : GetElementCount(MatrixTypes.Default) - GetElementCount(MatrixTypes.UpperTriangleDiagonalExclusive);
				case MatrixTypes.UpperTriangleDiagonalExclusive:
					return RowCount >= ColumnCount ? (ColumnCount * ColumnCount + ColumnCount) / 2 : GetElementCount(MatrixTypes.Default) - GetElementCount(MatrixTypes.LowerTriangleDiagonalInclusive);
				case MatrixTypes.UpperTriangleDiagonalInclusive:
					return RowCount >= ColumnCount ? (ColumnCount * ColumnCount + ColumnCount) / 2 : GetElementCount(MatrixTypes.Default) - GetElementCount(MatrixTypes.LowerTriangleDiagonalExclusive);
				case MatrixTypes.Default:
				default:
					return RowDimension * ColumnDimension;
			}
		}
	}


	/// <summary>
	/// Basic matrix implementation. Not optimized for large or special (sparse, diagonal, triagonal, etc.) matrices.
	/// </summary>
	public struct Matrix<T, ColDim, RowDim> : IEquatable<Matrix<T, ColDim, RowDim>>
		where RowDim : IDimensionProvider, new()
		where ColDim : IDimensionProvider, new()
	{
		public static int RowDimension => Matrix<ColDim, RowDim>.RowDimension;
		public static int ColumnDimension => Matrix<ColDim, RowDim>.ColumnDimension;
		public static int RowCount => Matrix<ColDim, RowDim>.RowCount;
		public static int ColumnCount => Matrix<ColDim, RowDim>.ColumnCount;
		public static int ElementCount { get { return RowDimension * ColumnDimension; } }
		public static Matrix<T, ColDim, RowDim> Zero { get { return new Matrix<T, ColDim, RowDim>(); } }

		readonly T[,] _elements;

		public bool IsZero { get { return _elements == null; } }

		Matrix(T[,] elements, bool copy)
		{
			if (elements != null)
			{
				if (elements.GetLength(0) != RowCount || elements.GetLength(1) != ColumnCount)
					throw new ArgumentException($"Dimension mistach: {elements.GetLength(0)} x {elements.GetLength(1)} instead of {RowCount} x {ColumnCount}");

				bool isZero = true;
				for (int row = 0; row < RowCount && isZero == true; row++)
				{
					for (int col = 0; col < ColumnCount; col++)
					{
						if (!Scalar<T>.AreEqual(elements[row, col], Scalar<T>.Zero))
						{
							isZero = false;
							break;
						}
					}
				}

				if (isZero)
					_elements = null;
				else
				{
					if (copy)
					{
						_elements = new T[RowCount, ColumnCount];
						Array.Copy(elements, _elements, elements.Length);
					}
					else
						_elements = elements;
				}
			}
			else
				_elements = null;
		}

		public Matrix(T[,] elements)
		: this(elements, true) { }

		public T this[int row, int col] { get { return _elements != null ? _elements[row, col] : Scalar<T>.Zero; } }

		public int GetSpecialElementCount(MatrixTypes type) => Matrix<ColDim, RowDim>.GetElementCount(type);

		public static Matrix<T, ColDim, RowDim> operator +(Matrix<T, ColDim, RowDim> a, Matrix<T, ColDim, RowDim> b)
		{
			if (a._elements == null)
				return b;
			if (b._elements == null)
				return a;
			T[,] result = new T[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[row, col] = Scalar<T>.Add(a[row, col], b[row, col]);
			}
			return new Matrix<T, ColDim, RowDim>(result, false);
		}

		public static Matrix<T, ColDim, RowDim> operator -(Matrix<T, ColDim, RowDim> a, Matrix<T, ColDim, RowDim> b)
		{
			if (a._elements == null)
				return -b;
			if (b._elements == null)
				return a;
			T[,] result = new T[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[row, col] = Scalar<T>.Subtract(a[row, col], b[row, col]);
			}
			return new Matrix<T, ColDim, RowDim>(result, false);
		}

		public static Matrix<T, ColDim, ColDim> operator *(Matrix<T, ColDim, RowDim> a, Matrix<T, RowDim, ColDim> b) => Matrix.Multiply(a, b);

		public static Matrix<T, ColDim, RowDim> operator -(Matrix<T, ColDim, RowDim> matrix)
		{
			if (matrix._elements == null)
				return matrix;
			T[,] result = new T[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[row, col] = Scalar<T>.Negate(matrix[row, col]);
			}
			return new Matrix<T, ColDim, RowDim>(result, false);
		}

		public static Vector<T, ColDim> operator *(Matrix<T, ColDim, RowDim> matrix, Vector<T, RowDim> vector)
		{
			if (matrix._elements == null || vector.IsZero)
				return Vector<T, ColDim>.Zero;
			var result = new Vector<T, ColDim>.Builder();
			for (int row = 0; row < RowCount; row++)
			{
				T val = Scalar<T>.Zero;
				for (int col = 0; col < ColumnCount; col++)
					val = Scalar<T>.Add(val, Scalar<T>.Multiply(matrix[row, col], vector[col]));
				result[row] = val;
			}
			return result.ToVector();
		}

		public static Vector<T, RowDim> operator *(Vector<T, ColDim> vector, Matrix<T, ColDim, RowDim> matrix)
		{
			if (matrix._elements == null || vector.IsZero)
				return Vector<T, RowDim>.Zero;
			var result = new Vector<T, RowDim>.Builder();
			for (int col = 0; col < ColumnCount; col++)
			{
				T val = Scalar<T>.Zero;
				for (int row = 0; row < RowCount; row++)
					val = Scalar<T>.Add(val, Scalar<T>.Multiply(matrix[row, col], vector[row]));
				result[col] = val;
			}
			return result.ToVector();
		}

		public Matrix<TOther, ColDim, RowDim> Cast<TOther>(Func<T, TOther> cast)
		{
			if (_elements == null)
				return Matrix<TOther, ColDim, RowDim>.Zero;

			TOther[,] elements = new TOther[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					elements[row, col] = cast(_elements[row, col]);
			}
			return new Matrix<TOther, ColDim, RowDim>(elements, false);
		}

		public static Matrix<T, ColDim, RowDim> operator *(T scalar, Matrix<T, ColDim, RowDim> matrix)
		{
			if (matrix._elements == null || Scalar<T>.AreEqual(scalar, Scalar<T>.Zero))
				return Matrix<T, ColDim, RowDim>.Zero;
			if (Scalar<T>.AreEqual(scalar, Scalar<T>.One))
				return matrix;
			T[,] result = new T[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[row, col] = Scalar<T>.Multiply(scalar, matrix[row, col]);
			}
			return new Matrix<T, ColDim, RowDim>(result, false);
		}

		public static Matrix<T, ColDim, RowDim> operator *(Matrix<T, ColDim, RowDim> matrix, T scalar) => scalar * matrix;

		public static Matrix<T, ColDim, RowDim> operator /(Matrix<T, ColDim, RowDim> matrix, T scalar)
		{
			if (Scalar<T>.AreEqual(scalar, Scalar<T>.Zero))
				throw new DivideByZeroException();
			if (Scalar<T>.AreEqual(scalar, Scalar<T>.One))
				return matrix;
			T[,] result = new T[RowCount, ColumnCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[row, col] = Scalar<T>.Divide(matrix[row, col], scalar);
			}
			return new Matrix<T, ColDim, RowDim>(result, false);
		}

		public static bool operator ==(Matrix<T, ColDim, RowDim> a, Matrix<T, ColDim, RowDim> b)
		{
			if (object.ReferenceEquals(a._elements, b._elements))
				return true;
			if (a._elements == null || b._elements == null)
			{
				T[,] elements = a._elements ?? b._elements;
				foreach (T element in elements)
				{
					if (!Scalar<T>.AreEqual(element, Scalar<T>.Zero))
						return false;
				}
			}
			else
			{
				for (int row = 0; row < RowCount; row++)
				{
					for (int col = 0; col < ColumnCount; col++)
					{
						if (!Scalar<T>.AreEqual(a._elements[row, col], b._elements[row, col]))
							return false;
					}
				}
			}
			return true;
		}

		public static bool operator !=(Matrix<T, ColDim, RowDim> a, Matrix<T, ColDim, RowDim> b) => !(a == b);

		public bool Equals(Matrix<T, ColDim, RowDim> other) => this == other;
		public override bool Equals(object obj) => obj is Matrix<T, ColDim, RowDim> ? (Matrix<T, ColDim, RowDim>)obj == this : false;
		public override int GetHashCode() => _elements?.GetHashCode() ?? 0;

		public class Builder
		{
			Matrix<T, ColDim, RowDim> _matrix;
			T[,] _elements = new T[ColumnDimension, RowDimension];

			public T this[int row, int column]
			{
				get { VerifyModifiable(); return _elements[row, column]; }
				set { VerifyModifiable(); _elements[row, column] = value; }
			}

			void VerifyModifiable()
			{
				if (_elements == null)
					throw new InvalidOperationException($"{nameof(ToMatrix)} has already been called, this {nameof(Matrix<T, ColDim, RowDim>.Builder)} is no longer modifiable.");
			}

			public bool IsModifiable { get { return _elements != null; } }

			/// <summary>
			/// Resets the <see cref="Matrix{T, ColDim, RowDim}.Builder"/> to its initial state,
			/// all changes are lost.
			/// </summary>
			public void Reset() => _elements = new T[ColumnDimension, RowDimension];

			/// <summary>
			/// Resets the <see cref="Matrix{T, ColDim, RowDim}.Builder"/> ot its initial state,
			/// but only if this instance is not modifiable anymore (that is, <see cref="ToMatrix"/> has already
			/// been called on it).
			/// </summary>
			public void Reuse() => _elements = _elements ?? new T[ColumnDimension, RowDimension];

			public Matrix<T, ColDim, RowDim> ToMatrix()
			{
				if (_elements != null)
				{
					_matrix = new Matrix<T, ColDim, RowDim>(_elements, false);
					_elements = null;
				}
				return _matrix;
			}

			public void CopyFrom(T[,] array)
			{
				if (array == null)
					throw new ArgumentNullException(nameof(array));
				if (array.GetLength(0) != RowCount || array.GetLength(1) != ColumnCount)
					throw new ArgumentException($"Dimension mistach: {array.GetLength(0)} x {array.GetLength(1)} instead of {RowCount} x {ColumnCount}");

				Array.Copy(array, _elements, _elements.Length);
			}
		}

		public Vector<T, RowDim>[] ToRowVectors()
		{
			var result = new Vector<T, RowDim>[RowCount];
			if (_elements == null)
				return result;

			var builder = new Vector<T, RowDim>.Builder();
			for (int row = 0; row < RowCount; row++)
			{
				builder.Reuse();
				for (int col = 0; col < ColumnCount; col++)
					builder[col] = _elements[row, col];
				result[row] = builder.ToVector();
			}
			return result;
		}

		public Vector<T, ColDim>[] ToColumnVectors()
		{
			var result = new Vector<T, ColDim>[ColumnCount];
			if (_elements == null)
				return result;

			var builder = new Vector<T, ColDim>.Builder();
			for (int col = 0; col < ColumnCount; col++)
			{
				builder.Reuse();
				for (int row = 0; row < RowCount; row++)
					builder[row] = _elements[row, col];
				result[col] = builder.ToVector();
			}
			return result;
		}

		public Matrix<T, RowDim, ColDim> Transpose()
		{
			if (_elements == null)
				return new Matrix<T, RowDim, ColDim>();
			T[,] result = new T[ColumnCount, RowCount];
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
					result[col, row] = _elements[row, col];
			}
			return new Matrix<T, RowDim, ColDim>(result, false);
		}
	}

	public static class Matrix
	{
		public static Matrix<T, Dim, Dim> CreateDiagonal<T, Dim>(params T[] diagonalElements)
			where Dim : IDimensionProvider, new()
		{
			if (diagonalElements == null)
				return new Matrix<T, Dim, Dim>();
			if (diagonalElements.Length != Matrix<T, Dim, Dim>.ColumnDimension)
				throw new ArgumentException($"Invalid number of elements: {diagonalElements.Length} instead of {Matrix<T, Dim, Dim>.ColumnDimension}");
			var builder = new Matrix<T, Dim, Dim>.Builder();
			for (int i = 0; i < diagonalElements.Length; i++)
				builder[i, i] = diagonalElements[i];
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotationX(Angle rad)
		{
			var builder = new Matrix<double, D3, D3>.Builder();
			double sin = rad.Sin();
			double cos = rad.Cos();
			builder[0, 0] = 1.0;
			builder[1, 1] = cos;
			builder[1, 2] = -sin;
			builder[2, 1] = sin;
			builder[2, 2] = cos;
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotationY(Angle rad)
		{
			var builder = new Matrix<double, D3, D3>.Builder();
			double sin = rad.Sin();
			double cos = rad.Cos();
			builder[0, 0] = cos;
			builder[1, 1] = 1.0;
			builder[2, 2] = cos;
			builder[0, 2] = sin;
			builder[2, 0] = -sin;
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotationZ(Angle rad)
		{
			var builder = new Matrix<double, D3, D3>.Builder();
			double sin = rad.Sin();
			double cos = rad.Cos();
			builder[0, 0] = cos;
			builder[1, 1] = cos;
			builder[2, 2] = 1.0;
			builder[0, 1] = -sin;
			builder[1, 0] = sin;
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotationEulerZXZ(Angle alpha, Angle beta, Angle gamma)
		{
			double aSin = alpha.Sin();
			double aCos = alpha.Cos();
			double bSin = beta.Sin();
			double bCos = beta.Cos();
			double cSin = gamma.Sin();
			double cCos = gamma.Cos();
			var builder = new Matrix<double, D3, D3>.Builder();
			builder[0, 0] = aCos * cCos - aSin * bCos * cSin;
			builder[0, 1] = aSin * cCos + aCos * bCos * cSin;
			builder[0, 2] = bSin * cSin;
			builder[1, 0] = -aCos * cSin - aSin * bCos * cCos;
			builder[1, 1] = aCos * bCos * cCos - aSin * cSin;
			builder[1, 2] = bSin * cCos;
			builder[2, 0] = aSin * bSin;
			builder[2, 1] = -aCos * bSin;
			builder[2, 2] = bCos;
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotationEulerZYZ(Angle alpha, Angle beta, Angle gamma)
		{
			double aSin = alpha.Sin();
			double aCos = alpha.Cos();
			double bSin = beta.Sin();
			double bCos = beta.Cos();
			double cSin = gamma.Sin();
			double cCos = gamma.Cos();
			var builder = new Matrix<double, D3, D3>.Builder();
			builder[0, 0] = aCos * bCos * cCos - aSin * cSin;
			builder[0, 1] = aCos * cSin + aSin * bCos * cCos;
			builder[0, 2] = -bSin * cCos;
			builder[1, 0] = -aSin * cCos - aCos * bCos * cSin;
			builder[1, 1] = aCos * cCos - aSin * bCos * cSin;
			builder[1, 2] = bSin * cSin;
			builder[2, 0] = aCos * bSin;
			builder[2, 1] = aSin * bSin;
			builder[2, 2] = bCos;
			return builder.ToMatrix();
		}

		public static Matrix<double, D3, D3> GetRotation(Vector<double, D3> axisDirection, Angle phi)
		{
			axisDirection = axisDirection.ToUnit();
			double sin = phi.Sin();
			double cos = phi.Cos();
			double ncos = 1.0 - cos;
			double n1 = axisDirection[0];
			double n1_2 = n1 * n1;
			double n2 = axisDirection[1];
			double n2_2 = n2 * n2;
			double n3 = axisDirection[2];
			double n3_2 = n3 * n3;
			var builder = new Matrix<double, D3, D3>.Builder();
			builder[0, 0] = n1_2 * ncos + cos;
			builder[0, 1] = n1 * n2 * ncos + n3 * sin;
			builder[0, 2] = n1 * n3 * ncos + n2 * sin;
			builder[1, 0] = n2 * n1 * ncos + n3 * sin;
			builder[1, 1] = n2_2 * ncos + cos;
			builder[1, 2] = n2 * n2 * ncos - n1 * sin;
			builder[2, 0] = n3 * n1 * ncos - n2 * sin;
			builder[2, 1] = n3 * n2 * ncos + n1 * sin;
			builder[2, 2] = n3_2 * ncos + cos;
			return builder.ToMatrix();
		}

		public static Matrix<T, ColDim, RowDim> Multiply<T, ColDim, Dim, RowDim>(Matrix<T, ColDim, Dim> a, Matrix<T, Dim, RowDim> b)
			where RowDim : IDimensionProvider, new()
			where ColDim : IDimensionProvider, new()
			where Dim : IDimensionProvider, new()
		{
			if (a.IsZero || b.IsZero)
				return new Matrix<T, ColDim, RowDim>();

			int RowCount = Matrix<T, ColDim, RowDim>.RowCount;
			int ColumnCount = Matrix<T, ColDim, RowDim>.ColumnCount;
			int DimCount = Matrix<T, ColDim, Dim>.RowDimension;

			// Naive matrix multiplication
			var result = new Matrix<T, ColDim, RowDim>.Builder();
			for (int row = 0; row < RowCount; row++)
			{
				for (int col = 0; col < ColumnCount; col++)
				{
					T val = Scalar<T>.Zero;
					for (int d = 0; d < DimCount; d++)
					{
						val = Scalar<T>.Add(val, Scalar<T>.Multiply(a[row, d], b[d, col]));
					}
					result[row, col] = val;
				}
			}
			return result.ToMatrix();
		}

		public static Matrix<T, Dim, Dim> Pow<T, Dim>(Matrix<T, Dim, Dim> matrix, int exponent)
			where Dim : IDimensionProvider, new()
		{
			if (matrix.IsZero)
				return matrix;
			var result = matrix;
			for (int i = 1; i < exponent; i++)
				result = Matrix.Multiply(result, matrix);
			return result;
		}
	}
}
