using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace ArgusLib
{
	public sealed class WeakDelegate<T> where T : class
	{
		static readonly Type ReturnType = InitializeReturnType();
		static readonly Type[] ParameterTypes = InitializeParameterTypes();
		static readonly Type OpenInstanceDelegateType = GetOpenInstanceDelgateType();

		InvocationListItem[] _invocationList = new InvocationListItem[0];
		readonly object _lock = new object();
		int _activeSubscriberCount = 0;

		object Lock => _lock;

		public WeakDelegate(T @delegate)
		{
			Proxy = CreateProxy(this);
			this.Add(@delegate);
		}

		InvocationListItem[] GetInvocationList() => Interlocked.CompareExchange(ref _invocationList, _invocationList ?? new InvocationListItem[0], null);
		void SetInvocationList(InvocationListItem[] value) => Interlocked.Exchange(ref _invocationList, value ?? new InvocationListItem[0]);

		public int ActiveSubscriberCount => Interlocked.CompareExchange(ref _activeSubscriberCount, 0, 0);

		void SetActiveSubscriberCount(int count) => Interlocked.Exchange(ref _activeSubscriberCount, count);

		public T Proxy { get; }

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
				SetActiveSubscriberCount(newList.Count);
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
				SetActiveSubscriberCount(newList.Count);
			}
		}

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

			ParameterExpression[] parameters = new ParameterExpression[ParameterTypes.Length];
			for (int i = 0; i < parameters.Length; i++)
				parameters[i] = Expression.Parameter(ParameterTypes[i], "arg" + i.ToStringInvariant());
		
			var loopBreak = Expression.Label(typeof(void), "loopBreak");
			var index = Expression.Variable(typeof(int), "index");
			var invocationList = Expression.Variable(typeof(InvocationListItem[]), "invocationList");
			var item = Expression.Variable(typeof(InvocationListItem), "item");
			var target = Expression.Variable(typeof(object), "target");
			var method = Expression.Variable(OpenInstanceDelegateType, "method");
			var activeCount = Expression.Variable(typeof(int), "activeCount");
			var weakDel = Expression.Constant(weakDelegate);

			var getInvocationListCall = Expression.Call(weakDel, new Func<InvocationListItem[]>(weakDelegate.GetInvocationList).GetMethodInfo());
			var getTargetCall = Expression.Call(item, "GetTarget", null);
			var setActiveCount = Expression.Call(weakDel, new Action<int>(weakDelegate.SetActiveSubscriberCount).GetMethodInfo(), activeCount);
			var invokeMethod =  Expression.Invoke(method, Enumerable.Repeat(target, 1).Concat(parameters));

			Expression<T> proxy;
			if (ReturnType != typeof(void))
			{
				var retVal = Expression.Variable(ReturnType, "retVal");
				proxy = Expression.Lambda<T>(
					Expression.Block(
						new[] { retVal, invocationList, index, activeCount },
						// TRetVal retVal = default(TRetVal);
						Expression.Assign(retVal, Expression.Default(ReturnType)),
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
									Expression.Assign(method, Expression.Convert(Expression.Property(item, "Method"), OpenInstanceDelegateType)),
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
									Expression.Assign(method, Expression.Convert(Expression.Property(item, "Method"), OpenInstanceDelegateType)),
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

			ParameterExpression[] parameters = new ParameterExpression[ParameterTypes.Length + 1];
			parameters[0] = Expression.Parameter(typeof(object), "target");
			for (int i = 1; i < parameters.Length; i++)
				parameters[i] = Expression.Parameter(ParameterTypes[i - 1], "arg" + i.ToStringInvariant());
			var returnLabel = Expression.Label(ReturnType);

			MethodInfo methodInfo = subscriber.GetMethodInfo();

			var method = Expression.Lambda(OpenInstanceDelegateType,
				Expression.Call(methodInfo.IsStatic ? null : Expression.Convert(parameters[0], methodInfo.DeclaringType),
					methodInfo, parameters.Skip(1)),
				"OpenInstanceDelegate",
				parameters);

			return new InvocationListItem(method.Compile(), methodInfo, methodInfo.IsStatic ? null : subscriber.Target);
		}

		static Type GetOpenInstanceDelgateType()
		{
			return Expression.GetDelegateType(Enumerable.Repeat(typeof(object), 1).Concat(ParameterTypes).Concat(Enumerable.Repeat(ReturnType, 1)).ToArray());
		}

		static Type InitializeReturnType()
		{
			MethodInfo invoke = typeof(T).GetTypeInfo().GetDeclaredMethod("Invoke");
			if (invoke == null)
				throw new GenericTypeParameterNotSupportetException<T>(new Exception($"{typeof(T).FullName} does not declare a public method named Invoke"));

			return invoke.ReturnType;
		}

		static Type[] InitializeParameterTypes()
		{
			MethodInfo invoke = typeof(T).GetTypeInfo().GetDeclaredMethod("Invoke");
			if (invoke == null)
				throw new GenericTypeParameterNotSupportetException<T>(new Exception($"{typeof(T).FullName} does not declare a public method named Invoke"));
			var parameters = invoke.GetParameters();
			Type[] types = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				types[i] = parameters[i].ParameterType;
			return types;
		}

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
	}
}
