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
	public class SDSRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_DEFAULT_STR_ID = 1;

		public ushort id;

		public ushort defaultStrId;
		public string defaultString;

		public SDSRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.defaultStrId = (ushort)this.table.readField( this, FIELD_DEFAULT_STR_ID );
			this.defaultString = ( this.defaultStrId != 0 ? this.table.db.getString( this.defaultStrId ) : "N/A" );
		}
	}
}
