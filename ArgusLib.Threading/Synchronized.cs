using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ArgusLib.Threading
{
	/// <summary>
	/// Provides synchronized (one thread at the time) access to a value of type <typeparamref name="T"/>.
	/// Make sure to always use the <see cref="Lock"/> returned by <see cref="AcquireLock()"/> or <see cref="TryAcquireLock(int)"/>
	/// with a <c>using</c> block.
	/// </summary>
	/// <example>
	/// <code>
	/// static Synchronzied<List<int>> syncedInt = new Synchronzied<List<int>>(new List<int>());
	/// 
	/// public static int SomeMethod(int newVal)
	/// {
	/// 	using (var _lock = syncedInt.AcquireLock())
	/// 	{
	///			_lock.Value.Add(1);
	///		}
	/// }
	/// </code>
	/// </example>
	public class Synchronized<T> : LockSource<Synchronized<T>.Lock>
	{
		public Synchronized(T value = default(T), object syncRoot = null)
			: base(new Lock(value, syncRoot)) { }

		public class Lock : ArgusLib.Threading.Lock
		{
			public T Value { get; set; }

			internal Lock(T value, object syncRoot)
				: base(syncRoot)
			{
				this.Value = value;
			}
		}
	}
}
