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
	public class MenuRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_PARENT_ID = 1;
		private const byte FIELD_NAME_ID = 2;
		private const byte FIELD_THREE = 3;
		private const byte FIELD_SCREEN_POS = 4;
		private const byte FIELD_FIVE = 5;
		private const byte FIELD_EMPTY = 6;

		public ushort id;

		public ushort parentid;

		public string name;
		public ushort nameid;

		public ushort fieldThree;

		public byte screenpos;

		public ushort fieldFive;

		public MenuRecord( Table table, byte[] record ) : base( table, record )
		{
			// get id
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			// get parentid
			this.parentid = (ushort)this.table.readField( this, FIELD_PARENT_ID );

			// get name
			this.nameid = (ushort)this.table.readField( this, FIELD_NAME_ID );
			this.name = this.table.db.getString( this.nameid );

			// get fieldThree
			// Seems important... likely points to an entry in a different table.
			// Possibilities: 3-3, 21-5
			this.fieldThree = (ushort)this.table.readField( this, FIELD_THREE );

			// get screenpos
			// This field is kind of strange.
			// Row 0 will appear as two 00 bytes, Row 1 will appear as two 01 bytes, etc.
			// They always match though, so we'll just ignore the extra byte.
			this.screenpos = (byte)(this.table.readField( this, FIELD_SCREEN_POS ) & 0xFF);

			// get fieldFive
			// Unknown. Only values are 0 and 9.
			this.fieldFive = (ushort)this.table.readField( this, FIELD_FIVE );
		}
	}
}
