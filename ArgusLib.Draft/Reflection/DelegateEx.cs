#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ArgusLib.Reflection;

namespace ArgusLib
{
	namespace Internal
	{
		public abstract class DelegateExCore<T> where T : class
		{
			internal DelegateExCore() { }

			public static TDelegate Create<TDelegate>(MethodInfo method, object target = null)
				where TDelegate : class, T
			{
				return method.CreateDelegate(typeof(TDelegate), target) as TDelegate;
			}

			public static TDelegate Combine<TDelegate>(TDelegate a, TDelegate b)
				where TDelegate : class, T
			{
				return Delegate.Combine(a as Delegate, b as Delegate) as TDelegate;
			}

			public static TDelegate Remove<TDelegate>(TDelegate source, TDelegate value)
				where TDelegate : class, T
			{
				return Delegate.Remove(source as Delegate, value as Delegate) as TDelegate;
			}

			public static TDelegate Create<TDelegate, TDefiningType>(string methodName, BindingAttributes bindingAttributes = BindingAttributes.Public | BindingAttributes.Instance | BindingAttributes.Static, TDefiningType target = null, bool ignoreCase = false)
				where TDelegate : class, T
				where TDefiningType : class
			{
				MethodInfo delegateMethod = typeof(TDelegate).GetRuntimeMethod("Invoke", BindingAttributes.Public | BindingAttributes.Instance);
				IEnumerable<Type> delParTypes = delegateMethod.GetParameters().Select((par) => par.ParameterType);
				if (target == null)
					bindingAttributes = EnumEx.RemoveFlag(bindingAttributes, BindingAttributes.Static);

				MethodInfo method = typeof(TDefiningType).GetRuntimeMethod(methodName, bindingAttributes, delegateMethod.ReturnType, delParTypes, ignoreCase);
				if (method == null)
					return default(TDelegate);

				return method.CreateDelegate(typeof(TDelegate), target) as TDelegate;
			}

			public static TDelegate[] GetInvocationList<TDelegate>(TDelegate source)
				where TDelegate : class, T
			{
				Delegate[] list = (source as Delegate).GetInvocationList();
				TDelegate[] RetVal = new TDelegate[list.Length];
				for (int i = 0; i < list.Length; i++)
					RetVal[i] = list[i] as TDelegate;
				return RetVal;
			}
		}
	}

	public sealed class DelegateEx : Internal.DelegateExCore<MulticastDelegate>
	{
		DelegateEx() { }
	}
}