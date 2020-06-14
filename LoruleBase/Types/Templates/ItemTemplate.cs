#region

using Darkages.Systems.Loot.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using static Darkages.Types.ElementManager;

#endregion

namespace Darkages.Types
{
    public class Common : ItemUpgrade
    {
        public override string Name => "Upgraded";
        public override int Upgrade => 1;
        public override double Weight => 2.5;
    }

    public class Epic : ItemUpgrade
    {
        public override string Name => "Epic";
        public override int Upgrade => 4;
        public override double Weight => 0.1;
    }

    public class Forsaken : ItemUpgrade
    {
        public override string Name => "Forsaken";
        public override int Upgrade => 8;
        public override double Weight => 0.02;
    }

    public class Godly : ItemUpgrade
    {
        public override string Name => "Godly";
        public override int Upgrade => 7;
        public override double Weight => 0.05;
    }

    public class ItemTemplate : Template, ILootDefinition
    {
        [Category("Mods")] public StatusOperator AcModifer { get; set; }
        [Category("Script")] public ActivationTrigger activationTrigger { get; set; } = ActivationTrigger.OnUse;
        public bool CanStack { get; set; }
        [Category("Item Properties")] public byte CarryWeight { get; set; }

        [Category("Requirements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Class Class { get; set; }

        [Category("Item Properties")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemColor Color { get; set; }

        [Category("Mods")] public StatusOperator ConModifer { get; set; }

        [Category("Elements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Element DefenseElement { get; set; }

        [Category("Mods")] public StatusOperator DexModifer { get; set; }
        [Browsable(false)] public ushort DisplayImage { get; set; }
        [Category("Item Properties")] public int DmgMax { get; set; }
        [Category("Item Properties")] public int DmgMin { get; set; }
        [Category("Mods")] public StatusOperator DmgModifer { get; set; }
        [Category("Item Properties")] public double DropRate { get; set; }
        [Category("Item Properties")] public bool Enchantable { get; set; }
        [Browsable(false)] public int EquipmentSlot { get; set; }

        [Category("Item Properties")]
        [JsonIgnore]
        public EquipSlot EquipSlot { get; set; }

        [Category("Item Properties")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemFlags Flags { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        [Category("Item Properties")] public bool HasPants { get; set; }
        [Category("Mods")] public StatusOperator HealthModifer { get; set; }
        [Category("Mods")] public StatusOperator HitModifer { get; set; }
        [Browsable(false)] public ushort Image { get; set; }
        [Category("Mods")] public StatusOperator IntModifer { get; set; }
        [Category("Requirements")] public byte LevelRequired { get; set; }
        [Category("Mods")] public StatusOperator ManaModifer { get; set; }
        [Category("Item Properties")] public uint MaxDurability { get; set; }
        public byte MaxStack { get; set; }
        [JsonProperty] [Browsable(false)] public string MiniScript { get; set; }
        [Category("Mods")] public StatusOperator MrModifer { get; set; }
        [Browsable(false)] public string NpcKey { get; set; }

        [Category("Elements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Element OffenseElement { get; set; }

        [Category("Mods")] public StatusOperator RegenModifer { get; set; }
        [Browsable(false)] public string ScriptName { get; set; }
        [Category("Mods")] public SpellOperator SpellOperator { get; set; }

        [Category("Requirements")]
        [Description("What state is required of the player to use the item?")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ClassStage StageRequired { get; set; }

        [Category("Mods")] public StatusOperator StrModifer { get; set; }
        [Category("Item Properties")] public uint Value { get; set; }
        [Browsable(false)] public string WeaponScript { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public double Weight
        {
            get => DropRate;
            set { }
        }

        [Category("Mods")] public StatusOperator WisModifer { get; set; }
    }

    public class ItemUpgrade : ILootDefinition
    {
        public virtual string Name { get; set; }
        public virtual int Upgrade { get; set; }
        public virtual double Weight { get; set; }
    }

    public class Legendary : ItemUpgrade
    {
        public override string Name => "Legendary";
        public override int Upgrade => 5;
        public override double Weight => 0.08;
    }

    public class Mythical : ItemUpgrade
    {
        public override string Name => "Mythical";
        public override int Upgrade => 6;
        public override double Weight => 0.06;
    }

    public class Rare : ItemUpgrade
    {
        public override string Name => "Rare";
        public override int Upgrade => 3;
        public override double Weight => 0.5;
    }

    public class Uncommon : ItemUpgrade
    {
        public override string Name => "Enhanced";
        public override int Upgrade => 2;
        public override double Weight => 1.5;
    }
}