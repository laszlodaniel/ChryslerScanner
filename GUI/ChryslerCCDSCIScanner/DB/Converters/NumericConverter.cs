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
	public class NumericConverter : Converter
	{
		public NCRecord ncRecord;
		public NDSRecord ndsRecord;

		public NumericConverter( Database db, byte[] record, ushort cfid, ushort dsid ) : base( db, record, cfid, dsid )
		{
			Table numConvTable = this.db.tables[Database.TABLE_CONVERTERS_NUMERIC];
			this.ncRecord = (NCRecord)numConvTable.getRecord( this.cfid );

			Table ndsTable = this.db.tables[Database.TABLE_NUMERIC_DATA_SPECIFIER];
			this.dsRecord = ndsTable.getRecord( this.dsid );
			this.ndsRecord = (NDSRecord)this.dsRecord;
		}

		public override string processData( long data, bool outputMetric = false )
		{
			decimal result = data * (decimal)this.ncRecord.slope + (decimal)this.ncRecord.offset;
			string unit = this.ndsRecord.imperialUnitString;
			if( outputMetric )
			{
				result = result * (decimal)this.ndsRecord.metricConvSlope + (decimal)this.ndsRecord.metricConvOffset;
				unit = this.ndsRecord.metricUnitString;
			}
			return result + " " + unit;
		}

		public override string ToString()
		{
			string ret = base.ToString() + Environment.NewLine;

			if( this.ndsRecord.imperialUnitStrId == this.ndsRecord.metricUnitStrId )
			{
				ret += Environment.NewLine + "UNIT: " + getUnitToStringOutput( this.ndsRecord.imperialUnitString );
			}
			else
			{
				ret += Environment.NewLine + "UNIT (DFLT/MTRC): ";
				ret += getUnitToStringOutput( this.ndsRecord.imperialUnitString );
				ret += "/" + getUnitToStringOutput( this.ndsRecord.metricUnitString );
			}

			ret += Environment.NewLine + "SLOPE:  " + this.ncRecord.slope;
			ret += Environment.NewLine + "OFFSET: " + this.ncRecord.offset;
			ret += Environment.NewLine + "SLCONV: " + this.ndsRecord.metricConvSlope;
			ret += Environment.NewLine + "OFCONV: " + this.ndsRecord.metricConvOffset;

			return ret;
		}

		private static string getUnitToStringOutput( string unit )
		{
			if( string.IsNullOrWhiteSpace( unit ) )
			{
				return "(null)";
			}
			return unit;
		}
	}
}
