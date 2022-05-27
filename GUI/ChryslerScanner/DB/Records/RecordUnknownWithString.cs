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
	public class RecordUnknownWithString : Record
	{
		private const byte FIELD_ID = 0;
		public byte stringidcol;

		public ushort id;

		public string str;
		public ushort strid;

		public RecordUnknownWithString( Table table, byte[] record, byte stringidcol ) : base( table, record )
		{
			this.stringidcol = stringidcol;
			// get id
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			// get string
			this.strid = (ushort)this.table.readField( this, this.stringidcol );
			this.str = this.table.db.getString( this.strid );
		}
	}
}
