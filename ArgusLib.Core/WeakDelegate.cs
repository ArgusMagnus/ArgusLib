using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Collections.Concurrent;

namespace ArgusLib
{
	public sealed class WeakDelegate<T> where T : class
	{
		#region Fields

		static readonly LazyWeakReference<Data> StaticData = new LazyWeakReference<Data>(() => new Data());

		readonly Data _data = StaticData.Get();

		/// <summary>
		/// Do not access directly, us <see cref="GetInvocationList"/>/<see cref="SetInvocationList(InvocationListItem[])"/> instead.
		/// </summary>
		InvocationListItem[] _invocationList = new InvocationListItem[0];

		/// <summary>
		/// Do not access directly, us <see cref="AliveSubscriberCount"/>/<see cref="SetAliveSubscriberCount(int)"/> instead.
		/// </summary>
		int _aliveSubscriberCount = 0;

		#endregion

		#region Constructor

		public WeakDelegate(T @delegate)
		{
			Proxy = CreateProxy(this);
			this.Add(@delegate);
		}

		#endregion

		#region Private Properties

		object Lock => _data;
		InvocationListItem[] GetInvocationList() => Interlocked.CompareExchange(ref _invocationList, _invocationList ?? new InvocationListItem[0], null);
		void SetInvocationList(InvocationListItem[] value) => Interlocked.Exchange(ref _invocationList, value ?? new InvocationListItem[0]);
		void SetAliveSubscriberCount(int count) => Interlocked.Exchange(ref _aliveSubscriberCount, count);

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns a delegate which calls each subscriber which has not yet been collected
		/// </summary>
		public T Proxy { get; }

		/// <summary>
		/// Returns the number of subscribers which have not been collected yet.
		/// This value is updated in each call to either <see cref="Add(T)"/>, <see cref="Remove(T)"/>,
		/// <see cref="CleanUp"/> or <see cref="Proxy"/>.
		/// </summary>
		public int AliveSubscriberCount => Interlocked.CompareExchange(ref _aliveSubscriberCount, 0, 0);

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds all delegates in the <paramref name="subscriber"/>'s invocation list to the
		/// <see cref="WeakDelegate{T}"/>'s invocation list and cleans up dead (collected) elements.
		/// </summary>
		public void Add(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			var invocationList = d?.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
				return;

			lock (Lock)
			{
				var oldList = GetInvocationList();
				List<InvocationListItem> newList = new List<InvocationListItem>(oldList.Length + invocationList.Length);
				foreach (var item in oldList)
				{
					if (item.IsStatic)
						newList.Add(item);
					object tmp;
					if (item.Target.TryGetTarget(out tmp))
						newList.Add(item);
				}
				foreach (var invoke in invocationList)
					newList.Add(GetListItem(invoke));
				SetInvocationList(newList.ToArray());
				SetAliveSubscriberCount(newList.Count);
			}
		}

		/// <summary>
		/// Removes all delegates in the <paramref name="subscriber"/>'s invocation list from the
		/// <see cref="WeakDelegate{T}"/>'s invocation list and cleans up dead (collected) elements.
		/// </summary>
		public void Remove(T subscriber)
		{
			Delegate d = subscriber as Delegate;
			if (d == null)
				return;

			List<Delegate> invocationsToRemove = new List<Delegate>(d.GetInvocationList());

			lock(Lock)
			{
				var oldList = GetInvocationList();
				List<InvocationListItem> newList = new List<InvocationListItem>(oldList.Length);
				foreach (var item in oldList)
				{
					object target = null;
					if (!item.IsStatic && !item.Target.TryGetTarget(out target))
						continue;

					bool removed = false;
					for (int i = 0; i < invocationsToRemove.Count; i++)
					{
						Delegate rem = invocationsToRemove[i];
						if (target == rem.Target && item.Id.Equals(rem.GetMethodInfo()))
						{
							invocationsToRemove.RemoveAt(i--);
							removed = true;
							break;
						}
					}
					if (!removed)
						newList.Add(item);
				}
				SetInvocationList(newList.ToArray());
				SetAliveSubscriberCount(newList.Count);
			}
		}

		/// <summary>
		/// Removes dead (collected) elements from the invocation list.
		/// </summary>
		public void CleanUp()
		{
			lock (Lock)
			{
				var oldList = GetInvocationList();
				List<InvocationListItem> newList = new List<InvocationListItem>(oldList.Length);
				foreach (var item in oldList)
				{
					if (item.IsStatic)
						newList.Add(item);
					object tmp;
					if (item.Target.TryGetTarget(out tmp))
						newList.Add(item);
				}
				SetInvocationList(newList.ToArray());
				SetAliveSubscriberCount(newList.Count);
			}
		}

		#endregion

		#region Helper Methods

		static T CreateProxy(WeakDelegate<T> weakDelegate)
		{
			/* Creates a delegate of the following form:
			 
			return (TArg1 arg1, TArg2 arg2, ...) =>
			{
				TRetVal retVal = default(TRetVal);
				InvocationListItem[] invocationList = weakDelegate.GetInvocationList();
				int activeCount = 0;
				for (int i = 0; i < invocationList.Length; i++)
				{
					InvocationListItem item = invocationList[i];
					object target = GetTarget(item);
					Delegate method = item.Method;
					if (item.IsStatic || target != null)
					{
						activeCount++;
						retVal = method(arg1, arg2, ...);
					}
				}
				weakDelegate.SetActiveSubscriberCount(activeCount);
				return retVal;
			}
			*/

			var data = weakDelegate._data;

			ParameterExpression[] parameters = new ParameterExpression[data.ParameterTypes.Length];
			for (int i = 0; i < parameters.Length; i++)
				parameters[i] = Expression.Parameter(data.ParameterTypes[i], "arg" + i.ToStringInvariant());
		
			var loopBreak = Expression.Label(typeof(void), "loopBreak");
			var index = Expression.Variable(typeof(int), "index");
			var invocationList = Expression.Variable(typeof(InvocationListItem[]), "invocationList");
			var item = Expression.Variable(typeof(InvocationListItem), "item");
			var target = Expression.Variable(typeof(object), "target");
			var method = Expression.Variable(data.OpenInstanceDelegateType, "method");
			var activeCount = Expression.Variable(typeof(int), "activeCount");
			var weakDel = Expression.Constant(weakDelegate);

			var getInvocationListCall = Expression.Call(weakDel, new Func<InvocationListItem[]>(weakDelegate.GetInvocationList).GetMethodInfo());
			var getTargetCall = Expression.Call(item, "GetTarget", null);
			var setActiveCount = Expression.Call(weakDel, new Action<int>(weakDelegate.SetAliveSubscriberCount).GetMethodInfo(), activeCount);
			var invokeMethod =  Expression.Invoke(method, Enumerable.Repeat(target, 1).Concat(parameters));

			Expression<T> proxy;
			if (weakDelegate._data.ReturnType != typeof(void))
			{
				var retVal = Expression.Variable(data.ReturnType, "retVal");
				proxy = Expression.Lambda<T>(
					Expression.Block(
						new[] { retVal, invocationList, index, activeCount },
						// TRetVal retVal = default(TRetVal);
						Expression.Assign(retVal, Expression.Default(data.ReturnType)),
						// InvocationListItem[] invocationList = GetInvocationList();
						Expression.Assign(invocationList, getInvocationListCall),
						// int activeCount = 0;
						Expression.Assign(activeCount, Expression.Constant(0)),
						// for (int i = 0; i < _invocationList.Count; i++)
						Expression.Assign(index, Expression.Constant(0)),
						Expression.Loop(
							Expression.IfThenElse(Expression.GreaterThanOrEqual(index, Expression.ArrayLength(invocationList)),
								Expression.Break(loopBreak),
								Expression.Block(
									new[] { item, target, method },
									// InvocationListItem item = invocationList[i];
									Expression.Assign(item, Expression.ArrayIndex(invocationList, index)),
									// object target = GetTarget(item);
									Expression.Assign(target, getTargetCall),
									// Delegate method = item.Method;
									Expression.Assign(method, Expression.Convert(Expression.Property(item, "Method"), data.OpenInstanceDelegateType)),
									// if (item.IsStatic || target != null)
									Expression.IfThen(Expression.Or(Expression.Property(item, "IsStatic"), Expression.ReferenceNotEqual(target, Expression.Constant((object)null))),
										Expression.Block(
											// retVal = method(arg1, arg2, ...);
											Expression.Assign(retVal, invokeMethod),
											// activeCount++;
											Expression.PostIncrementAssign(activeCount))
										),
									// index++;
									Expression.PostIncrementAssign(index)
								)
							),
							loopBreak
						),
						setActiveCount,
						retVal
					),
					"ProxyDelegate",
					parameters);
			}
			else
			{
				proxy = Expression.Lambda<T>(
					Expression.Block(
						new[] { invocationList, index, activeCount },
						// InvocationListItem[] invocationList = GetInvocationList();
						Expression.Assign(invocationList, getInvocationListCall),
						// int activeCount = 0;
						Expression.Assign(activeCount, Expression.Constant(0)),
						// for (int i = 0; i < _invocationList.Count; i++)
						Expression.Assign(index, Expression.Constant(0)),
						Expression.Loop(
							Expression.IfThenElse(Expression.GreaterThanOrEqual(index, Expression.ArrayLength(invocationList)),
								Expression.Break(loopBreak),
								Expression.Block(
									new[] { item, target, method },
									// InvocationListItem item = invocationList[i];
									Expression.Assign(item, Expression.ArrayIndex(invocationList, index)),
									// object target = GetTarget(item);
									Expression.Assign(target, getTargetCall),
									// Delegate method = item.Method;
									Expression.Assign(method, Expression.Convert(Expression.Property(item, "Method"), data.OpenInstanceDelegateType)),
									// if (item.IsStatic || target != null)
									Expression.IfThen(Expression.Or(Expression.Property(item, "IsStatic"), Expression.ReferenceNotEqual(target, Expression.Constant((object)null))),
										Expression.Block(
											// method(arg1, arg2, ...);
											invokeMethod,
											// activeCount++;
											Expression.PostIncrementAssign(activeCount))
										),
									// index++;
									Expression.PostIncrementAssign(index)
								)
							),
							loopBreak
						),
						setActiveCount
					),
					"ProxyDelegate",
					parameters);
			}
			return proxy.Compile();
		}

		static InvocationListItem GetListItem(Delegate subscriber)
		{
			/* Creates a delegate of the following form:
			
			Delegate method = (object target, TArg1 arg1, TArg2 arg2, ...) =>
			{
				subscriber.OpenInstanceMethod((TTarget)target, arg1, arg2, ...);
			};
			*/

			var data = StaticData.Get();
			var methodInfo = subscriber.GetMethodInfo();
			Delegate method;
			if (data.OpenInstanceDelegates.TryGetValue(methodInfo, out method))
				return new InvocationListItem(method, methodInfo, methodInfo.IsStatic ? null : subscriber.Target);


			ParameterExpression[] parameters = new ParameterExpression[data.ParameterTypes.Length + 1];
			parameters[0] = Expression.Parameter(typeof(object), "target");
			for (int i = 1; i < parameters.Length; i++)
				parameters[i] = Expression.Parameter(data.ParameterTypes[i - 1], "arg" + i.ToStringInvariant());

			method = Expression.Lambda(data.OpenInstanceDelegateType,
				Expression.Call(methodInfo.IsStatic ? null : Expression.Convert(parameters[0], methodInfo.DeclaringType),
					methodInfo, parameters.Skip(1)),
				"OpenInstanceDelegate",
				parameters).Compile();

			data.OpenInstanceDelegates.TryAdd(methodInfo, method);

			return new InvocationListItem(method, methodInfo, methodInfo.IsStatic ? null : subscriber.Target);
		}

		#endregion

		#region Helper Classes

		class InvocationListItem
		{
			public Delegate Method { get; }
			public MethodInfo Id { get; }
			public WeakReference<object> Target { get; }
			public bool IsStatic => Target == null;

			public InvocationListItem(Delegate method, MethodInfo id, object target)
			{
				Method = method ?? throw new ArgumentNullException(nameof(method));
				Id = id ?? throw new ArgumentNullException(nameof(id));
				Target = target == null ? null : new WeakReference<object>(target);
			}

			public object GetTarget()
			{
				if (IsStatic)
					return null;
				object tmp = null;
				Target.TryGetTarget(out tmp);
				return tmp;
			}
		}

		class Data
		{
			public Type ReturnType { get; }
			public Type[] ParameterTypes { get; }
			public Type OpenInstanceDelegateType { get; }
			public ConcurrentDictionary<MethodInfo, Delegate> OpenInstanceDelegates { get; }

			public Data()
			{
				MethodInfo invoke = typeof(T).GetTypeInfo().GetDeclaredMethod("Invoke");
				if (invoke == null)
					throw new GenericTypeParameterNotSupportetException<T>(new Exception($"{typeof(T).FullName} does not declare a public method named Invoke"));

				ReturnType = invoke.ReturnType;

				var parameters = invoke.GetParameters();
				ParameterTypes = new Type[parameters.Length];
				for (int i = 0; i < parameters.Length; i++)
					ParameterTypes[i] = parameters[i].ParameterType;

				OpenInstanceDelegateType = Expression.GetDelegateType(Enumerable.Repeat(typeof(object), 1).Concat(ParameterTypes).Concat(Enumerable.Repeat(ReturnType, 1)).ToArray());

				OpenInstanceDelegates = new ConcurrentDictionary<MethodInfo, Delegate>();
			}
		}

		#endregion

	}
}
