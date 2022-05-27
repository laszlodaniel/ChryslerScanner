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
namespace DRBDBReader.DB.Records
{
	public class DADRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_REQUEST_LENGTH = 1;
		private const byte FIELD_RESPONSE_LENGTH = 3;
		private const byte FIELD_EXTRACT_OFFSET = 5;
		private const byte FIELD_EXTRACT_SIZE = 6;
		private const byte FIELD_EMPTY_ONE = 7;
		private const byte FIELD_EMPTY_TWO = 8;
		private const byte FIELD_PROTOCOL = 10;

		public ushort id;

		public byte requestLength;
		public byte responseLength;
		public byte extractOffset;
		public byte extractSize;

		public ushort protocolid;

		public DADRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.requestLength = (byte)this.table.readField( this, FIELD_REQUEST_LENGTH );
			this.responseLength = (byte)this.table.readField( this, FIELD_RESPONSE_LENGTH );

			this.extractOffset = (byte)this.table.readField( this, FIELD_EXTRACT_OFFSET );
			this.extractSize = (byte)this.table.readField( this, FIELD_EXTRACT_SIZE );

			this.protocolid = (ushort)this.table.readField( this, FIELD_PROTOCOL );
		}
	}
}
