///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using System;

namespace Darkages.Types
{
    [Flags]
    public enum StatusFlags : byte
    {
        All = StructA | StructB | StructC | StructD,

        /// <summary>
        ///     Includes: Maximum HP/MP, STR, INT, WIS, CON, DEX, Stat Points, ABP/EXP Level, Current/Maximum Weight
        /// </summary>
        StructA = 0x20,

        /// <summary>
        ///     Includes: Current HP/MP
        /// </summary>
        StructB = 0x10,

        /// <summary>
        ///     Includes: EXP Total/Next/Limit, ABP Total/Next/Limit, Game Points, Gold Points
        /// </summary>
        StructC = 0x08,

        /// <summary>
        ///     Includes: HIT, DMG, AC, MR, Offense/Defense Element, Flags
        /// </summary>
        StructD = 0x04
    }
}
