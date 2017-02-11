#region Copyright and License
/*
This file is part of ArgusLib.Runtime.InteropServices
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Runtime.InteropServices;

namespace ArgusLib.Runtime.InteropServices
{
	public class AlignedArray<T> : IPinnedArray<T>
		where T : struct
	{
		readonly PinnedGCHandle<byte[]> _pin;
		readonly int _alignment;
		readonly IntPtr _alignedPtr;
		readonly int _length;
		readonly int[] _lengths;

		public int Length => _length;
		public bool IsDisposed => !_pin.IsAllocated;
		public int Rank => _lengths.Length;

		public IntPtr Pointer => _alignedPtr;

		public AlignedArray(byte[] buffer, int alignment, params int[] lengths)
		{
			_alignment = alignment;
			_length = 1;
			checked
			{
				foreach (var n in lengths)
					_length *= n;
			}
			_lengths = lengths;

			if (_length > buffer.Length / Marshal.SizeOf<T>())
				throw new ArgumentException($"Buffer is to small to hold array of size {nameof(lengths)}", nameof(buffer));

			_pin = PinnedGCHandle<byte[]>.Pin(buffer);

			long value = _pin.Pointer.ToInt64();
			int offset = alignment - (int)(value % alignment);
			_alignedPtr = new IntPtr(value + offset);
			int maxLength = (buffer.Length - offset) / Marshal.SizeOf<T>();

			if (_length > maxLength)
			{
				_pin.Free();
				throw new ArgumentException($"Buffer is to small to hold array of size {nameof(lengths)}", nameof(buffer));
			}
		}

		public AlignedArray(int alignment, params int[] lengths)
		: this(GetBuffer(alignment, lengths), alignment, lengths) { }

		static byte[] GetBuffer(int alignment, int[] lengths)
		{
			long length = Marshal.SizeOf<T>();
			foreach (var n in lengths)
				length *= n;
			return new byte[length + alignment];
		}

		public void Dispose() => _pin.Free();

		public int GetLength(int dimension) => _lengths[dimension];

		public int[] GetSize()
		{
			int[] result = new int[Rank];
			Buffer.BlockCopy(_lengths, 0, result, 0, Rank * sizeof(int));
			return result;
		}

		protected virtual T GetCore(IntPtr ptr) => Marshal.PtrToStructure<T>(ptr);
		protected virtual void SetCore(T value, IntPtr ptr) => Marshal.StructureToPtr<T>(value, ptr, false);

		void VerifyRank(int rank)
		{
			if (rank != this.Rank)
				throw new InvalidOperationException($"Dimension mismatch: Rank is not {Rank}");
		}

		void VerifyNotDisposed()
		{
			if (!_pin.IsAllocated)
				throw new ObjectDisposedException(this.GetType().FullName);
		}

		public T this[int i1]
		{
			get
			{
				VerifyNotDisposed();
				VerifyRank(1);
				var ptr = this.Pointer + (i1 * Marshal.SizeOf<T>());
				return GetCore(ptr);
			}
			set
			{
				VerifyNotDisposed();
				VerifyRank(1);
				var ptr = this.Pointer + (i1 * Marshal.SizeOf<T>());
				SetCore(value, ptr);
			}
		}

		public T this[int i1, int i2]
		{
			get
			{
				VerifyNotDisposed();
				VerifyRank(2);
				var ptr = this.Pointer + (i2 + GetLength(1) * i1) * Marshal.SizeOf<T>();
				return GetCore(ptr);
			}
			set
			{
				VerifyNotDisposed();
				VerifyRank(2);
				var ptr = this.Pointer + (i2 + GetLength(1) * i1) * Marshal.SizeOf<T>();
				SetCore(value, ptr);
			}
		}

		public T this[int i1, int i2, int i3]
		{
			get
			{
				VerifyNotDisposed();
				VerifyRank(3);
				var ptr = this.Pointer + (i3 + GetLength(2) * (i2 + GetLength(1) * i1)) * Marshal.SizeOf<T>();
				return GetCore(ptr);
			}
			set
			{
				VerifyNotDisposed();
				VerifyRank(3);
				var ptr = this.Pointer + (i3 + GetLength(2) * (i2 + GetLength(1) * i1)) * Marshal.SizeOf<T>();
				SetCore(value, ptr);
			}
		}

		public T this[params int[] indices]
		{
			get
			{
				VerifyNotDisposed();
				VerifyRank(indices.Length);
				var ptr = new IntPtr(this.Pointer.ToInt64() + this.GetIndex(indices));
				return GetCore(ptr);
			}
			set
			{
				VerifyNotDisposed();
				VerifyRank(indices.Length);
				var ptr = new IntPtr(this.Pointer.ToInt64() + this.GetIndex(indices));
				SetCore(value, ptr);
			}
		}
	}
}
