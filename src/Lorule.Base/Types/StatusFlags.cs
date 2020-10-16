#region

using System;

#endregion

namespace Darkages.Types
{
    [Flags]
    public enum StatusFlags : byte
    {
        UnreadMail = 0x01,
        Unknown = 0x02,
        StructA = 0x20,
        StructB = 0x10,
        StructC = 0x08,
        StructD = 0x04,
        GameMasterA = 0x40,
        GameMasterB = 0x80,
        Swimming = GameMasterA | GameMasterB,
        All = StructA | StructB | StructC | StructD
    }
}