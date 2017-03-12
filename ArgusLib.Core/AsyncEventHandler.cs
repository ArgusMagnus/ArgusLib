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
	public delegate Task AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);

	public static partial class ExtensionMethods
	{
		public static async Task InvokeAsync<TSender, TEventArgs>(this AsyncEventHandler<TSender, TEventArgs> handler, TSender sender, TEventArgs e)
		{
			foreach (AsyncEventHandler<TSender, TEventArgs> subscriber in handler.GetInvocationList())
				await subscriber.Invoke(sender, e).ConfigureAwait(false);
		}

		public static async Task InvokeInParallelAsync<TSender, TEventArgs>(this AsyncEventHandler<TSender, TEventArgs> handler, TSender sender, TEventArgs e)
		{
			var invocationList = handler.GetInvocationList();
			var tasks = new Task[invocationList.Length];
			for (int i = 0; i < invocationList.Length; i++)
				tasks[i] = (invocationList[i] as AsyncEventHandler<TSender, TEventArgs>).Invoke(sender, e);
			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}