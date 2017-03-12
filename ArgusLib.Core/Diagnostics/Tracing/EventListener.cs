#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Diagnostics.Tracing;

namespace ArgusLib.Diagnostics.Tracing
{
	public static partial class Tracer
	{
		sealed class _EventListener : EventListener
		{
			/// <summary>
			/// <see cref="EventWrittenAsync"/> is invoked, when an enabled <see cref="EventSource"/> writes an event.
			/// By default, no <see cref="EventSource"/> is enabled. To enable/disable <see cref="EventSource"/>s
			/// call one of the <see cref="EventListener.EnableEvents"/>/<see cref="EventListener.DisableEvents"/> methods.
			/// </summary>
			public event AsyncEventHandler<object, EventWrittenEventArgs> EventWrittenAsync;

			/// <summary>
			/// <see cref="EventWritten"/> is invoked, when an enabled <see cref="EventSource"/> writes an event.
			/// By default, no <see cref="EventSource"/> is enabled. To enable/disable <see cref="EventSource"/>s
			/// call one of the <see cref="EventListener.EnableEvents"/>/<see cref="EventListener.DisableEvents"/> methods.
			/// </summary>
			public event EventHandler<EventWrittenEventArgs> EventWritten;

			internal _EventListener()
				: base()
			{
				//// Make sure platform-specific libraries are initializes
				//// since they may hook up events to write Tracer events
				//// e.g. to the EventLog
				//PlatformLibraryLoader.Load(PlatformLibraryLoader.NameArgusLib);
			}

			protected override async void OnEventWritten(EventWrittenEventArgs eventData)
			{
				EventWritten?.Invoke(null, eventData);
				if (EventWrittenAsync != null)
					await EventWrittenAsync.InvokeInParallelAsync(null, eventData).ConfigureAwait(false);
			}
		}
	}
}
