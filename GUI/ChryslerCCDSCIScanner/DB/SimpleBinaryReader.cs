/*
 * DRBDBReader
 * Copyright (C) 2017, Kyle Repinski
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.IO;

namespace DRBDBReader.DB
{
	/// <summary>
	/// Basic thread-safe in-memory file reader.
	/// 
	/// Used instead of Stream-based APIs which require single-thread "Seek" nonsense.
	/// </summary>
	public class SimpleBinaryReader
	{
		public byte[] rawDB;

		public SimpleBinaryReader( FileStream dbFile )
		{
			this.rawDB = new byte[dbFile.Length];
			dbFile.Read( this.rawDB, 0, this.rawDB.Length );
		}

		/// <summary>
		/// Read an arbitrary amount of bytes, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <param name="length">number of bytes requested to read</param>
		/// <returns>requested byte array</returns>
		public byte[] ReadBytes( ref int offset, int length )
		{
			byte[] ret = new byte[length];
			Array.ConstrainedCopy( this.rawDB, offset, ret, 0, ret.Length );
			offset += length;
			return ret;
		}

		/// <summary>
		/// Read a byte, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>byte read</returns>
		public byte ReadUInt8( ref int offset )
		{
			byte ret = this.rawDB[offset];
			offset += sizeof( byte );
			return ret;
		}

		/// <summary>
		/// Read a ushort, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>ushort read</returns>
		public ushort ReadUInt16( ref int offset )
		{
			ushort ret = BitConverter.ToUInt16( this.rawDB, offset );
			offset += sizeof( ushort );
			return ret;
		}

		/// <summary>
		/// Read a uint, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>uint read</returns>
		public uint ReadUInt32( ref int offset )
		{
			uint ret = BitConverter.ToUInt32( this.rawDB, offset );
			offset += sizeof( uint );
			return ret;
		}

		/// <summary>
		/// Read a ulong, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>ulong read</returns>
		public ulong ReadUInt64( ref int offset )
		{
			ulong ret = BitConverter.ToUInt64( this.rawDB, offset );
			offset += sizeof( ulong );
			return ret;
		}

		/// <summary>
		/// Read an sbyte, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>sbyte read</returns>
		public sbyte ReadInt8( ref int offset )
		{
			sbyte ret = Convert.ToSByte( this.rawDB[offset] );
			offset += sizeof( sbyte );
			return ret;
		}

		/// <summary>
		/// Read a short, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>short read</returns>
		public short ReadInt16( ref int offset )
		{
			short ret = BitConverter.ToInt16( this.rawDB, offset );
			offset += sizeof( short );
			return ret;
		}

		/// <summary>
		/// Read an int, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>int read</returns>
		public int ReadInt32( ref int offset )
		{
			int ret = BitConverter.ToInt32( this.rawDB, offset );
			offset += sizeof( int );
			return ret;
		}

		/// <summary>
		/// Read a long, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>long read</returns>
		public long ReadInt64( ref int offset )
		{
			long ret = BitConverter.ToInt64( this.rawDB, offset );
			offset += sizeof( long );
			return ret;
		}

		/// <summary>
		/// Read a float, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>float read</returns>
		public float ReadFloat( ref int offset )
		{
			float ret = BitConverter.ToSingle( this.rawDB, offset );
			offset += sizeof( float );
			return ret;
		}

		/// <summary>
		/// Read a double, add bytes read to offset
		/// </summary>
		/// <param name="offset">byte offset, which is advanced for you</param>
		/// <returns>double read</returns>
		public double ReadDouble( ref int offset )
		{
			double ret = BitConverter.ToDouble( this.rawDB, offset );
			offset += sizeof( double );
			return ret;
		}
	}
}
