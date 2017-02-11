using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArgusLib.Internal;

namespace ArgusLib.Threading
{
	/// <summary>
	/// Provides thread-safe access to a <typeparamref name="T"/> value.
	/// Beware: Only access to the reference itself is thread-safe,
	/// access to instance members is not.
	/// </summary>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedSingle"/>
	/// <seealso cref="InterlockedDouble"/>
	/// <seealso cref="InterlockedBoolean"/>
	/// <seealso cref="InterlockedEnum"/>
	public sealed class Interlocked<T> where T : class
	{
		T _value;

		public T Value
		{
			get { return Interlocked.CompareExchange<T>(ref _value, default(T), default(T)); }
			set { Interlocked.Exchange<T>(ref _value, value); }
		}

		public Interlocked(T value = default(T))
		{
			this.Value = value;
		}

		public T Exchange(T value)
		{
			return Interlocked.Exchange(ref _value, value);
		}

		public T CompareExchange(T value, T comparand)
		{
			return Interlocked.CompareExchange<T>(ref _value, value, comparand);
		}

		public static implicit operator T(Interlocked<T> value) { return value.Value; }
	}

	/// <summary>
	/// Provides thread-safe access to a <see cref="bool"/> value.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedSingle"/>
	/// <seealso cref="InterlockedDouble"/>
	/// <seealso cref="InterlockedEnum"/>
	public sealed class InterlockedBoolean
	{
		int _value;

		public bool Value
		{
			get { return Interlocked.CompareExchange(ref _value, 0, 0) != 0; }
			set { Interlocked.Exchange(ref _value, value ? 1 : 0); }
		}

		public InterlockedBoolean(bool value = default(bool))
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Int32, Int32)"/>
		public bool Exchange(bool value)
		{
			return Interlocked.Exchange(ref _value, value ? 1 : 0) != 0;
		}

		/// <seealso cref="Interlocked.CompareExchange(ref Int32, Int32, Int32)"/>
		public bool CompareExchange(bool value, bool comparand)
		{
			return Interlocked.CompareExchange(ref _value, value ? 1 : 0, comparand ? 1 : 0) != 0;
		}

		public static implicit operator bool(InterlockedBoolean value) { return value.Value; }
	}

	/// <summary>
	/// Provides thread-safe access to a value of the enum type <typeparamref name="TEnum"/>.
	/// To get an instance of this class, use the <seealso cref="InterlockedEnum.Create"/> method.
	/// </summary>
	/// <seealso cref="Interlocked{T}"/>
	/// <seealso cref="InterlockedInt32"/>
	/// <seealso cref="InterlockedInt64"/>
	/// <seealso cref="InterlockedSingle"/>
	/// <seealso cref="InterlockedDouble"/>
	/// <seealso cref="InterlockedBoolean"/>
	public abstract class InterlockedEnum<TEnum>
	{
		public abstract TEnum Value { get; set; }

		internal protected InterlockedEnum(TEnum value)
		{
			this.Value = value;
		}

		/// <seealso cref="Interlocked.Exchange(ref Int32, Int32)"/>
		/// <seealso cref="Interlocked.Exchange(ref Int64, Int64)"/>
		public abstract TEnum Exchange(TEnum value);

		/// <seealso cref="Interlocked.CompareExchange(ref Int32, Int32, Int32)"/>
		/// <seealso cref="Interlocked.CompareExchange(ref Int64, Int64, Int64)"/>
		public abstract TEnum CompareExchange(TEnum value, TEnum comparand);

		public static implicit operator TEnum(InterlockedEnum<TEnum> value) { return value.Value; }
	}

	namespace Internal
	{
		public abstract class InterlockedEnumCore<T>
		{
			static InterlockedEnumCore()
			{
				if (typeof(T) != typeof(Enum))
					throw new GenericTypeParameterNotSupportetException<T>(new InvalidOperationException($"{nameof(T)} must be {typeof(Enum).FullName}."));
			}

			internal InterlockedEnumCore()
			{ }

			public static InterlockedEnum<TEnum> Create<TEnum>(TEnum value = default(TEnum))
				where TEnum : struct, T
			{
				Type type = Enum.GetUnderlyingType(typeof(TEnum));
				if (type == typeof(long) || type == typeof(ulong))
					return new InterlockedEnum64<TEnum>(value);
				else
					return new InterlockedEnum32<TEnum>(value);
			}
		}

		sealed class InterlockedEnum32<T> : InterlockedEnum<T>
		{
			int _value;

			public override T Value
			{
				get { return Integer<T>.FromInt32(Interlocked.CompareExchange(ref _value, 0, 0)); }
				set { Interlocked.Exchange(ref _value, Integer<T>.ToInt32(value)); }
			}

			internal InterlockedEnum32(T value)
				: base(value) { }

			public override T Exchange(T value)
			{
				return Integer<T>.FromInt32(Interlocked.Exchange(ref _value, Integer<T>.ToInt32(value)));
			}

			public override T CompareExchange(T value, T comparand)
			{
				return Integer<T>.FromInt32(Interlocked.CompareExchange(ref _value, Integer<T>.ToInt32(value), Integer<T>.ToInt32(comparand)));
			}
		}

		sealed class InterlockedEnum64<T> : InterlockedEnum<T>
		{
			long _value;

			public override T Value
			{
				get { return Integer<T>.FromInt64(Interlocked.CompareExchange(ref _value, 0, 0)); }
				set { Interlocked.Exchange(ref _value, Integer<T>.ToInt64(value)); }
			}

			internal InterlockedEnum64(T value)
				: base(value) { }

			public override T Exchange(T value)
			{
				return Integer<T>.FromInt64(Interlocked.Exchange(ref _value, Integer<T>.ToInt64(value)));
			}

			public override T CompareExchange(T value, T comparand)
			{
				return Integer<T>.FromInt64(Interlocked.CompareExchange(ref _value, Integer<T>.ToInt64(value), Integer<T>.ToInt64(comparand)));
			}
		}
	}

	public sealed class InterlockedEnum : Internal.InterlockedEnumCore<Enum>
	{
		InterlockedEnum()
		{ }
	}
}