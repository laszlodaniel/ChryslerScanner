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
using System.Collections.Generic;

namespace DRBDBReader.DB.Records
{
	public class ModuleRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_SCID = 1;
		private const byte FIELD_NAMEID = 3;

		public ushort id;
		public ushort scid;
		public string scname;
		public ushort nameid;
		public string name;

		public List<TXRecord> dataelements;

		public ModuleRecord( Table table, byte[] record ) : base( table, record )
		{
			// get id
			this.id = (ushort)this.table.readField( this, FIELD_ID );


			// get scid/scname
			this.scid = (ushort)this.table.readField( this, FIELD_SCID );
			this.scname = this.table.db.getServiceCatString( this.scid );


			// get name
			this.nameid = (ushort)this.table.readField( this, FIELD_NAMEID );
			this.name = this.table.db.getString( this.nameid );


			// dataelements will be filled in as ModuleDataElemRecord's load
			this.dataelements = new List<TXRecord>();
		}
	}
}
