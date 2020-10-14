#region

using System;

#endregion

namespace Darkages.Types
{
    [Flags]
    public enum LootQualifer
    {
        Random = 1 << 1,
        Table = 1 << 2,
        Event = 1 << 3,
        Gold = 1 << 5,
        None = 256
    }
}