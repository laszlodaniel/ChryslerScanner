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
namespace DRBDBReader.DB.Converters
{
	public enum Operator : byte
	{
		EQUAL = 0x3D,
		NOT_EQUAL = 0x21,
		GREATER = 0x3E,
		LESS = 0x3C,
		MASK_ZERO = 0x30,
		MASK_NOT_ZERO = 0x39
	}
}
