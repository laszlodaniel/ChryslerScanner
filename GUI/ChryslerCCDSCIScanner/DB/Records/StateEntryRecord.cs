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
	public class StateEntryRecord : Record
	{
		private const byte FIELD_STRING_ID = 0;
		private const byte FIELD_VALUE = 1;
		private const byte FIELD_DSID_THING = 3;

		public ushort nameStrId;
		public string nameString;

		public ushort value;

		public ushort dsidThing;

		public StateEntryRecord( Table table, byte[] record ) : base( table, record )
		{
			this.nameStrId = (ushort)this.table.readField( this, FIELD_STRING_ID );
			this.nameString = this.table.db.getString( this.nameStrId );

			this.value = (ushort)this.table.readField( this, FIELD_VALUE );

			this.dsidThing = (ushort)this.table.readField( this, FIELD_DSID_THING );
		}
	}
}
