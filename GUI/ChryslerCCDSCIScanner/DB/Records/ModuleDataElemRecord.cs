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
	public class ModuleDataElemRecord : Record
	{
		private const byte FIELD_MODULE_FOR_ID = 0;
		private const byte FIELD_TXID = 1;

		public ushort moduleForId;

		public uint txid;

		public ModuleDataElemRecord( Table table, byte[] record ) : base( table, record )
		{
			this.moduleForId = (ushort)this.table.readField( this, FIELD_MODULE_FOR_ID );

			this.txid = (uint)this.table.readField( this, FIELD_TXID );

			// Add TXRecord to corresponding ModuleRecord's List<TXRecord>
			Table moduleTable = this.table.db.tables[Database.TABLE_MODULE];
			Table txTable = this.table.db.tables[Database.TABLE_TRANSMIT];
			ModuleRecord moduleRec = (ModuleRecord)moduleTable.getRecord( this.moduleForId );
			TXRecord txRec = (TXRecord)txTable.getRecord( this.txid );
			moduleRec.dataelements.Add( txRec );
		}
	}
}
