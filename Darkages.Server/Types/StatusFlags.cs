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