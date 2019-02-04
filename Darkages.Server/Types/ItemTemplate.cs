///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Script.Context;
using Darkages.Systems.Loot.Interfaces;
using Newtonsoft.Json;
using System;
using static Darkages.Types.ElementManager;

namespace Darkages.Types
{
    public class ItemUpgrade : ILootDefinition
    {
        public virtual string Name { get; set; }
        public virtual double Weight { get; set; }
        public virtual int Upgrade { get; set; }
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
        [JsonProperty]
        public string MiniScript{ get; set; }

        public int ID { get; set; }

        public bool CanStack { get; set; }

        public byte MaxStack { get; set; }

        public ushort Image { get; set; }

        public ushort DisplayImage { get; set; }

        public string ScriptName { get; set; }

        public Gender Gender { get; set; }

        public StatusOperator HealthModifer { get; set; }

        public StatusOperator ManaModifer { get; set; }

        public StatusOperator StrModifer { get; set; }

        public StatusOperator IntModifer { get; set; }

        public StatusOperator WisModifer { get; set; }

        public StatusOperator ConModifer { get; set; }

        public StatusOperator DexModifer { get; set; }

        public StatusOperator AcModifer { get; set; }

        public StatusOperator MrModifer { get; set; }

        public StatusOperator HitModifer { get; set; }

        public StatusOperator DmgModifer { get; set; }

        public SpellOperator SpellOperator { get; set; }

        public Element OffenseElement { get; set; }

        public Element DefenseElement { get; set; }

        public byte CarryWeight { get; set; }

        public ItemFlags Flags { get; set; }

        public uint MaxDurability { get; set; }

        public uint Value { get; set; }

        public int EquipmentSlot { get; set; }

        public string NpcKey { get; set; }

        public Class Class { get; set; }

        public byte LevelRequired { get; set; }

        public int DmgMin { get; set; }

        public int DmgMax { get; set; }

        public double DropRate { get; set; }

        public ClassStage StageRequired { get; set; }

        public bool HasPants { get; set; }

        public ItemColor Color { get; set; }

        public string WeaponScript { get; set; }

        [JsonIgnore]
        public double Weight
        {
            get => DropRate; set { }
        }


        public void RunMiniScript(GameClient client)
        {
            Context.Items["client"] = client;
            Context.Items["user"] = client.Aisling;

            "var client = (GameClient)Context.Items[\"client\"];".Run();
            "var user   = (Sprite)Context.Items[\"user\"];".Run();

            try
            {
                MiniScript.Run();
            }
            catch (Exception)
            {
                //ingore
            }
        }
    }
}
