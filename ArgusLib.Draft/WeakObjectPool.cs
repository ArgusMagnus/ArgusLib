#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;

namespace ArgusLib
{
    public class WeakObjectPool<TKey, TObj> where TObj : class
    {
		readonly Dictionary<TKey, Stack<WeakReference<TObj>>> _dict = new Dictionary<TKey, Stack<WeakReference<TObj>>>();
		readonly Func<TKey, TObj> _factory;
		readonly Action<TObj> _reset;

		public WeakObjectPool(Func<TKey, TObj> factory, Action<TObj> reset = null)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_reset = reset;
		}

		public Item RequestObject(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			return new Item(this, key, Take(key) ?? _factory(key));
		}

		TObj Take(TKey key)
		{
			Stack<WeakReference<TObj>> refs;
			lock (_dict)
			{
				if (!_dict.TryGetValue(key, out refs))
					return null;
			}

			TObj obj = null;
			lock (refs)
			{
				while (refs.Count > 0)
				{
					var objRef = refs.Pop();
					if (objRef.TryGetTarget(out obj))
						break;
				}

				if (refs.Count == 0)
				{
					lock(_dict)
					{
						_dict.Remove(key);
					}
				}
			}
			if (obj != null && _reset != null)
				_reset(obj);
			return obj;
		}

		void Insert(TKey key, TObj obj)
		{
			Stack<WeakReference<TObj>> refs;
			lock(_dict)
			{
				if (!_dict.TryGetValue(key, out refs))
				{
					refs = new Stack<WeakReference<TObj>>();
					_dict.Add(key, refs);
				}
			}

			lock (refs)
			{
				refs.Push(new WeakReference<TObj>(obj));
			}
		}

		public struct Item : IDisposable
		{
			readonly WeakObjectPool<TKey, TObj> _parent;
			readonly TKey _key;
			TObj _obj;

			public TKey Key => _key;
			public TObj Object => _obj;

			internal Item(WeakObjectPool<TKey, TObj> parent, TKey key, TObj obj)
			{
				_parent = parent;
				_key = key;
				_obj = obj;
			}

			public void Dispose()
			{
				if (_obj == null)
					return;
				_parent.Insert(_key, _obj);
				_obj = null;
			}
		}
    }
}
