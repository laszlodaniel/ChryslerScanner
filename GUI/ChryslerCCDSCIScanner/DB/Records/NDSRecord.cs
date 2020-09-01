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
using System;

namespace DRBDBReader.DB.Records
{
	public class NDSRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_METRIC_CONV_SLOPE = 2;
		private const byte FIELD_UNIT_METRIC_STR_ID = 3;
		private const byte FIELD_METRIC_CONV_OFFSET = 4;
		private const byte FIELD_UNIT_IMPERIAL_STR_ID = 6;

		public ushort id;

		public ushort metricUnitStrId;
		public string metricUnitString;

		public ushort imperialUnitStrId;
		public string imperialUnitString;

		// To convert imperial unit input to metric: input * slope + offset
		// NOTE: in some cases the 'imperialUnitString' and 'metricUnitString' are "backwards."
		//       further investigation is required, but presumably this is for discrepencies when
		//       the car outputs data in metric by default when everything else is imperial or w/e
		public float metricConvSlope;
		public float metricConvOffset;

		public NDSRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.metricUnitStrId = (ushort)this.table.readField( this, FIELD_UNIT_METRIC_STR_ID );
			this.metricUnitString = ( this.metricUnitStrId != 0 ? this.table.db.getString( this.metricUnitStrId ) : "" );

			this.imperialUnitStrId = (ushort)this.table.readField( this, FIELD_UNIT_IMPERIAL_STR_ID );
			this.imperialUnitString = ( this.imperialUnitStrId != 0 ? this.table.db.getString( this.imperialUnitStrId ) : "" );

			this.metricConvSlope = BitConverter.ToSingle( BitConverter.GetBytes( (int)this.table.readField( this, FIELD_METRIC_CONV_SLOPE ) ), 0 );
			this.metricConvOffset = BitConverter.ToSingle( BitConverter.GetBytes( (int)this.table.readField( this, FIELD_METRIC_CONV_OFFSET ) ), 0 );
		}
	}
}
