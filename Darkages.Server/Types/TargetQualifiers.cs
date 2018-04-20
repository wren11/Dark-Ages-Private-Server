using System;

namespace Darkages.Types
{
    [Flags]
    public enum TargetQualifiers
    {
        Single = 1,
        Radius = 2,
        Adjacent = 3,
        Parallel = 4,
        All = 5,
        Infront = 6
    }
}