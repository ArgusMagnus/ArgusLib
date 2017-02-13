﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ArgusLib
{
	public sealed class WeakDelegateReference : Internal.WeakDelegateReferenceCore<MulticastDelegate>
	{
		WeakDelegateReference()
			: base() { }
	}

	public sealed class WeakDelegateReference<T> where T : class
	{
		readonly List<Tuple<MethodInfo, WeakReference<object>>> _invocationList;
		readonly WeakReference<T> _delegate;

		object Lock => _invocationList;
		object StaticTarget => _invocationList;

		internal WeakDelegateReference(T @delegate)
		{
			_delegate = new WeakReference<T>(@delegate);
			_invocationList = new List<Tuple<MethodInfo, WeakReference<object>>>(0);
			this.Add(@delegate);
		}

		public T Get()
		{
			T retVal;

			lock (Lock)
			{
				if (_delegate.TryGetTarget(out retVal))
					return retVal;

				Delegate del = null;
				for (int i = 0; i < _invocationList.Count; i++)
				{
					object target;
					if (_invocationList[i].Item2.TryGetTarget(out target))
					{
						if (object.ReferenceEquals(target, StaticTarget))
							target = null;

						del = Delegate.Combine(del, _invocationList[i].Item1.CreateDelegate(typeof(T), target));
					}
					else
					{
						_invocationList.RemoveAt(i);
						i--;
					}
				}
				_invocationList.Capacity = _invocationList.Count;

				retVal = del as T;
				_delegate.SetTarget(retVal);
			}

			return retVal;
		}

		public void Add(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			var invocationList = d?.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
				return;

			lock(Lock)
			{
				_delegate.SetTarget(null);
				_invocationList.Capacity += invocationList.Length;
				for (int i = 0; i < invocationList.Length; i++)
				{
					var item = new Tuple<MethodInfo, WeakReference<object>>(invocationList[i].GetMethodInfo(), new WeakReference<object>(invocationList[i].Target ?? StaticTarget));
					_invocationList.Add(item);
				}
			}
		}

		public void Remove(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			if (d == null)
				return;
			List<Delegate> invocationsToRemove = new List<Delegate>(d.GetInvocationList());

			lock(Lock)
			{
				_delegate.SetTarget(null);
				for (int i = 0; i < _invocationList.Count; i++)
				{
					MethodInfo method = _invocationList[i].Item1;
					object target;
					if (!_invocationList[i].Item2.TryGetTarget(out target))
					{
						_invocationList.RemoveAt(i);
						i--;
						continue;
					}
					if (object.ReferenceEquals(target, StaticTarget))
						target = null;

					for (int k = 0; k < invocationsToRemove.Count; k++)
					{
						Delegate rem = invocationsToRemove[k];
						if (method.Equals(rem.GetMethodInfo()) && object.ReferenceEquals(target, rem.Target))
						{
							_invocationList.RemoveAt(i);
							i--;
							invocationsToRemove.RemoveAt(k);
							break;
						}
					}
				}
			}
		}
	}

	namespace Internal
	{
		public abstract class WeakDelegateReferenceCore<T> where T : class
		{
			static WeakDelegateReferenceCore()
			{
				if (typeof(T) != typeof(MulticastDelegate))
					throw new GenericTypeParameterNotSupportetException<T>();
			}

			internal protected WeakDelegateReferenceCore() { }

			public static WeakDelegateReference<TDelegate> Create<TDelegate>(TDelegate @delegate)
				where TDelegate : class, T
			{
				return new WeakDelegateReference<TDelegate>(@delegate);
			}
		}
	}
}
