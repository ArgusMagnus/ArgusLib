#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using ArgusLib.Diagnostics.Tracing;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ArgusLib
{
	public static class ExceptionUtil
	{
		public static void Try(Action action, [CallerMemberName]string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			try { action(); }
			catch (Exception e) when (Tracer.ExceptionWarning(e, typeof(ExceptionUtil), true, callerMemberName, callerFilePath, callerLineNo)) { }
		}

		public static T Try<T>(Func<T> func, T fallbackValue = default(T), [CallerMemberName]string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			try { return func(); }
			catch (Exception e) when (Tracer.ExceptionWarning(e, typeof(ExceptionUtil), true, callerMemberName, callerFilePath, callerLineNo)) { return fallbackValue; }
		}

		public static async Task TryAsync(Func<Task> func, [CallerMemberName]string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			try { await func(); }
			catch (Exception e) when (Tracer.ExceptionWarning(e, typeof(ExceptionUtil), true, callerMemberName, callerFilePath, callerLineNo)) { }
		}

		public static async Task<T> TryAsync<T>(Func<Task<T>> func, T fallbackValue = default(T), [CallerMemberName]string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			try { return await func(); }
			catch (Exception e) when (Tracer.ExceptionWarning(e, typeof(ExceptionUtil), true, callerMemberName, callerFilePath, callerLineNo)) { return fallbackValue; }
		}
	}

	public class GenericTypeParameterNotSupportetException<T> : Exception
	{
		public GenericTypeParameterNotSupportetException()
			: base(string.Format(Exceptions.GenericTypeParameterNotSupportetException, typeof(T).FullName)) { }

		public GenericTypeParameterNotSupportetException(Exception innerException)
			: base(string.Format(Exceptions.GenericTypeParameterNotSupportetException, typeof(T).FullName), innerException) { }
	}

	public class BugException : Exception
	{
		public BugException()
			: base(Exceptions.BugException) { }

		public BugException(string message)
			: base(Exceptions.BugException + ": " + message) { }
	}
}
