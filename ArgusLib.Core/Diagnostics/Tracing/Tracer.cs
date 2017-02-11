#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace ArgusLib.Diagnostics.Tracing
{
	public static partial class Tracer
	{
		public const string EventSourceName = "ArgusLibEventSource";
		static readonly _EventSource EventSource = new _EventSource();
		static readonly _EventListener EventListener = new _EventListener();

		/// <summary>
		/// <see cref="EventWritten"/> is invoked, when the tracer is enabled and an event is written.
		/// By default, the tracer is disabled. To enable/disable the tracer
		/// call one of the <see cref="Enable(EventLevel)"/>/<see cref="Disable"/> methods.
		/// </summary>
		public static event EventHandler<EventWrittenEventArgs> EventWritten
		{
			add { EventListener.EventWritten += value; }
			remove { EventListener.EventWritten -= value; }
		}

		/// <summary>
		/// <see cref="EventWrittenAsync"/> is invoked, when the tracer is enabled and an event is written.
		/// By default, the tracer is disabled. To enable/disable the tracer
		/// call one of the <see cref="Enable(EventLevel)"/>/<see cref="Disable"/> methods.
		/// </summary>
		public static event AsyncEventHandler<EventWrittenEventArgs> EventWrittenAsync
		{
			add { EventListener.EventWrittenAsync += value; }
			remove { EventListener.EventWrittenAsync -= value; }
		}

		public static void Enable(EventLevel eventLevel) => EventListener.EnableEvents(EventSource, eventLevel);
		public static void Enable(EventLevel eventLevel, IDictionary<string, string> arguments) => EventListener.EnableEvents(EventSource, eventLevel, EventKeywords.None, arguments);
		public static void Disable() => EventListener.DisableEvents(EventSource);

		#region Tracing Methods

		#region Write Message Methods

		public static void WriteLogAlways(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteLogAlways(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteLogAlways<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteLogAlways(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteVerbose(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteVerbose(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteVerbose<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteVerbose(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteInformational(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteInformational(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteInformational<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteInformational(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteWarning(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteWarning(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteWarning<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteWarning(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteError(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteError(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteError<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteWarning(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteCritical(string message, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteCritical(message, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteCritical<TCaller>(string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteWarning(message, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteUnhandledException(string exception, bool isTerminating, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteUnhandledException(exception, isTerminating, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		public static void WriteUnhandledException<TCaller>(string exception, bool isTerminating, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.WriteUnhandledException(exception, isTerminating, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
		}

		#endregion

		#region Exception Filters

		public static bool ExceptionLogAlways(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionLogAlways(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionLogAlways<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionLogAlways(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionVerbose(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionVerbose(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionVerbose<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionVerbose(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionInformational(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionInformational(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionInformational<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionInformational(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionWarning(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionWarning(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionWarning<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionWarning(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionError(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionError(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionError<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionError(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionCritical(Exception exception, Type callerType, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionCritical(exception.ToString(), catchException, callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		public static bool ExceptionCritical<TCaller>(Exception exception, bool catchException = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ExceptionCritical(exception.ToString(), catchException, typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return catchException;
		}

		#endregion

		#region Throw Exception Methods

		public static Exception ThrowLogAlways(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowLogAlways(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowLogAlways<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowLogAlways(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowVerbose(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowVerbose(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowVerbose<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowVerbose(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowInformational(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowInformational(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowInformational<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowInformational(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowWarning(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowWarning(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowWarning<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowWarning(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowError(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowError(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowError<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowError(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowCritical(Exception exception, Type callerType, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowCritical(exception.ToString(), callerType?.FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		public static Exception ThrowCritical<TCaller>(Exception exception, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNo = 0)
		{
			EventSource.ThrowCritical(exception.ToString(), typeof(TCaller).FullName, callerMemberName, callerFilePath, callerLineNo);
			return exception;
		}

		#endregion

		#endregion
	}
}
