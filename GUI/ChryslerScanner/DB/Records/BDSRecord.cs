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
	public class BDSRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_TRUE_STR_ID = 1;
		private const byte FIELD_FALSE_STR_ID = 2;

		public ushort id;

		public ushort trueStrId;
		public string trueString;

		public ushort falseStrId;
		public string falseString;

		public BDSRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.trueStrId = (ushort)this.table.readField( this, FIELD_TRUE_STR_ID );
			this.trueString = this.table.db.getString( this.trueStrId );

			this.falseStrId = (ushort)this.table.readField( this, FIELD_FALSE_STR_ID );
			this.falseString = this.table.db.getString( this.falseStrId );
		}
	}
}
