#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using ArgusLib.Diagnostics.Tracing;

namespace ArgusLib
{
	/// <summary>
	/// Holds a <see cref="WeakReference{T}"/> to a object of type <typeparamref name="T"/>
	/// and initializes the object each time <see cref="Value"/> is accessed and <see cref="WeakReference{T}.TryGetTarget(out T)"/>
	/// returns <c>false</c>.
	/// </summary>
	public class LazyWeakReference<T> where T : class
	{
		WeakReference<T> _weakRef = new WeakReference<T>(default(T));
		Func<T> _create;

		public LazyWeakReference(Func<T> valueFactory)
		{
			_create = valueFactory ?? throw Tracer.ThrowCritical<LazyWeakReference<T>>(new ArgumentNullException(nameof(valueFactory)));
		}

		public T Get()
		{
			T RetVal;
			if (_weakRef.TryGetTarget(out RetVal))
				return RetVal;
			lock (_weakRef)
			{
				if (_weakRef.TryGetTarget(out RetVal))
					return RetVal;
				RetVal = _create();
				_weakRef.SetTarget(RetVal);
				return RetVal;
			}
		}
	}
}
