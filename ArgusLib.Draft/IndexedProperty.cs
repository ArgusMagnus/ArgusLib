#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ArgusLib
{
	public abstract class IndexedCollection<T> : IEnumerable<T>
	{
		public abstract int Count { get; }
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				return this.GetCore(index);
			}
			set
			{
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				this.SetCore(index, value);
			}
		}
		protected abstract T GetCore(int index);
		protected abstract void SetCore(int index, T value);

		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return new Enumerator(this); }
		IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

		public void CopyTo(T[] array, int startIndex)
		{
			for (int i = 0; i < this.Count; i++)
				array[i + startIndex] = this[i];
		}

		public T[] ToArray()
		{
			T[] RetVal = new T[this.Count];
			this.CopyTo(RetVal, 0);
			return RetVal;
		}

		class Enumerator : IEnumerator<T>
		{
			IndexedCollection<T> parent;
			int index;

			public Enumerator(IndexedCollection<T> parent)
			{
				this.parent = parent;
				this.Reset();
			}

			public void Reset()
			{
				this.index = -1;
			}

			public bool MoveNext()
			{
				this.index++;
				return this.index < this.parent.Count;
			}

			public T Current { get { return this.parent[this.index]; } }
			object IEnumerator.Current { get { return this.Current; } }

			void IDisposable.Dispose() { }
		}
	}

	public abstract class IndexedReadOnlyCollection<T> : IEnumerable<T>
	{
		public abstract int Count { get; }
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				return this.GetCore(index);
			}
		}
		protected abstract T GetCore(int index);

		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return new Enumerator(this); }
		IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

		public void CopyTo(T[] array, int startIndex)
		{
			for (int i = 0; i < this.Count; i++)
				array[i + startIndex] = this[i];
		}

		public T[] ToArray()
		{
			T[] RetVal = new T[this.Count];
			this.CopyTo(RetVal, 0);
			return RetVal;
		}

		class Enumerator : IEnumerator<T>
		{
			IndexedReadOnlyCollection<T> parent;
			int index;

			public Enumerator(IndexedReadOnlyCollection<T> parent)
			{
				this.parent = parent;
				this.Reset();
			}

			public void Reset()
			{
				this.index = -1;
			}

			public bool MoveNext()
			{
				this.index++;
				return this.index < this.parent.Count;
			}

			public T Current { get { return this.parent[this.index]; } }
			object IEnumerator.Current { get { return this.Current; } }

			void IDisposable.Dispose() { }
		}
	}

	public sealed class IndexedProperty<TProperty> : IndexedCollection<TProperty>
	{
		readonly Func<int, TProperty> _get;
		readonly Action<int, TProperty> _set;
		readonly Func<int> _getCount;

		public IndexedProperty(Func<int,TProperty> get, Action<int,TProperty> set, Func<int> getCount)
		{
			_get = get;
			_set = set;
			_getCount = getCount;
		}

		public override int Count { get { return _getCount(); } }
		protected override TProperty GetCore(int index) => _get(index);
		protected override void SetCore(int index, TProperty value) => _set(index, value);
	}

	public sealed class IndexedReadOnlyProperty<TProperty> : IndexedReadOnlyCollection<TProperty>
	{
		readonly Func<int, TProperty> _get;
		readonly Func<int> _getCount;

		public IndexedReadOnlyProperty(Func<int, TProperty> get, Func<int> getCount)
		{
			_get = get;
			_getCount = getCount;
		}

		public override int Count { get { return _getCount(); } }
		protected override TProperty GetCore(int index) => _get(index);
	}

	public class IndexedProperty<TIndex, TProperty>
	{
		readonly Func<TIndex,TProperty> _get;
		readonly Action<TIndex,TProperty> _set;

		public IndexedProperty(Func<TIndex, TProperty> get, Action<TIndex, TProperty> set)
		{
			this._get = get;
			this._set = set;
		}

		public TProperty this[TIndex index]
		{
			get { return this._get(index); }
			set { this._set(index, value); }
		}
	}

	public class IndexedProperty<TIndex1, TIndex2, TProperty>
	{
		readonly Func<TIndex1,TIndex2,TProperty> _get;
		readonly Action<TIndex1,TIndex2,TProperty> _set;

		public IndexedProperty(Func<TIndex1, TIndex2, TProperty> get, Action<TIndex1, TIndex2, TProperty> set)
		{
			this._get = get;
			this._set = set;
		}

		public TProperty this[TIndex1 index1, TIndex2 index2]
		{
			get { return this._get(index1, index2); }
			set { this._set(index1, index2, value); }
		}
	}

	public class IndexedProperty<TIndex1, TIndex2, TIndex3, TProperty>
	{
		readonly Func<TIndex1,TIndex2,TIndex3,TProperty> _get;
		readonly Action<TIndex1, TIndex2, TIndex3, TProperty> _set;

		public IndexedProperty(Func<TIndex1, TIndex2, TIndex3, TProperty> get, Action<TIndex1, TIndex2, TIndex3, TProperty> set)
		{
			this._get = get;
			this._set = set;
		}

		public TProperty this[TIndex1 index1, TIndex2 index2, TIndex3 index3]
		{
			get { return this._get(index1, index2, index3); }
			set { this._set(index1, index2, index3, value); }
		}
	}

	public class IndexedReadOnlyProperty<TIndex, TProperty>
	{
		readonly Func<TIndex,TProperty> _get;

		public IndexedReadOnlyProperty(Func<TIndex,TProperty> get)
		{
			this._get = get;
		}

		public TProperty this[TIndex index]
		{
			get { return this._get(index); }
		}
	}

	public class IndexedReadOnlyProperty<TIndex1, TIndex2, TProperty>
	{
		readonly Func<TIndex1,TIndex2,TProperty> _get;

		public IndexedReadOnlyProperty(Func<TIndex1, TIndex2, TProperty> get)
		{
			this._get = get;
		}

		public TProperty this[TIndex1 index1, TIndex2 index2]
		{
			get { return this._get(index1, index2); }
		}
	}

	public class IndexedReadOnlyProperty<TProperty, TIndex1, TIndex2, TIndex3>
	{
		readonly Func<TIndex1,TIndex2,TIndex3,TProperty> _get;

		public IndexedReadOnlyProperty(Func<TIndex1, TIndex2, TIndex3, TProperty> get)
		{
			this._get = get;
		}

		public TProperty this[TIndex1 index1, TIndex2 index2, TIndex3 index3]
		{
			get { return this._get(index1, index2, index3); }
		}
	}
}
