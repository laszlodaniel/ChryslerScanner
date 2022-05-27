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
using DRBDBReader.DB.Converters;

namespace DRBDBReader.DB.Records
{
	public class SCRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_MASK = 1;
		private const byte FIELD_OP = 2;

		public ushort id;

		public ushort mask;
		public Operator op;

		public SCRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.mask = (ushort)this.table.readField( this, FIELD_MASK );
			this.op = (Operator)( (byte)( ( (ushort)this.table.readField( this, FIELD_OP ) ) >> 8 ) );
		}
	}
}
