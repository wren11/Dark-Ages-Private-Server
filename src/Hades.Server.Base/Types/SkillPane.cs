#region

using System;

#endregion

namespace Darkages.Types
{
    [Flags]
    public enum Pane
    {
        Inventory = 0,
        Spells = 1,
        Skills = 2,
        Tools = 3
    }
}