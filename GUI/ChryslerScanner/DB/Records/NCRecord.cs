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

namespace DRBDBReader.DB.Records
{
	public class NCRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_SLOPE = 1;
		private const byte FIELD_OFFSET = 2;
		private const byte FIELD_THREE = 3; // Always 0 or 257
		private const byte FIELD_EMPTY = 4;

		public ushort id;

		public float slope;
		public float offset;

		public NCRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.slope = BitConverter.ToSingle( BitConverter.GetBytes( (int)this.table.readField( this, FIELD_SLOPE ) ), 0 );
			this.offset = BitConverter.ToSingle( BitConverter.GetBytes( (int)this.table.readField( this, FIELD_OFFSET ) ), 0 );
		}
	}
}
