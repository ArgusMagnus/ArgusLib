﻿#region Copyright and License
/*
This file is part of ArgusLib
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ArgusLib.Runtime.InteropServices
{
	public struct PinnedGCHandle : IDisposable
	{
		GCHandle _handle;

		PinnedGCHandle(GCHandle handle)
		{
			_handle = handle;
		}

		public static PinnedGCHandle Pin(object obj) => new PinnedGCHandle(GCHandle.Alloc(obj, GCHandleType.Pinned));

		public void Free() => _handle.Free();

		void IDisposable.Dispose()
		{
			if (_handle.IsAllocated)
				_handle.Free();
		}

		public override string ToString() => _handle.ToString();

		public IntPtr Pointer => _handle.AddrOfPinnedObject();
		public bool IsAllocated => _handle.IsAllocated;
		public object Target => _handle.Target;
	}

	public struct PinnedGCHandle<T> : IDisposable
		where T : class
	{
		GCHandle _handle;

		PinnedGCHandle(GCHandle handle)
		{
			_handle = handle;
		}

		public static PinnedGCHandle<T> Pin(T obj) => new PinnedGCHandle<T>(GCHandle.Alloc(obj, GCHandleType.Pinned));

		public void Free() => _handle.Free();

		void IDisposable.Dispose()
		{
			if (_handle.IsAllocated)
				_handle.Free();
		}

		public override string ToString() => _handle.ToString();

		public IntPtr Pointer => _handle.AddrOfPinnedObject();
		public bool IsAllocated => _handle.IsAllocated;
		public T Target => (T)_handle.Target;
	}
}
