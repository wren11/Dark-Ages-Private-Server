using System;

namespace Darkages.Types
{
    [Flags]
    public enum MoodQualifer
    {
        Idle = 1,
        Aggressive = 2,
        Unpredicable = 4,
        Neutral = 8,
        VeryAggressive = 16
    }
}