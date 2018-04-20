using System;

namespace Darkages.Types
{
    [Flags]
    public enum AislingFlags : byte
    {
        Normal = 0,
        SeeInvisible = 1,
        GM = 2,
        Dead = 4,
        Invisible = 8
    }
}