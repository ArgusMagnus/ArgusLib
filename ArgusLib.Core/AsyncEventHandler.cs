#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Threading.Tasks;

namespace ArgusLib
{
	public delegate Task AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) where TEventArgs : EventArgs;
	public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;
	public delegate Task AsyncEventHandler(object sender, EventArgs e);

	public static partial class ExtensionMethods
	{
		public static async Task InvokeAsync<TSender, TEventArgs>(this AsyncEventHandler<TSender, TEventArgs> handler, TSender sender, TEventArgs e)
			where TEventArgs : EventArgs
		{
			foreach (AsyncEventHandler<TSender, TEventArgs> subscriber in handler.GetInvocationList())
				await subscriber.Invoke(sender, e).ConfigureAwait(false);
		}

		public static async Task InvokeAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler, object sender, TEventArgs e)
			where TEventArgs : EventArgs
		{
			foreach (AsyncEventHandler<TEventArgs> subscriber in handler.GetInvocationList())
				await subscriber.Invoke(sender, e).ConfigureAwait(false);
		}

		public static async Task InvokeAsync(this AsyncEventHandler handler, object sender, EventArgs e)
		{
			foreach (AsyncEventHandler subscriber in handler.GetInvocationList())
				await subscriber.Invoke(sender, e).ConfigureAwait(false);
		}
	}
}