using System;
using System.Threading;

namespace ArgusLib.Threading
{
	/// <summary>
	/// Provides thread-safe access to a <see cref="Int32"/> value.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedSingle"/>
	/// <seealso cref="InterlockedDouble"/>
	public sealed class InterlockedInt32
	{
		Int32 _value;

		public Int32 Value
		{
			get { return Interlocked.CompareExchange(ref _value, default(Int32), default(Int32)); }
			set { Interlocked.Exchange(ref _value, value); }
		}

		public InterlockedInt32(Int32 value = default(Int32))
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Int32, Int32)"/>
		public Int32 Exchange(Int32 value)
		{
			return Interlocked.Exchange(ref _value, value);
		}

		/// <seealso cref="Interlocked.CompareExchange(ref Int32, Int32, Int32)"/>
		public Int32 CompareExchange(Int32 value, Int32 comparand)
		{
			return Interlocked.CompareExchange(ref _value, value, comparand);
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the maximum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (maximum) of <see cref="Value"/>.</returns>
		public Int32 Max(Int32 comparand)
		{
			var val = this.Value;
			while (comparand > val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the minimum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (minimum) of <see cref="Value"/>.</returns>
		public Int32 Min(Int32 comparand)
		{
			var val = this.Value;
			while (comparand < val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Increments the <see cref="Value"/> in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Increment(ref Int32)"/>
		public Int32 Increment() { return Interlocked.Increment(ref _value); }

		/// <summary>
		/// Decrements the <see cref="Value"/> in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Decrement(ref Int32)"/>
		public Int32 Decrement() { return Interlocked.Decrement(ref _value); }

		/// <summary>
		/// Sets <see cref="Value"/> to the sum of <see cref="Value"/> and <paramref name="summand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Add(ref Int32, Int32)"/>
		public Int32 Add(Int32 summand) { return Interlocked.Add(ref _value, summand); }

		/// <summary>
		/// Sets <see cref="Value"/> to the difference of <see cref="Value"/> and <paramref name="subtrahend"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Add(ref Int32, Int32)"/>
		public Int32 Subtract(Int32 subtrahend) { return Interlocked.Add(ref _value, -subtrahend); }

		public static implicit operator Int32(InterlockedInt32 value) { return value.Value; }
	}

	/// <summary>
	/// Provides thread-safe access to a <see cref="Int64"/> value.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedSingle"/>
	/// <seealso cref="InterlockedDouble"/>
	public sealed class InterlockedInt64
	{
		Int64 _value;

		public Int64 Value
		{
			get { return Interlocked.CompareExchange(ref _value, default(Int64), default(Int64)); }
			set { Interlocked.Exchange(ref _value, value); }
		}

		public InterlockedInt64(Int64 value = default(Int64))
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Int64, Int64)"/>
		public Int64 Exchange(Int64 value)
		{
			return Interlocked.Exchange(ref _value, value);
		}

		/// <seealso cref="Interlocked.CompareExchange(ref Int64, Int64, Int64)"/>
		public Int64 CompareExchange(Int64 value, Int64 comparand)
		{
			return Interlocked.CompareExchange(ref _value, value, comparand);
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the maximum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (maximum) of <see cref="Value"/>.</returns>
		public Int64 Max(Int64 comparand)
		{
			var val = this.Value;
			while (comparand > val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the minimum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (minimum) of <see cref="Value"/>.</returns>
		public Int64 Min(Int64 comparand)
		{
			var val = this.Value;
			while (comparand < val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Increments the <see cref="Value"/> in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Increment(ref Int64)"/>
		public Int64 Increment() { return Interlocked.Increment(ref _value); }

		/// <summary>
		/// Decrements the <see cref="Value"/> in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Decrement(ref Int64)"/>
		public Int64 Decrement() { return Interlocked.Decrement(ref _value); }

		/// <summary>
		/// Sets <see cref="Value"/> to the sum of <see cref="Value"/> and <paramref name="summand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Add(ref Int64, Int64)"/>
		public Int64 Add(Int64 summand) { return Interlocked.Add(ref _value, summand); }

		/// <summary>
		/// Sets <see cref="Value"/> to the difference of <see cref="Value"/> and <paramref name="subtrahend"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <seealso cref="Interlocked.Add(ref Int64, Int64)"/>
		public Int64 Subtract(Int64 subtrahend) { return Interlocked.Add(ref _value, -subtrahend); }

		public static implicit operator Int64(InterlockedInt64 value) { return value.Value; }
	}

	/// <summary>
	/// Provides thread-safe access to a <see cref="Single"/> value.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedDouble"/>
	public sealed class InterlockedSingle
	{
		Single _value;

		public Single Value
		{
			get { return Interlocked.CompareExchange(ref _value, default(Single), default(Single)); }
			set { Interlocked.Exchange(ref _value, value); }
		}

		public InterlockedSingle(Single value = default(Single))
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Single, Single)"/>
		public Single Exchange(Single value)
		{
			return Interlocked.Exchange(ref _value, value);
		}

		/// <seealso cref="Interlocked.CompareExchange(ref Single, Single, Single)"/>
		public Single CompareExchange(Single value, Single comparand)
		{
			return Interlocked.CompareExchange(ref _value, value, comparand);
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the maximum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (maximum) of <see cref="Value"/>.</returns>
		public Single Max(Single comparand)
		{
			var val = this.Value;
			while (comparand > val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the minimum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (minimum) of <see cref="Value"/>.</returns>
		public Single Min(Single comparand)
		{
			var val = this.Value;
			while (comparand < val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		public static implicit operator Single(InterlockedSingle value) { return value.Value; }
	}

	/// <summary>
	/// Provides thread-safe access to a <see cref="Double"/> value.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedSingle"/>
	public sealed class InterlockedDouble
	{
		Double _value;

		public Double Value
		{
			get { return Interlocked.CompareExchange(ref _value, default(Double), default(Double)); }
			set { Interlocked.Exchange(ref _value, value); }
		}

		public InterlockedDouble(Double value = default(Double))
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Double, Double)"/>
		public Double Exchange(Double value)
		{
			return Interlocked.Exchange(ref _value, value);
		}

		/// <seealso cref="Interlocked.CompareExchange(ref Double, Double, Double)"/>
		public Double CompareExchange(Double value, Double comparand)
		{
			return Interlocked.CompareExchange(ref _value, value, comparand);
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the maximum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (maximum) of <see cref="Value"/>.</returns>
		public Double Max(Double comparand)
		{
			var val = this.Value;
			while (comparand > val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		/// <summary>
		/// Sets <see cref="Value"/> to the minimum of <see cref="Value"/> and <paramref name="comparand"/>
		/// in a thread-safe manner and returns the new value.
		/// </summary>
		/// <returns>The new value (minimum) of <see cref="Value"/>.</returns>
		public Double Min(Double comparand)
		{
			var val = this.Value;
			while (comparand < val)
			{
				val = comparand;
				comparand = Interlocked.Exchange(ref _value, val);
			}
			return val;
		}

		public static implicit operator Double(InterlockedDouble value) { return value.Value; }
	}

}