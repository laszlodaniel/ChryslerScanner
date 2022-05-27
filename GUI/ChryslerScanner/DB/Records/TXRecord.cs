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
using DRBDBReader.DB.Converters;

namespace DRBDBReader.DB.Records
{
	public class TXRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_CONVERSION = 1;
		private const byte FIELD_DATA_AQU_DESC_ID = 2;
		private const byte FIELD_DATA_ELEM_SET_ID = 4;
		private const byte FIELD_TXBYTES = 6;
		private const byte FIELD_STRING_ID = 8;
		private const byte FIELD_SVCCAT_ID = 14;

		public uint id;

		public ushort dadid;
		public DADRecord dadRecord;

		public ushort dataelemsetid;

		public byte[] xmitbytes;
		public string xmitstring;

		public ushort nameid;
		public string name;

		public ushort scid;
		public string scname;

		public Converter converter;

		public TXRecord( Table table, byte[] record ) : base( table, record )
		{
			// get id
			this.id = (uint)this.table.readField( this, FIELD_ID );


			// get converter info
			byte[] convertfield = this.table.readFieldRaw( this, FIELD_CONVERSION );

			ushort dsid = (ushort)this.table.readInternal( convertfield, 2, 2 );
			ushort cfid = (ushort)this.table.readInternal( convertfield, 4, 2 );

			switch( (Converter.Types)convertfield[0] )
			{
				case Converter.Types.BINARY_STATE:
					this.converter = this.getBSC( convertfield, cfid, dsid );
					break;
				case Converter.Types.NUMERIC:
					this.converter = this.getNC( convertfield, cfid, dsid );
					break;
				case Converter.Types.STATE:
					this.converter = this.getSC( convertfield, cfid, dsid );
					break;
				case Converter.Types.UNKNOWN_x2:
				case Converter.Types.UNKNOWN_x12:
				case Converter.Types.UNKNOWN_x22:
					this.converter = this.getUC( convertfield, cfid, dsid );
					break;
				default:
					this.converter = new Converter( this.table.db, convertfield, cfid, dsid );
					break;
			}


			// get protocol info
			this.dadid = (ushort)this.table.readField( this, FIELD_DATA_AQU_DESC_ID );
			Table dadTable = this.table.db.tables[Database.TABLE_DATA_ACQUISITION_DESCRIPTION];
			this.dadRecord = (DADRecord)dadTable.getRecord( this.dadid );


			// get data elem set stuff
			this.dataelemsetid = (ushort)this.table.readField( this, FIELD_DATA_ELEM_SET_ID );


			// get xmitbytes
			int index = this.table.getColumnOffset( FIELD_TXBYTES );
			this.xmitbytes = new byte[this.record[index]];
			Array.Copy( this.record, index + 1, this.xmitbytes, 0, this.xmitbytes.Length );

			this.xmitstring = BitConverter.ToString( this.xmitbytes );


			// get name
			this.nameid = (ushort)this.table.readField( this, FIELD_STRING_ID );
			this.name = this.table.db.getString( this.nameid );


			// get scid/scname
			this.scid = (ushort)((int)(this.table.readField( this, FIELD_SVCCAT_ID ) >> 8));
			this.scname = this.table.db.getServiceCatString( this.scid );
		}

		private BinaryStateConverter getBSC( byte[] convertfield, ushort cfid, ushort dsid )
		{
			if( this.table.txBSCCache.ContainsKey( cfid ) )
			{
				if( this.table.txBSCCache[cfid].ContainsKey( dsid ) )
				{
					return this.table.txBSCCache[cfid][dsid];
				}
			}
			else
			{
				this.table.txBSCCache[cfid] = new Dictionary<ushort, BinaryStateConverter>();
			}
			this.table.txBSCCache[cfid][dsid] = new BinaryStateConverter( this.table.db, convertfield, cfid, dsid );
			return this.table.txBSCCache[cfid][dsid];
		}

		private StateConverter getSC( byte[] convertfield, ushort cfid, ushort dsid )
		{
			if( this.table.txSCCache.ContainsKey( cfid ) )
			{
				if( this.table.txSCCache[cfid].ContainsKey( dsid ) )
				{
					return this.table.txSCCache[cfid][dsid];
				}
			}
			else
			{
				this.table.txSCCache[cfid] = new Dictionary<ushort, StateConverter>();
			}
			this.table.txSCCache[cfid][dsid] = new StateConverter( this.table.db, convertfield, cfid, dsid );
			return this.table.txSCCache[cfid][dsid];
		}

		private NumericConverter getNC( byte[] convertfield, ushort cfid, ushort dsid )
		{
			if( this.table.txNCCache.ContainsKey( cfid ) )
			{
				if( this.table.txNCCache[cfid].ContainsKey( dsid ) )
				{
					return this.table.txNCCache[cfid][dsid];
				}
			}
			else
			{
				this.table.txNCCache[cfid] = new Dictionary<ushort, NumericConverter>();
			}
			this.table.txNCCache[cfid][dsid] = new NumericConverter( this.table.db, convertfield, cfid, dsid );
			return this.table.txNCCache[cfid][dsid];
		}

		private UnknownConverter getUC( byte[] convertfield, ushort cfid, ushort dsid )
		{
			if( this.table.txUCCache.ContainsKey( cfid ) )
			{
				if( this.table.txUCCache[cfid].ContainsKey( dsid ) )
				{
					return this.table.txUCCache[cfid][dsid];
				}
			}
			else
			{
				this.table.txUCCache[cfid] = new Dictionary<ushort, UnknownConverter>();
			}
			this.table.txUCCache[cfid][dsid] = new UnknownConverter( this.table.db, convertfield, cfid, dsid );
			return this.table.txUCCache[cfid][dsid];
		}
	}
}
