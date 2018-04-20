using System;

namespace Darkages.Types
{
    [Flags]
    public enum ItemFlags
    {
        Equipable = 1,
        Perishable = 1 << 1,
        Tradeable = 1 << 2,
        Dropable = 1 << 3,
        Bankable = 1 << 4,
        Sellable = 1 << 5,
        Repairable = 1 << 6,
        Stackable = 1 << 7,
        Consumable = 1 << 8,
        Elemental = 1 << 10,
        QuestRelated = 1 << 11,
        Upgradeable = 1 << 12,
        TwoHanded = 1 << 13
    }
}