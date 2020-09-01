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
using System.IO;
using System.Linq;
using System.Threading;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB
{
	public class Database
	{
		public const ushort TABLE_MODULE = 0;
		public const ushort TABLE_DES_INFO = 1;
		public const ushort TABLE_BINARY_DATA_SPECIFIER = 2;
		public const ushort TABLE_UNKNOWN_3 = 3;
		public const ushort TABLE_CONVERTERS_STATE = 4;
		public const ushort TABLE_CONVERTERS_NUMERIC = 5;
		public const ushort TABLE_SERIVCE_CAT_STUFFS = 6;
		public const ushort TABLE_QUALIFIER = 7;
		public const ushort TABLE_DATA_ACQUISITION_DESCRIPTION = 8;
		public const ushort TABLE_DRB_MENU = 9;
		public const ushort TABLE_MODULE_DATAELEMENT = 10;
		public const ushort TABLE_UNKNOWN_11 = 11;
		public const ushort TABLE_EMPTY_12 = 12;
		public const ushort TABLE_STATE_DATA_SPECIFIER = 13;
		public const ushort TABLE_UNKNOWN_14 = 14;
		public const ushort TABLE_STATE_ENTRY = 15;
		public const ushort TABLE_STRINGS = 16;
		public const ushort TABLE_NUMERIC_DATA_SPECIFIER = 17;
		public const ushort TABLE_DATAELEMENT_QUALIFIER = 18;
		public const ushort TABLE_UNKNOWN_19 = 19;
		public const ushort TABLE_UNKNOWN_20 = 20;
		public const ushort TABLE_UNKNOWN_21 = 21;
		public const ushort TABLE_UNKNOWN_22 = 22;
		public const ushort TABLE_TRANSMIT = 23;
		public const ushort TABLE_EMPTY_24 = 24;
		public const ushort TABLE_EMPTY_25 = 25;
		public const ushort TABLE_DBTEXT_1 = 26;
		public const ushort TABLE_DBTEXT_2 = 27;

		private FileInfo dbFile;
		public SimpleBinaryReader dbReader;
		public Table[] tables;

		private static ushort[] syncedTableReadOrder = {
			TABLE_DBTEXT_1, // must come before TABLE_STRINGS
			TABLE_DBTEXT_2, // must come before TABLE_STRINGS
			TABLE_STRINGS,

			TABLE_EMPTY_12,
			TABLE_EMPTY_24,
			TABLE_EMPTY_25,
			TABLE_UNKNOWN_3,  // Field 2: string id
			TABLE_UNKNOWN_11,
			TABLE_UNKNOWN_14,
			TABLE_UNKNOWN_19,
			TABLE_UNKNOWN_20, // Field 2: looks like string id but isn't
			TABLE_UNKNOWN_21, // Field 3: string id
			TABLE_UNKNOWN_22,

			TABLE_STATE_DATA_SPECIFIER, // must come before TABLE_TRANSMIT, probably before TABLE_BINARY_DATA_SPECIFIER
			TABLE_BINARY_DATA_SPECIFIER, // must come before TABLE_TRANSMIT
			TABLE_NUMERIC_DATA_SPECIFIER, // must come before TABLE_TRANSMIT

			TABLE_CONVERTERS_STATE, // must come before TABLE_TRANSMIT
			TABLE_CONVERTERS_NUMERIC, // must come before TABLE_TRANSMIT
			TABLE_STATE_ENTRY, // must come before TABLE_TRANSMIT
			TABLE_DATA_ACQUISITION_DESCRIPTION, // must come before TABLE_TRANSMIT
			TABLE_QUALIFIER,
			TABLE_DATAELEMENT_QUALIFIER,
			TABLE_DRB_MENU,

			TABLE_DES_INFO, // must come before TABLE_TRANSMIT, maybe others?
			TABLE_SERIVCE_CAT_STUFFS, // must come before TABLE_TRANSMIT and TABLE_MODULE
			TABLE_MODULE,
			TABLE_TRANSMIT, // must come before TABLE_MODULE_DATAELEMENT
			TABLE_MODULE_DATAELEMENT // must come AFTER TABLE_MODULE
		};

		private static ushort[] primaryTableReadOrder = {
			TABLE_UNKNOWN_3,  // Field 2: string id
			TABLE_UNKNOWN_21, // Field 3: string id

			TABLE_STATE_DATA_SPECIFIER, // must come before TABLE_TRANSMIT, probably before TABLE_BINARY_DATA_SPECIFIER
			TABLE_BINARY_DATA_SPECIFIER, // must come before TABLE_TRANSMIT
			TABLE_NUMERIC_DATA_SPECIFIER, // must come before TABLE_TRANSMIT

			TABLE_STATE_ENTRY, // must come before TABLE_TRANSMIT
			TABLE_DRB_MENU,

			TABLE_DES_INFO, // must come before TABLE_TRANSMIT, maybe others?
			TABLE_SERIVCE_CAT_STUFFS, // must come before TABLE_TRANSMIT and TABLE_MODULE
			TABLE_MODULE
		};

		// Secondary tables have no string dependencies, to avoid locking in Database.getString()
		private static ushort[] secondaryTableReadOrder = {
			TABLE_DATAELEMENT_QUALIFIER, // Field 2: looks like string id but isn't
			TABLE_QUALIFIER,
			TABLE_CONVERTERS_STATE, // must come before TABLE_TRANSMIT
			TABLE_CONVERTERS_NUMERIC, // must come before TABLE_TRANSMIT
			TABLE_DATA_ACQUISITION_DESCRIPTION // must come before TABLE_TRANSMIT
		};

		private static ushort[] finalTableReadOrder = {
			TABLE_TRANSMIT, // must come before TABLE_MODULE_DATAELEMENT
			TABLE_MODULE_DATAELEMENT // must come AFTER TABLE_MODULE
		};

		private static ushort[] noDependencyTables = {
			TABLE_EMPTY_12,
			TABLE_EMPTY_24,
			TABLE_EMPTY_25,
			TABLE_UNKNOWN_11,
			TABLE_UNKNOWN_14,
			TABLE_UNKNOWN_19,
			TABLE_UNKNOWN_20, // Field 2: looks like string id but isn't
			TABLE_UNKNOWN_22
		};

		public bool isStarScanDB;
		
		public Database( FileInfo dbFile )
		{
			this.dbFile = dbFile;

			/* Since we're going to need access to this data often, lets load it into a MemoryStream.
			 * With it being about 2.5MB it's fairly cheap.
			 */
			using( FileStream fs = new FileStream( this.dbFile.FullName, FileMode.Open, FileAccess.Read ) )
			{
				this.dbReader = new SimpleBinaryReader( fs );
			}

			/* StarSCAN's database.mem has a different endianness;
			 * This detects and accounts for that as needed.
			 */
			this.isStarScanDB = this.checkStarScan();

			this.makeTables();
		}

		private bool checkStarScan()
		{
			/* Rather than try and deal with converting this to a string etc.,
			 * it's cheaper to just work with and compare bytes directly. */
			int offset = this.dbReader.rawDB.Length - 0x17;
			byte[] starscanbytes = this.dbReader.ReadBytes( ref offset, 8 );
			return starscanbytes.SequenceEqual( new byte[] { 0x53, 0x74, 0x61, 0x72, 0x53, 0x43, 0x41, 0x4E } );
		}

		private void makeTables()
		{
			int readOffset = 0;
			uint fileSize = this.dbReader.ReadUInt32( ref readOffset );
			ushort idk = this.dbReader.ReadUInt16( ref readOffset );
			ushort numTables = this.dbReader.ReadUInt16( ref readOffset );
			this.tables = new Table[numTables];
			for( ushort i = 0; i < numTables; ++i )
			{
				uint tableOffset = this.dbReader.ReadUInt32( ref readOffset );
				ushort rowCount = this.dbReader.ReadUInt16( ref readOffset );
				ushort rowSize = this.dbReader.ReadUInt16( ref readOffset );

				/* While technically the 'stated' code alone was correct, it had an issue:
				 * there are some columns with a size of 0! This is a waste.
				 * As such, empty columns are now removed and field IDs adjusted for that. */
				byte statedColCount = this.dbReader.ReadUInt8( ref readOffset );
				byte[] statedColSizes = this.dbReader.ReadBytes( ref readOffset, statedColCount );

				/* There's actually room reserved for 27 bytes after the statedColCount,
				 * so it is necessary to read past whatever bytes that go unread. */
				readOffset += 27 - statedColCount;

				List<byte> colSizes = new List<byte>();
				for( byte j = 0; j < statedColCount; ++j )
				{
					if( statedColSizes[j] != 0 )
					{
						colSizes.Add( statedColSizes[j] );
					}
				}

				this.tables[i] = new Table( this, i, tableOffset, rowCount, rowSize, (byte)colSizes.Count, colSizes.ToArray<byte>() );
			}


			//// single-threaded read version
			//this.readTables( syncedTableReadOrder );

			//// multi-threaded read version
			// The no-dependency tables can be read off right away
			Thread noDependencyTablesThread = new Thread( this.readTables );
			noDependencyTablesThread.Start( noDependencyTables );

			// Text tables should be read first.
			Thread dbTextOneThread = new Thread( this.readTables );
			Thread dbTextTwoThread = new Thread( this.readTables );
			Thread stateTableThread = new Thread( this.readTables );

			dbTextOneThread.Start( new ushort[] { TABLE_DBTEXT_1 } );
			dbTextTwoThread.Start( new ushort[] { TABLE_DBTEXT_2 } );
			dbTextTwoThread.Join();
			dbTextOneThread.Join();

			// TABLE_STRINGS depends on the first two
			stateTableThread.Start( new ushort[] { TABLE_STRINGS } );
			stateTableThread.Join();


			// Now read off the two thread-able Table sets
			Thread primaryTableThread = new Thread( this.readTables );
			Thread secondaryTableThread = new Thread( this.readTables );

			primaryTableThread.Start( primaryTableReadOrder );
			secondaryTableThread.Start( secondaryTableReadOrder );

			secondaryTableThread.Join();
			primaryTableThread.Join();

			// Finish off with the final, single-threaded Table set
			Thread finalTableThread = new Thread( this.readTables );
			finalTableThread.Start( finalTableReadOrder );
			finalTableThread.Join();

			noDependencyTablesThread.Join();
		}

		private void readTables( object tableReadOrder )
		{
			foreach( ushort x in (ushort[])tableReadOrder )
			{
				this.tables[x].readRecords();
			}
		}

		public string getString( ushort id )
		{
			Table t = this.tables[TABLE_STRINGS];
			Record recordObj = t.getRecord( id );

			if( recordObj == null )
			{
				return "(null)";
			}

			return ( (StringRecord)recordObj ).text;
		}

		public string getServiceCatString( ushort id )
		{
			Table t = this.tables[TABLE_SERIVCE_CAT_STUFFS];
			Record recordObj = t.getRecord( id, 3 );

			if( recordObj == null )
			{
				return "(null)";
			}

			ServiceCatRecord screc = (ServiceCatRecord)recordObj;

			return screc.name;
		}

		public string getDESString( ushort id )
		{
			Table t = this.tables[TABLE_DES_INFO];
			Record recordObj = t.getRecord( id, 0 );

			if( recordObj == null )
			{
				return "(null)";
			}

			DESRecord desrec = (DESRecord)recordObj;

			return desrec.name;
		}

		public string getProtocolText( ushort id )
		{
			switch( id )
			{
				case 1:
					return "J1850";
				case 53:
					return "CCD";
				case 60:
					return "SCI";
				case 103:
					return "ISO";
				case 159:
					return "Multimeter";
				case 160:
					return "J2190?";
				default:
					return "P" + id;
			}
		}

		public string getTX( uint id )
		{	
			Table t = this.tables[TABLE_TRANSMIT];
			Record recordObj = t.getRecord( id );
			if( recordObj == null )
			{
				return null;
			}
			TXRecord txrec = (TXRecord)recordObj;

			string protocolTxt = this.getProtocolText( txrec.dadRecord.protocolid ) + "; ";

			return txrec.name + ": " + protocolTxt + "xmit: " + txrec.xmitstring + "; sc: " + txrec.scname;
		}

		public string getDetailedTX( uint id )
		{
			Table t = this.tables[TABLE_TRANSMIT];
			Record recordObj = t.getRecord( id );
			if( recordObj == null )
			{
				return null;
			}
			TXRecord txrec = (TXRecord)recordObj;

			string protocolTxt = this.getProtocolText( txrec.dadRecord.protocolid ) + "; ";

			string detailText = "";
			detailText += Environment.NewLine + "dadreqlen: " + txrec.dadRecord.requestLength + "; dadresplen: " + txrec.dadRecord.responseLength + ";";
			detailText += Environment.NewLine + "dadextroff: " + txrec.dadRecord.extractOffset + "; dadextrsize: " + txrec.dadRecord.extractSize + ";";
			detailText += Environment.NewLine + "desid: " + txrec.dataelemsetid + "; desname: " + this.getDESString( txrec.dataelemsetid ) + ";";
			detailText += Environment.NewLine + "record: " + BitConverter.ToString( txrec.record ) + ";";

			return txrec.name + ": " + protocolTxt + "xmit: " + txrec.xmitstring + "; sc: " + txrec.scname + ";" + detailText;
		}

		public string getModule( ushort id )
		{
			Table t = this.tables[TABLE_MODULE];
			Record recordObj = t.getRecord( id );
			if( recordObj == null )
			{
				return null;
			}
			ModuleRecord modrec = (ModuleRecord)recordObj;

			return modrec.name + "; sc: " + modrec.scname;
		}
	}
}
