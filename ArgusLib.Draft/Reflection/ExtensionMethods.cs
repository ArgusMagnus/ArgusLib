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
using System.Linq.Expressions;
using System.Reflection;

namespace ArgusLib.Reflection
{
	public static class ExtensionMethods
	{
		static BindingAttributes GetBindingAttributesCore(MemberInfo member)
		{
			if (member is PropertyInfo)
				return GetBindingAttributesCore(((PropertyInfo)member).GetMethod);
			BindingAttributes attributes = new BindingAttributes();
			dynamic dynMember = member;
			if (dynMember.IsStatic)
				attributes |= BindingAttributes.Static;
			else
				attributes |= BindingAttributes.Instance;
			if (dynMember.IsPublic)
				attributes |= BindingAttributes.Public;
			else
				attributes |= BindingAttributes.NonPublic;
			return attributes;
		}

		public static BindingAttributes GetBindingAttributes(this EventInfo info) { return GetBindingAttributesCore(info); }
		public static BindingAttributes GetBindingAttributes(this FieldInfo info) { return GetBindingAttributesCore(info); }
		public static BindingAttributes GetBindingAttributes(this MethodBase info) { return GetBindingAttributesCore(info); }
		public static BindingAttributes GetBindingAttributes(this PropertyInfo info) { return GetBindingAttributesCore(info); }

		public static IEnumerable<TMemberInfo> GetRuntimeMembers<TMemberInfo>(this Type type, string name = null, BindingAttributes bindingAttr = BindingAttributes.Public | BindingAttributes.Instance | BindingAttributes.Static, bool ignoreCase = false)
			where TMemberInfo : MemberInfo
		{
			IEnumerable<TMemberInfo> members;
			if (typeof(TMemberInfo) == typeof(EventInfo))
				members = type.GetRuntimeEvents() as IEnumerable<TMemberInfo>;
			else if (typeof(TMemberInfo) == typeof(FieldInfo))
				members = type.GetRuntimeFields() as IEnumerable<TMemberInfo>;
			else if (typeof(TMemberInfo) == typeof(MethodInfo))
				members = type.GetRuntimeMethods() as IEnumerable<TMemberInfo>;
			else if (typeof(TMemberInfo) == typeof(PropertyInfo))
				members = type.GetRuntimeProperties() as IEnumerable<TMemberInfo>;
			else
				throw new GenericTypeParameterNotSupportetException<TMemberInfo>();

			StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			foreach (TMemberInfo member in members)
			{
				if (!string.IsNullOrEmpty(name) && !string.Equals(member.Name, name, stringComparison))
					continue;
				if (!EnumEx.HasFlag(bindingAttr, GetBindingAttributesCore(member)))
					continue;
				yield return member;
			}
		}

		public static TMemberInfo GetRuntimeMember<TMemberInfo>(this Type type, string name, BindingAttributes bindingAttr = BindingAttributes.Public | BindingAttributes.Instance | BindingAttributes.Static, bool ignoreCase = false)
			where TMemberInfo : MemberInfo
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			foreach (TMemberInfo member in type.GetRuntimeMembers<TMemberInfo>(name, bindingAttr, ignoreCase))
				return member;
			return null;
		}

		public static MethodInfo GetRuntimeMethod(this Type type, string name, BindingAttributes bindingAttr = BindingAttributes.Public | BindingAttributes.Instance | BindingAttributes.Static, Type returnType = null, IEnumerable<Type> parameterTypes = null, bool ignoreCase = false)
		{
			StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			foreach (MethodInfo method in type.GetRuntimeMethods())
			{
				if (!string.Equals(method.Name, name, stringComparison))
					continue;
				if (returnType != null && method.ReturnType != returnType)
					continue;
				if (!EnumEx.HasFlag(bindingAttr, method.GetBindingAttributes()))
					continue;
				if (parameterTypes != null)
				{
					ParameterInfo[] parInfo = method.GetParameters();
					int i = 0;
					foreach (Type parType in parameterTypes)
					{
						if (parType != parInfo[i++].ParameterType)
						{
							i = -1;
							break;
						}
					}
					if (i < 0)
						continue;
				}
				return method;
			}
			return null;
		}

		public static bool IsSignatureValid(this MethodBase method, TypeInfo[] parameterTypes, TypeInfo returnType = null, bool exactMatch = false)
		{
			if (returnType != null && method is MethodInfo)
			{
				TypeInfo methodReturnType = ((MethodInfo)method).ReturnType.GetTypeInfo();
				if (exactMatch && returnType != methodReturnType)
					return false;
				if (!returnType.IsAssignableFrom(methodReturnType))
					return false;
			}

			if (parameterTypes == null)
				parameterTypes = new TypeInfo[0];
			ParameterInfo[] parInfos = method.GetParameters();
			if (parInfos.Length != parameterTypes.Length)
				return false;
			IEnumerable<TypeInfo> paras = parInfos.Select((x) => x.ParameterType.GetTypeInfo());
			int i = 0;
			foreach (TypeInfo parType in paras)
			{
				if (exactMatch && parType != parameterTypes[i])
					return false;
				if (!parType.IsAssignableFrom(parameterTypes[i]))
					return false;
				i++;
			}
			return true;
		}

		public static IEnumerable<TypeInfo> GetInterfaceImplementors(this Assembly assembly, Type interfaceType, bool includeNonPublic = false, bool includeAbstract = false, Type[] constructorParameters = null)
		{
			return assembly.GetInterfaceImplementors(interfaceType.GetTypeInfo(), includeNonPublic, includeAbstract, constructorParameters?.Select((x) => x.GetTypeInfo()).ToArray());
		}

		public static IEnumerable<TypeInfo> GetInterfaceImplementors(this Assembly assembly, TypeInfo interfaceType, bool includeNonPublic = false, bool includeAbstract = false, TypeInfo[] constructorParameters = null)
		{
			if (assembly == null || interfaceType == null)
				throw new ArgumentNullException();
			if (!interfaceType.IsInterface)
				throw new ArgumentException($"{nameof(interfaceType)} must be an interface type.");
			foreach (TypeInfo type in assembly.DefinedTypes)
			{
				if (type.IsInterface)
					continue;
				if (!includeNonPublic && !type.IsPublic)
					continue;
				if (!includeAbstract && type.IsAbstract)
					continue;
				if (!interfaceType.IsAssignableFrom(type))
					continue;
				if (constructorParameters != null && !type.DeclaredConstructors.Any((ci) => ci.IsSignatureValid(constructorParameters)))
					continue;

				yield return type;
			}
		}

		static public Func<TObj, TValue> CreateGetDelegate<TObj, TValue>(this FieldInfo fieldInfo)
		{
			var instExp = Expression.Parameter(typeof(TObj));
			var fieldExp = Expression.Field(instExp, fieldInfo);
			return Expression.Lambda<Func<TObj, TValue>>(fieldExp, instExp).Compile();
		}
	}
}
