/*
 * DRBDBReader
 * Copyright (C) 2016-2017, Kyle Repinski
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
	public class ServiceCatRecord : Record
	{
		private const byte FIELD_NAMEID = 1;
		private const byte FIELD_SCID = 3;

		public ushort nameid;
		public string name;

		public ushort scid;

		public ServiceCatRecord( Table table, byte[] record ) : base( table, record )
		{
			// get (sc) id
			this.scid = (ushort)this.table.readField( this, FIELD_SCID );


			// get name
			this.nameid = (ushort)this.table.readField( this, FIELD_NAMEID );
			this.name = this.table.db.getString( this.nameid );
		}
	}
}
