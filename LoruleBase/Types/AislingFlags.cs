#region

using System;

#endregion

namespace Darkages.Types
{
    [Flags]
    public enum AislingFlags
    {
        Normal = 1,
        SeeInvisible = 1 << 1,
        Dead = 1 << 3,
        Invisible = 1 << 4,
        SeeGhosts = 1 << 5
    }
}