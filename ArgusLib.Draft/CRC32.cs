#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion
using System;
using System.IO;

namespace ArgusLib
{
	/// <include file='Documentation/CRC32.xml' path='CRC32/*'/>
	public partial class CRC32
	{
		static readonly LazyWeakReference<uint[]> _refTable = new LazyWeakReference<uint[]>(CreateTable);

		uint[] table;
		uint currentCRC;

		/// <summary>
		/// Gets the current CRC32 value.
		/// </summary>
		/// <seealso cref="Add"/>
		/// <seealso cref="Clear"/>
		public int CurrentCRC
		{
			get
			{
				uint x = 0xffffffff;
				return (int)(this.currentCRC ^ x);
			}
		}

		public CRC32()
		{
			this.table = _refTable.Get();
			this.Clear();
		}

		static uint Update(uint[] table, uint crc, byte[] data)
		{
			return Update(table, crc, data, 0, data.Length);
		}

		static uint Update(uint[] table, uint crc, byte b)
		{
			return table[(crc ^ (uint)b) & 0xFF] ^ (crc >> 8);
		}

		static uint Update(uint[] table, uint crc, byte[] data, int offset, int count)
		{
			for (int n = offset; n < count; n++)
			{
				crc = Update(table, crc, data[n]);
			}
			return crc;
		}

		static uint Update(uint[] table, uint crc, Stream stream, int count)
		{
			for (int n = 0; n < count; n++)
			{
				int b = stream.ReadByte();
				if (b < 0)
					throw new EndOfStreamException();
				crc = Update(table, crc, (byte)b);
			}
			return crc;
		}

		/// <summary>
		/// Updates the value in <see cref="CurrentCRC"/> with the data from a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The stream from which the data is read.</param>
		/// <param name="count">The number of bytes to read from <paramref name="stream"/>.</param>
		public void Add(Stream stream, int count)
		{
			this.currentCRC = Update(this.table, this.currentCRC, stream, count);
		}

		/// <summary>
		/// Updates the value in <see cref="CurrentCRC"/> with the data in a <see cref="byte"/>.
		/// </summary>
		/// <param name="b">The data for which the CRC32 value is updated.</param>
		public void Add(byte b)
		{
			this.currentCRC = Update(this.table, this.currentCRC, b);
		}

		/// <summary>
		/// Updates the value in <see cref="CurrentCRC"/> with the data in a <see cref="byte[]"/>.
		/// </summary>
		/// <param name="data">The data for which the CRC32 value is updated.</param>
		public void Add(byte[] data)
		{
			this.currentCRC = Update(table, this.currentCRC, data);
		}

		/// <summary>
		/// Updates the value in <see cref="CurrentCRC"/> with the data in a <see cref="byte[]"/>.
		/// </summary>
		/// <param name="data">The data for which the CRC32 value is updated.</param>
		/// <param name="offset">The zero-based index for where to start reading bytes in <paramref name="data"/>.</param>
		/// <param name="count">The number of bytes to read from <paramref name="data"/>.</param>
		public void Add(byte[] data, int offset, int count)
		{
			this.currentCRC = Update(table, this.currentCRC, data, offset, count);
		}

		/// <summary>
		/// Resets <see cref="CurrentCRC"/> to 0.
		/// </summary>
		public void Clear()
		{
			this.currentCRC = 0xffffffff;
		}

		/// <summary>
		/// Calculates and returns the CRC32 value for an array of bytes independent of any pervious calculations.
		/// </summary>
		/// <param name="data">The data for which the CRC32 should be calculated.</param>
		/// <returns>The calculated CRC32 value.</returns>
		public static int Calculate(byte[] data)
		{
			return Calculate(data, 0, data.Length);
		}

		/// <summary>
		/// Calculates and returns the CRC32 value for an array of bytes independent of any pervious calculations.
		/// </summary>
		/// <param name="data">The data for which the CRC32 should be calculated.</param>
		/// <param name="offset">The zero-based index for where to start reading bytes in <paramref name="data"/>.</param>
		/// <param name="count">The number of bytes to read from <paramref name="data"/>.</param>
		/// <returns>The calculated CRC32 value.</returns>
		public static int Calculate(byte[] data, int offset, int count)
		{
			uint crc = 0xffffffff;
			return (int)(Update(_refTable.Get(), crc, data, offset, count) ^ crc);
		}

		/// <summary>
		/// Calculates and returns the CRC32 value for binary data read from a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The stream from which the data is read.</param>
		/// <param name="count">The number of bytes to read from <paramref name="stream"/>.</param>
		/// <returns>The calculated CRC32 value.</returns>
		public static int Calculate(Stream stream, int count)
		{
			uint crc = 0xffffffff;
			return (int)(Update(_refTable.Get(), crc, stream, count) ^ crc);
		}
	}
}
