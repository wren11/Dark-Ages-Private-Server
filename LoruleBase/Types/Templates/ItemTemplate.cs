using Darkages.Systems.Loot.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using static Darkages.Types.ElementManager;

namespace Darkages.Types
{
    public class ItemUpgrade : ILootDefinition
    {
        public virtual int Upgrade { get; set; }
        public virtual string Name { get; set; }
        public virtual double Weight { get; set; }
    }

    public class Common : ItemUpgrade
    {
        public override string Name => "Upgraded";
        public override double Weight => 2.5;
        public override int Upgrade => 1;
    }

    public class Uncommon : ItemUpgrade
    {
        public override string Name => "Enhanced";
        public override double Weight => 1.5;
        public override int Upgrade => 2;
    }

    public class Rare : ItemUpgrade
    {
        public override string Name => "Rare";
        public override double Weight => 0.5;
        public override int Upgrade => 3;
    }

    public class Epic : ItemUpgrade
    {
        public override string Name => "Epic";
        public override double Weight => 0.1;
        public override int Upgrade => 4;
    }

    public class Legendary : ItemUpgrade
    {
        public override string Name => "Legendary";
        public override double Weight => 0.08;
        public override int Upgrade => 5;
    }

    public class Mythical : ItemUpgrade
    {
        public override string Name => "Mythical";
        public override double Weight => 0.06;
        public override int Upgrade => 6;
    }

    public class Godly : ItemUpgrade
    {
        public override string Name => "Godly";
        public override double Weight => 0.05;
        public override int Upgrade => 7;
    }

    public class Forsaken : ItemUpgrade
    {
        public override string Name => "Forsaken";
        public override double Weight => 0.02;
        public override int Upgrade => 8;
    }


    public class ItemTemplate : Template, ILootDefinition
    {
        [JsonProperty] [Browsable(false)] public string MiniScript { get; set; }

        public bool CanStack { get; set; }

        public byte MaxStack { get; set; }

        [Browsable(false)] public ushort Image { get; set; }

        [Browsable(false)] public ushort DisplayImage { get; set; }

        [Browsable(false)] public string ScriptName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        [Category("Mods")] public StatusOperator HealthModifer { get; set; }

        [Category("Mods")] public StatusOperator ManaModifer { get; set; }

        [Category("Mods")] public StatusOperator StrModifer { get; set; }

        [Category("Mods")] public StatusOperator IntModifer { get; set; }

        [Category("Mods")] public StatusOperator WisModifer { get; set; }

        [Category("Mods")] public StatusOperator ConModifer { get; set; }

        [Category("Mods")] public StatusOperator DexModifer { get; set; }

        [Category("Mods")] public StatusOperator AcModifer { get; set; }

        [Category("Mods")] public StatusOperator MrModifer { get; set; }

        [Category("Mods")] public StatusOperator HitModifer { get; set; }

        [Category("Mods")] public StatusOperator DmgModifer { get; set; }

        [Category("Mods")] public StatusOperator RegenModifer { get; set; }


        [Category("Mods")] public SpellOperator SpellOperator { get; set; }

        [Category("Elements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Element OffenseElement { get; set; }

        [Category("Elements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Element DefenseElement { get; set; }


        [Category("Item Properties")] public byte CarryWeight { get; set; }

        [Category("Item Properties")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemFlags Flags { get; set; }

        [Category("Item Properties")] public uint MaxDurability { get; set; }

        [Category("Item Properties")] public uint Value { get; set; }


        [Browsable(false)] public int EquipmentSlot { get; set; }

        [Category("Item Properties")]
        [JsonIgnore]
        public EquipSlot EquipSlot { get; set; }

        [Browsable(false)] public string NpcKey { get; set; }

        [Category("Requirements")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Class Class { get; set; }

        [Category("Requirements")] public byte LevelRequired { get; set; }

        [Category("Item Properties")] public int DmgMin { get; set; }

        [Category("Item Properties")] public int DmgMax { get; set; }

        [Category("Item Properties")] public double DropRate { get; set; }


        [Category("Requirements")]
        [Description("What state is required of the player to use the item?")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ClassStage StageRequired { get; set; }

        [Category("Item Properties")] public bool HasPants { get; set; }

        [Category("Item Properties")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemColor Color { get; set; }

        [Browsable(false)] public string WeaponScript { get; set; }

        [Category("Item Properties")] public bool Enchantable { get; set; }

        [Category("Script")] public ActivationTrigger activationTrigger { get; set; } = ActivationTrigger.OnUse;

        [JsonIgnore]
        [Browsable(false)]
        public double Weight
        {
            get => DropRate;
            set { }
        }
    }
}