using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ArgusLib.Threading
{
	/// <summary>
	/// This class simplifies the use of <see cref="ReaderWriterLockSlim"/>.
	/// Be careful to always use the <see cref="DisposableLock"/> returned by one
	/// of the Lock* and TryLock* methods with a <c>using</c> block.
	/// </summary>
	/// <example>
	/// <code>
	/// static ReaderWriterLockEx readerWriterLock = new ReaderWriterLockEx();
	/// 
	/// // ...
	/// 
	/// public static void SomeMethod()
	/// {
	///		using (readerWriterLock.AcquireReadLock())
	///		{
	///			// Do something...
	///		}
	/// 
	///		using (var _lock = readerWriterLock.TryAcquireWriteLock())
	///		{
	/// 		// _lock is not null if readerWriterLock.TryAcquireWriteLock() succeeded
	/// 		if (_lock != null)
	/// 		{
	///				// Do something...
	///			}
	///		}
	///	}
	/// </code>
	/// </example>
	public class ReaderWriterLockSource : IDisposable
	{
		readonly ReaderWriterLockSlim _lock;
		readonly ILock _readerToken;
		readonly ILock _writerToken;
		readonly ILock _upgradeableReadToken;

		public ReaderWriterLockSource(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
		{
			_lock = new ReaderWriterLockSlim(recursionPolicy);
			_readerToken = new ReadLock(_lock);
			_writerToken = new WriteLock(_lock);
			_upgradeableReadToken = new UpgradeableReadeLock(_lock);
		}

		public ReadLock AcquireReadLock() { _readerToken.Lock(); return (ReadLock)_readerToken; }
		public WriteLock AcquireWriteLock() { _writerToken.Lock(); return (WriteLock)_writerToken; }
		public UpgradeableReadeLock AcquireUpgradeableReadLock() { _upgradeableReadToken.Lock(); return (UpgradeableReadeLock)_upgradeableReadToken; }
		public ReadLock TryAcquireReadLock(int timeoutMs = 0) => _readerToken.TryLock(timeoutMs) ? (ReadLock)_readerToken : null;
		public WriteLock TryAcquireWriteLock(int timeoutMs = 0) => _writerToken.TryLock(timeoutMs) ? (WriteLock)_writerToken : null;
		public UpgradeableReadeLock TryAcquireUpgradeableReadLock(int timeoutMs = 0) => _upgradeableReadToken.TryLock(timeoutMs) ? (UpgradeableReadeLock)_upgradeableReadToken : null;

		public void Dispose() { _lock.Dispose(); }

		public class ReadLock : ILock
		{
			readonly ReaderWriterLockSlim _lock;

			public ReadLock(ReaderWriterLockSlim @lock)
			{
				_lock = @lock;
			}

			void IDisposable.Dispose() => _lock.ExitReadLock();

			void ILock.Lock() => _lock.EnterReadLock();

			bool ILock.TryLock(int timeoutMs) => _lock.TryEnterReadLock(timeoutMs);
		}

		public class WriteLock : ILock
		{
			readonly ReaderWriterLockSlim _lock;

			public WriteLock(ReaderWriterLockSlim @lock)
			{
				_lock = @lock;
			}

			void IDisposable.Dispose() => _lock.ExitWriteLock();

			void ILock.Lock() => _lock.EnterWriteLock();

			bool ILock.TryLock(int timeoutMs) => _lock.TryEnterWriteLock(timeoutMs);
		}

		public class UpgradeableReadeLock : ILock
		{
			readonly ReaderWriterLockSlim _lock;

			public UpgradeableReadeLock(ReaderWriterLockSlim @lock)
			{
				_lock = @lock;
			}

			void IDisposable.Dispose() => _lock.ExitUpgradeableReadLock();

			void ILock.Lock() => _lock.EnterUpgradeableReadLock();

			bool ILock.TryLock(int timeoutMs) => _lock.TryEnterUpgradeableReadLock(timeoutMs);
		}
	}
}
