using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;

namespace ArgusLib
{
	public static class WeakEvent
	{
		static readonly ConcurrentDictionary<EventInfo, object> _dict = new ConcurrentDictionary<EventInfo, object>();

		public static void AddHandler<TEventSource, TSender, TEventArgs>(string eventName, Action<TSender, TEventArgs> handler)
		{
			AddHandler(typeof(TEventSource), null, eventName, handler);
		}

		public static void AddHandler<TEventSource, TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler)
		{
			AddHandler(typeof(TEventSource), eventSource, eventName, handler);
		}

		public static void AddHandler<TSender, TEventArgs>(Type eventSourceType, string staticEventName, Action<TSender,TEventArgs> handler)
		{
			AddHandler(eventSourceType, null, staticEventName, handler);
		}

		public static void AddHandler<TSender, TEventArgs>(object eventSource, string eventName, Action<TSender, TEventArgs> handler)
		{
			AddHandler(eventSource.GetType(), eventSource, eventName, handler);
		}

		static void AddHandler<TSender, TEventArgs>(Type eventSourceType, object eventSource, string eventName, Action<TSender, TEventArgs> handler)
		{
			if (eventSourceType == null)
				throw new ArgumentNullException(nameof(eventSourceType));

			EventInfo eventInfo = eventSourceType.GetRuntimeEvent(eventName);
			if (eventInfo == null)
				throw new ArgumentException($"The class {eventSourceType.FullName} has no public event named '{eventName}'.");

			if (eventInfo.AddMethod.IsStatic && eventSource != null)
				throw new ArgumentException($"{eventName} is a static event, {nameof(eventSource)} must be null.", nameof(eventSource));
			else if (!eventInfo.AddMethod.IsStatic && eventSource == null)
				throw new ArgumentException($"{eventName} is an instance event, {nameof(eventSource)} must not be null.", nameof(eventSource));

			if (handler.Target == null)
			{
				// For handlers invoking static methods the handler is added
				var eventHandler = handler.Cast(eventInfo.EventHandlerType);
				if (eventHandler == null)
					throw new ArgumentException($"'{eventName}' does not accept the signature 'void ({typeof(TSender).FullName}, {typeof(TEventArgs).FullName}'.");
				eventInfo.AddEventHandler(eventSource, eventHandler);
				return;
			}

			bool isFirst = false;
			var weakDelegate = (WeakDelegate<Action<TSender, TEventArgs>>)_dict.GetOrAdd(eventInfo, (key) =>
				{
					isFirst = true;
					return new WeakDelegate<Action<TSender, TEventArgs>>(handler);
				});
			handler = null;

			if (isFirst)
				eventInfo.AddEventHandler(eventSource, GetHandlerProxy<TSender, TEventArgs>(eventInfo, eventSource == null ? null : new WeakReference<object>(eventSource), weakDelegate));
			else
				weakDelegate.Add(handler);
		}

		/// <summary>
		/// Gets the proxy event handler. This is encapsulated in a seperate method to make sure that the lambda expression
		/// captures no unnecessary objects.
		/// </summary>
		static Delegate GetHandlerProxy<TSender, TEventArgs>(EventInfo eventInfo, WeakReference<object> eventSourceRef, WeakDelegate<Action<TSender,TEventArgs>> weakDelegate)
		{
			Delegate proxy = null;
			Action<TSender, TEventArgs> action = null;
			if (eventSourceRef != null)
			{
				action = (sender, e) =>
				{
					var eventHandler = weakDelegate.Target;
					if (eventHandler == null)
					{
						object eventSource;
						if (_dict.TryRemove(eventInfo, out eventSource) && eventSourceRef.TryGetTarget(out eventSource))
							eventInfo.RemoveEventHandler(eventSource, proxy);
					}
					else
						eventHandler(sender, e);
				};
			}
			else // Static event
			{
				action = (sender, e) =>
				{
					var eventHandler = weakDelegate.Target;
					if (eventHandler == null)
					{
						object tmp;
						if (_dict.TryRemove(eventInfo, out tmp))
							eventInfo.RemoveEventHandler(null, proxy);
					}
					else
						eventHandler(sender, e);
				};
			}
			proxy = action.Cast(eventInfo.EventHandlerType);
			if (proxy == null)
			{
				object tmp;
				_dict.TryRemove(eventInfo, out tmp);
				throw new ArgumentException($"'{eventInfo.Name}' does not accept the signature 'void ({typeof(TSender).FullName}, {typeof(TEventArgs).FullName}'.");
			}
			return proxy;
		}

		public static void RemoveHandler<TEventSource, TSender, TEventArgs>(string eventName, Action<TSender, TEventArgs> handler)
		{
			RemoveHandler(typeof(TEventSource), null, eventName, handler);
		}

		public static void RemoveHandler<TEventSource, TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler)
		{
			RemoveHandler(typeof(TEventSource), eventSource, eventName, handler);
		}

		public static void RemoveHandler<TSender, TEventArgs>(Type eventSourceType, object eventSource, string eventName, Action<TSender, TEventArgs> handler)
		{
			if (eventSourceType == null)
				throw new ArgumentNullException(nameof(eventSourceType));

			EventInfo eventInfo = eventSourceType.GetRuntimeEvent(eventName);
			if (eventInfo == null)
				throw new ArgumentException($"The class {eventSourceType.FullName} has no public event named '{eventName}'.");

			if (eventInfo.RemoveMethod.IsStatic && eventSource != null)
				throw new ArgumentException($"{eventName} is a static event, {nameof(eventSource)} must be null.", nameof(eventSource));
			else if (!eventInfo.RemoveMethod.IsStatic && eventSource == null)
				throw new ArgumentException($"{eventName} is an instance event, {nameof(eventSource)} must not be null.", nameof(eventSource));

			if (handler.Target == null)
			{
				// For handlers invoking static methods the handler was added
				var eventHandler = handler.Cast(eventInfo.EventHandlerType);
				if (eventHandler == null)
					throw new ArgumentException($"'{eventName}' does not accept the signature 'void ({typeof(TSender).FullName}, {typeof(TEventArgs).FullName}'.");
				eventInfo.RemoveEventHandler(eventSource, eventHandler);
				return;
			}

			object tmp;
			if (!_dict.TryGetValue(eventInfo, out tmp))
				return;

			var weakDel = tmp as WeakDelegate<Action<TSender, TEventArgs>>;
			weakDel?.Remove(handler);
		}
	}
}
