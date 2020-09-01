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
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters
{
	public class BinaryStateConverter : StateConverter
	{
		public BDSRecord bdsRecord;

		public BinaryStateConverter( Database db, byte[] record, ushort cfid, ushort dsid ) : base( db, record, cfid, dsid )
		{
		}

		protected override void buildStateList()
		{
			Table bdsTable = this.db.tables[Database.TABLE_BINARY_DATA_SPECIFIER];
			this.dsRecord = bdsTable.getRecord( this.dsid );
			this.bdsRecord = (BDSRecord)this.dsRecord;

			this.entries.Add( 0, bdsRecord.falseString );
			this.entries.Add( 1, bdsRecord.trueString );
		}

		protected override ushort getEntryID( ushort val )
		{
			switch( this.scRecord.op )
			{
				case Operator.GREATER:
					return (ushort)( val > this.scRecord.mask ? 1 : 0 );
				case Operator.LESS:
					return (ushort)( val < this.scRecord.mask ? 1 : 0 );
				case Operator.MASK_ZERO:
					return (ushort)( ( val & this.scRecord.mask ) == 0 ? 1 : 0 );
				case Operator.MASK_NOT_ZERO:
					return (ushort)( ( val & this.scRecord.mask ) != 0 ? 1 : 0 );
				case Operator.NOT_EQUAL:
					return (ushort)( val != this.scRecord.mask ? 1 : 0 );
				case Operator.EQUAL:
				default:
					return (ushort)( val == this.scRecord.mask ? 1 : 0 );
			}
		}

		public override string ToString()
		{
			string ret = base.ToString() + Environment.NewLine;
			ret = ret.Replace( Environment.NewLine + "0x00: ", Environment.NewLine + "FALSE: " );
			ret = ret.Replace( Environment.NewLine + "0x01: ", Environment.NewLine + "TRUE:  " );

			ret += Environment.NewLine + "MASK:  0x" + this.scRecord.mask.ToString( "X2" );
			ret += Environment.NewLine + "OP:    " + this.scRecord.op.ToString();

			return ret;
		}
	}
}
