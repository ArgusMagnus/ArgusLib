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
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace ArgusLib.Diagnostics.Tracing
{
	public static partial class Tracer
	{
		[EventSource(Name = EventSourceName, Guid ="ArgusLib-Diagnostics-Tracing-ETWProvider")]
		sealed class _EventSource : EventSource
		{
			// Event IDs must be in the range of ushort (0-65535) even though they are 
			// later cast to int
			enum EventID : ushort
			{
				WriteLogAlways = 1,
				WriteVerbose,
				WriteInformational,
				WriteWarning,
				WriteError,
				WriteCritical,
				WriteUnhandledException,
				ExceptionLogAlways,
				ExceptionVerbose,
				ExceptionInformational,
				ExceptionWarning,
				ExceptionError,
				ExceptionCritical,
				ThrowLogAlways,
				ThrowVerbose,
				ThrowInformational,
				ThrowWarning,
				ThrowError,
				ThrowCritical,
				MaxID
			}

			internal _EventSource()
			: base() { }

			internal _EventSource(bool throwOnEventWriteErrors)
			: base(throwOnEventWriteErrors) { }

			#region WriteEvent Methods

			[NonEvent]
			unsafe void WriteEvent(int eventId, string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				if (!this.IsEnabled())
					return;
				
				message = message?.Replace(BuildEnvironment.SolutionDirectory, string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty;
				callerTypeName = callerTypeName ?? string.Empty;
				callerMemberName = callerMemberName ?? string.Empty;
				callerFilePath = callerFilePath?.Replace(BuildEnvironment.SolutionDirectory, string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty;

				fixed (char* chMessage = message)
				fixed (char* chCallerType = callerTypeName)
				fixed (char* chCallerName = callerMemberName)
				fixed (char* chCallerFile = callerFilePath)
				{
					EventData* data = stackalloc EventData[5];
					data[0].DataPointer = (IntPtr)chMessage;
					data[0].Size = (message.Length + 1) * 2;
					data[1].DataPointer = (IntPtr)chCallerType;
					data[1].Size = (callerTypeName.Length + 1) * 2;
					data[2].DataPointer = (IntPtr)chCallerName;
					data[2].Size = (callerMemberName.Length + 1) * 2;
					data[3].DataPointer = (IntPtr)chCallerFile;
					data[3].Size = (callerFilePath.Length + 1) * 2;
					data[4].DataPointer = (IntPtr)(&callerLineNo);
					data[4].Size = 4;
					base.WriteEventCore(eventId, 5, data);
				}
			}

			[NonEvent]
			unsafe void WriteEvent(int eventId, string exception, bool wasCaught, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				if (!this.IsEnabled())
					return;

				exception = exception?.ToString().Replace(BuildEnvironment.SolutionDirectory, string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty;
				callerTypeName = callerTypeName ?? string.Empty;
				callerMemberName = callerMemberName ?? string.Empty;
				callerFilePath = callerFilePath?.Replace(BuildEnvironment.SolutionDirectory, string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty;

				fixed (char* chException = exception)
				fixed (char* chCallerType = callerTypeName)
				fixed (char* chCallerName = callerMemberName)
				fixed (char* chCallerFile = callerFilePath)
				{
					EventData* data = stackalloc EventData[6];
					data[0].DataPointer = (IntPtr)chException;
					data[0].Size = (exception.Length + 1) * 2;
					data[1].DataPointer = (IntPtr)(&wasCaught);
					data[1].Size = 4;
					data[2].DataPointer = (IntPtr)chCallerType;
					data[2].Size = (callerTypeName.Length + 1) * 2;
					data[3].DataPointer = (IntPtr)chCallerName;
					data[3].Size = (callerMemberName.Length + 1) * 2;
					data[4].DataPointer = (IntPtr)chCallerFile;
					data[4].Size = (callerFilePath.Length + 1) * 2;
					data[5].DataPointer = (IntPtr)(&callerLineNo);
					data[5].Size = 4;
					base.WriteEventCore(eventId, 6, data);
				}
			}

			#endregion

			#region Event Methods

			#region Write Message Methdos

			[Event((int)EventID.WriteLogAlways, Level = EventLevel.LogAlways)]
			internal void WriteLogAlways(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteLogAlways, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}


			[Event((int)EventID.WriteVerbose, Level = EventLevel.Verbose)]
			internal void WriteVerbose(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteVerbose, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.WriteInformational, Level = EventLevel.Informational)]
			internal void WriteInformational(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteInformational, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.WriteWarning, Level = EventLevel.Warning)]
			internal void WriteWarning(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteWarning, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.WriteError, Level = EventLevel.Error)]
			internal void WriteError(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteError, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.WriteCritical, Level = EventLevel.Critical)]
			internal void WriteCritical(string message, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteCritical, message, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.WriteUnhandledException, Level = EventLevel.Critical)]
			internal void WriteUnhandledException(string exception, bool isTerminating, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo = 0)
			{
				WriteEvent((int)EventID.WriteUnhandledException, exception, isTerminating, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			#endregion

			#region Exception Filters

			[Event((int)EventID.ExceptionLogAlways, Level = EventLevel.LogAlways)]
			internal void ExceptionLogAlways(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionLogAlways, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ExceptionVerbose, Level = EventLevel.Verbose)]
			internal void ExceptionVerbose(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionVerbose, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ExceptionInformational, Level = EventLevel.Informational)]
			internal void ExceptionInformational(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionInformational, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ExceptionWarning, Level = EventLevel.Warning)]
			internal void ExceptionWarning(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionWarning, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ExceptionError, Level = EventLevel.Error)]
			internal void ExceptionError(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionError, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ExceptionCritical, Level = EventLevel.Critical)]
			internal void ExceptionCritical(string exception, bool catchException, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ExceptionCritical, exception, catchException, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			#endregion

			#region Throw Exception Methods

			[Event((int)EventID.ThrowLogAlways, Level = EventLevel.LogAlways)]
			internal void ThrowLogAlways(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowLogAlways, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ThrowVerbose, Level = EventLevel.Verbose)]
			internal void ThrowVerbose(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowVerbose, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ThrowInformational, Level = EventLevel.Informational)]
			internal void ThrowInformational(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowInformational, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ThrowWarning, Level = EventLevel.Warning)]
			internal void ThrowWarning(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowWarning, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ThrowError, Level = EventLevel.Error)]
			internal void ThrowError(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowError, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			[Event((int)EventID.ThrowCritical, Level = EventLevel.Critical)]
			internal void ThrowCritical(string exception, string callerTypeName, string callerMemberName, string callerFilePath, int callerLineNo)
			{
				WriteEvent((int)EventID.ThrowCritical, exception, callerTypeName, callerMemberName, callerFilePath, callerLineNo);
			}

			#endregion

			#endregion
		}
	}
}
