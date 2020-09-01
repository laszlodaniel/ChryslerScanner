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
using System;
using System.Collections.Generic;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters
{
	public class StateConverter : Converter
	{
		public SCRecord scRecord;

		public SDSRecord sdsRecord;
		public SortedDictionary<ushort, string> entries;

		public StateConverter( Database db, byte[] record, ushort cfid, ushort dsid ) : base( db, record, cfid, dsid )
		{
			Table stateConvTable = this.db.tables[Database.TABLE_CONVERTERS_STATE];
			this.scRecord = (SCRecord)stateConvTable.getRecord( this.cfid );

			this.entries = new SortedDictionary<ushort, string>();
			this.buildStateList();
		}

		protected virtual void buildStateList()
		{
			Table sdsTable = this.db.tables[Database.TABLE_STATE_DATA_SPECIFIER];
			this.dsRecord = sdsTable.getRecord( this.dsid );
			this.sdsRecord = (SDSRecord)this.dsRecord;

			Table stateTable = this.db.tables[Database.TABLE_STATE_ENTRY];
			foreach( StateEntryRecord seRecord in stateTable.records )
			{
				if( seRecord.dsidThing == this.dsid )
				{
					this.entries.Add( seRecord.value, seRecord.nameString );
				}
			}
		}

		public override string processData( long data, bool outputMetric = false )
		{
			ushort entryID = this.getEntryID( (ushort)data );
			if( this.entries.ContainsKey( entryID ) )
			{
				return this.entries[entryID];
			}
			return this.sdsRecord.defaultString;
		}

		protected virtual ushort getEntryID( ushort val )
		{
			if( this.scRecord.mask != 0 )
			{
				val = (ushort)( val & this.scRecord.mask );
			}
			return val;
		}

		public override string ToString()
		{
			string ret = base.ToString() + Environment.NewLine;

			if( this.sdsRecord != null )
			{
				ret += Environment.NewLine + "DFLT: " + this.sdsRecord.defaultString;
			}

			foreach( KeyValuePair<ushort, string> kvp in this.entries )
			{
				ret += Environment.NewLine + "0x" + kvp.Key.ToString( "X2" ) + ": " + kvp.Value;
			}

			return ret;
		}
	}
}
