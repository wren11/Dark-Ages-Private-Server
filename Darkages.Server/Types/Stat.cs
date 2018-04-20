using System;

namespace Darkages.Types
{
    [Flags]
    public enum Stat
    {
        Str = 0x01,
        Int = 0x04,
        Wis = 0x08,
        Con = 0x10,
        Dex = 0x02
    }
}