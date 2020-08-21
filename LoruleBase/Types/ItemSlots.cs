using System.ComponentModel;

namespace Darkages.Types
{
    public enum EquipSlot
    {
        None = 0,
        [Description("Weapons")]
        Weapon = 1,
        [Description("Armors")]
        Armor = 2,
        [Description("Shields")]
        Shield = 3,
        [Description("Helmets")]
        Helmet = 4,
        [Description("Earrings")]
        Earring = 5,
        [Description("Necklaces")]
        Necklace = 6,
        [Description("Rings")]
        LHand = 7,
        [Description("Rings")]
        RHand = 8,
        [Description("Gauntlets")]
        LArm = 9,
        [Description("Gauntlets")]
        RArm = 10,
        [Description("Belts")]
        Waist = 11,
        [Description("Greaves")]
        Leg = 12,
        [Description("Boots")]
        Foot = 13,
        [Description("Accessories")]
        FirstAcc = 14,
        [Description("Accessories")]
        Trousers = 15,
        [Description("Accessories")]
        Coat = 16,
        [Description("Accessories")]
        SecondAcc = 17
    }

    public static class ItemSlots
    {
        public const int Armor = 2;
        public const int Coat = 16;
        public const int Earring = 5;
        public const int FirstAcc = 14;
        public const int Foot = 13;
        public const int Helmet = 4;
        public const int LArm = 9;
        public const int Leg = 12;
        public const int LHand = 7;
        public const int Necklace = 6;
        public const int None = 0;
        public const int RArm = 10;
        public const int RHand = 8;
        public const int SecondAcc = 17;
        public const int Shield = 3;
        public const int Trousers = 15;
        public const int Waist = 11;
        public const int Weapon = 1;
    }
}