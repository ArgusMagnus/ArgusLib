using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ArgusLib
{

	public sealed class WeakDelegate<T> where T : class
	{
		static WeakDelegate()
		{
			var handlerType = typeof(T).GetTypeInfo();
			if (!typeof(Delegate).GetTypeInfo().IsAssignableFrom(handlerType) || handlerType.IsAbstract)
				throw new GenericTypeParameterNotSupportetException<T>();
		}

		readonly List<InvocationListItem> _invocationList = new List<InvocationListItem>(0);
		readonly WeakReference<T> _delegate;

		object Lock => _invocationList;
		object StaticTarget => _invocationList;

		public WeakDelegate(T @delegate)
		{
			_delegate = new WeakReference<T>(@delegate);
			this.Add(@delegate);
		}

		public T Target
		{
			get
			{
				T retVal;
				if (_delegate.TryGetTarget(out retVal))
					return retVal;

				lock (Lock)
				{
					Delegate del = null;
					for (int i = 0; i < _invocationList.Count; i++)
					{
						object target;
						if (_invocationList[i].Target.TryGetTarget(out target))
						{
							if (object.ReferenceEquals(target, StaticTarget))
								target = null;

							del = Delegate.Combine(del, _invocationList[i].MethodInfo.CreateDelegate(typeof(T), target));
						}
						else
						{
							_invocationList.RemoveAt(i);
							i--;
						}
					}
					_invocationList.Capacity = _invocationList.Count;

					retVal = del as T;
				}

				_delegate.SetTarget(retVal);
				return retVal;
			}
		}

		public void Add(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			var invocationList = d?.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
				return;

			_delegate.SetTarget(null);

			lock (Lock)
			{
				_invocationList.Capacity += invocationList.Length;
				for (int i = 0; i < invocationList.Length; i++)
				{
					var item = new InvocationListItem(invocationList[i].GetMethodInfo(), invocationList[i].Target ?? StaticTarget);
					_invocationList.Add(item);
				}
			}
		}

		public void Remove(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			if (d == null)
				return;

			_delegate.SetTarget(null);
			List<Delegate> invocationsToRemove = new List<Delegate>(d.GetInvocationList());

			lock(Lock)
			{
				for (int i = 0; i < _invocationList.Count; i++)
				{
					object target;
					if (!_invocationList[i].Target.TryGetTarget(out target))
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
						if (_invocationList[i].MethodInfo.Equals(rem.GetMethodInfo()) && object.ReferenceEquals(target, rem.Target))
						{
							_invocationList.RemoveAt(i);
							i--;
							invocationsToRemove.RemoveAt(k);
							break;
						}
					}
				}
				_invocationList.Capacity = _invocationList.Count;
			}
		}

		struct InvocationListItem
		{
			public MethodInfo MethodInfo { get; }
			public WeakReference<object> Target { get; }

			public InvocationListItem(MethodInfo methodInfo, object target)
			{
				MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
				Target = new WeakReference<object>(target);
			}
		}
	}
}
