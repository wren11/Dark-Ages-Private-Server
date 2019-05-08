using Darkages.Types;
using System;
using System.ComponentModel;

namespace Content_Maker
{

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
    {
        public EnumDisplayNameAttribute(string data) : base(data) { }
    }

    public enum Class : int
    {
        [EnumDisplayName("Any Class")]
        Peasant,
        [EnumDisplayName("Warriors")]
        Warrior,
        [EnumDisplayName("Roguess")]
        Rogue,
        [EnumDisplayName("Wizards")]
        Wizard,
        [EnumDisplayName("Priests")]
        Priest,
        [EnumDisplayName("Monks")]
        Monk
    }

    public enum Stage : int
    {
        [EnumDisplayName("Normal Aislings")]
        Normal,
        [EnumDisplayName("Only Masters")]
        Masters,
        [EnumDisplayName("Only SubClass Masters")]
        SubMaster,
        [EnumDisplayName("Only PureClass Masters")]
        PureMaster
    }

    public class ArmorTemplate
    {

        [Category("General")]
        public string Name { get; set; }

        [Category("General")]
        public int Value { get; set; } = 5000;

        [Category("Appearence")]
        public bool RenderPants { get; set; } = false;

        [Category("World")]
        public double DropRate { get; set; } = 0.20;

        [Category("World")]
        public string NPCKEY { get; set; } = "Blacksmith";

        [Category("General")]
        public int Durability { get; set; } = 10000;

        [Category("Flags")]
        public bool CanDrop { get; set; } = true;

        [Category("Flags")]
        public bool CanRepair { get; set; } = true;

        [Category("Flags")]
        public bool CanSell { get; set; } = true;

        [Category("Flags")]
        public bool CanTrade { get; set; } = true;

        [Category("Flags")]
        public bool CanEquip { get; set; } = true;

        [Category("Requirements")]
        public int LevelRequired { get; set; } = 1;

        [Category("Requirements")]
        public Class AislingPath { get; set; } = Class.Peasant;

        [Category("Requirements")]
        public Stage AislingStage { get; set; } = Stage.Normal;

        [Category("+ Attributes")]
        public int AddArmor { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveArmor { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddHP { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveHP { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddMR { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveMR { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddMP { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveMP { get; set; } = 0;

        [Category("General")]
        public string Script { get; set; } = "Armor";

        [Category("+ Attributes")]
        public int AddStr { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveStr { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddWis { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveWis { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddInt { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveInt { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddCon { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveCon { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddDex { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveDex { get; set; } = 0;


        [Category("- Attributes")]
        public int RemoveHit { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddHit { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddDMG { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveDMG { get; set; } = 0;

        [Category("+ Attributes")]
        public int AddRegen { get; set; } = 0;

        [Category("- Attributes")]
        public int RemoveRegen { get; set; } = 0;


        public ArmorTemplate()
        {

        }


        public ItemTemplate Compile()
        {
            var template = new ItemTemplate();

            return template;
        }
    }
}