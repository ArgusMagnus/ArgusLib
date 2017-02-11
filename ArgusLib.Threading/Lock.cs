using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ArgusLib.Threading
{
	/// <summary>
	/// When implementing this interface, the implementing type must follow this rules:
	/// - Lock/TryLock must (try to) acquire a lock (e.g. with <see cref="Monitor.Enter(object)"/>/<see cref="Monitor.TryEnter(object, int)"/>
	/// - Dispose should free the lock (e.g. with <see cref="Monitor.Exit(object)"/>
	/// - All of the above interface methods should be implemented explicitly
	/// - An instance of the implementing type must be reusable
	///   (it must be possible to call as many cycles of <see cref="ILock.Lock"/>/<see cref="ILock.TryLock(int)"/>
	///   and <see cref="IDisposable.Dispose"/> on one instance).
	/// </summary>
	public interface ILock : IDisposable
	{
		void Lock();
		bool TryLock(int timeoutMs);
	}

	public class Lock : ILock
	{
		readonly object _syncRoot;

		public Lock(object syncRoot = null)
		{
			_syncRoot = syncRoot ?? this;
		}

		void ILock.Lock() => Monitor.Enter(_syncRoot);

		bool ILock.TryLock(int timeoutMs) => Monitor.TryEnter(_syncRoot, timeoutMs);

		void IDisposable.Dispose() => Monitor.Exit(_syncRoot);
	}

    public class LockSource<T> where T : class, ILock
    {
		readonly T _lock;

		public LockSource(T @lock)
		{
			_lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
		}

		public T AcquireLock() { _lock.Lock(); return _lock; }

		public T TryAcquireLock(int timeoutMs = 0) => _lock.TryLock(timeoutMs) ? _lock : null;
    }
}
