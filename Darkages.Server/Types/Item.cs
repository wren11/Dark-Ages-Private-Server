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
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Darkages.Types
{
    public class Item : Sprite
    {
        public ItemTemplate Template { get; set; }

        [JsonIgnore] public ItemScript Script { get; set; }

        [JsonIgnore] public WeaponScript WeaponScript { get; set; }

        [JsonIgnore] public Sprite[] AuthenticatedAislings { get; set; }

        public bool Cursed { get; set; }

        public uint Owner { get; set; }

        public ushort Image { get; set; }

        public ushort DisplayImage { get; set; }

        public ushort Stacks { get; set; }

        public byte Slot { get; set; }

        public uint Durability { get; set; }

        public bool[] Warnings { get; set; }

        public byte Color { get; set; }

        public int Upgrades { get; set; }

        public Type Type { get; set; }


        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (Upgrades == 4)
                {
                    return "{=fRare " + Template.Name;
                }

                if (Upgrades == 5)
                {
                    return "{=pEpic " + Template.Name;
                }

                if (Upgrades == 6)
                {
                    return "{=sLegendary " + Template.Name;
                }

                if (Upgrades == 7)
                {
                    return "{=bGodly " + Template.Name;
                }

                if (Upgrades == 8)
                {
                    return "{=uForsaken " + Template.Name;
                }


                return Template.Name;
            }
        }

        [JsonIgnore]
        public bool Equipped { get; set; }

        public bool CanCarry(Sprite sprite)
        {
            if ((sprite as Aisling).CurrentWeight + Template.CarryWeight > (sprite as Aisling).MaximumWeight)
            {
                (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, ServerContext.Config.ToWeakToLift);
                return false;
            }

            return true;
        }

        public bool GiveTo(Sprite sprite, bool CheckWeight = true)
        {
            if (sprite is Aisling)
            {
                #region stackable items 
                if (Template.Flags.HasFlag(ItemFlags.Stackable))
                {
                    var num_stacks = (byte)Stacks;

                    if (num_stacks <= 0)
                        num_stacks = 1;

                    //find first item in inventory that is stackable with the same name.
                    var item = (sprite as Aisling).Inventory.Get(i => i != null && i.Template.Name == Template.Name
                                                                                && i.Stacks + num_stacks <
                                                                                i.Template.MaxStack).FirstOrDefault();

                    if (item != null) // current stack
                    {
                        //use the same slot as this stack we found of the same item.
                        Slot = item.Slot;

                        //update the stack quanity.
                        item.Stacks += num_stacks;

                        //refresh this item slot.
                        (sprite as Aisling).Client.Aisling.Inventory.Set(item, false);

                        //send remove packet.
                        (sprite as Aisling).Client.Send(new ServerFormat10(item.Slot));

                        //add it again with updated information.
                        (sprite as Aisling).Client.Send(new ServerFormat0F(item));

                        //send message
                        (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02,
                            string.Format("Received {0}, You now have ({1})", DisplayName, item.Stacks == 0 ? item.Stacks + 1 : item.Stacks));

                        return true;
                    }
                    else // it's the first item of a stack.
                    {
                        //if we don't find an existing item of this stack, create a new stack.
                        if (Stacks <= 0)
                            Stacks = 1;

                        if (CheckWeight)
                        {
                            if (!CanCarry(sprite))
                            {
                                return false;
                            }
                        }

                        Slot = (sprite as Aisling).Inventory.FindEmpty();

                        if (Slot == byte.MaxValue)
                        {
                            (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, ServerContext.Config.CantCarryMoreMsg);
                            return false;
                        }

                    //assign this item to the inventory.
                    (sprite as Aisling).Inventory.Set(this, false);
                        var format = new ServerFormat0F(this);
                        (sprite as Aisling).Show(Scope.Self, format);
                        (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02,
                            string.Format("{0} Received.", DisplayName));

                        if (CheckWeight)
                        {
                            (sprite as Aisling).CurrentWeight += Template.CarryWeight;
                            (sprite as Aisling).Client.SendStats(StatusFlags.StructA);
                        }

                        return true;
                    }
                }
                #endregion
                #region not stackable items
                else
                {
                    //not stackable. find inventory slot.
                    Slot = (sprite as Aisling).Inventory.FindEmpty();

                    if (Slot == byte.MaxValue)
                    {
                        (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, ServerContext.Config.CantCarryMoreMsg);
                        return false;
                    }

                    if (CheckWeight)
                    {
                        if (!CanCarry(sprite))
                        {
                            return false;
                        }
                    }


                    //assign this item to the inventory.
                    (sprite as Aisling).Inventory.Assign(this);
                    var format = new ServerFormat0F(this);
                    (sprite as Aisling).Show(Scope.Self, format);

                    if (CheckWeight)
                    {
                        (sprite as Aisling).CurrentWeight += Template.CarryWeight;
                        (sprite as Aisling).Client?.SendStats(StatusFlags.StructA);
                    }

                    return true;
                }
                #endregion
            }

            return false;
        }

        public void RemoveModifiers(GameClient client)
        {
            if (client == null || client.Aisling == null)
                return;

            #region Armor class Modifers

            if (Template.AcModifer != null)
            {
                //Inverted Modifier
                if (Template.AcModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusAc += Template.AcModifer.Value;
                if (Template.AcModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusAc -= Template.AcModifer.Value;

                client.SendMessage(0x03, string.Format("E: {0}, AC: {1}", Template.Name, client.Aisling.Ac));
                client.SendStats(StatusFlags.StructD);
            }

            #endregion

            #region Lines

            if (Template.SpellOperator != null)
            {
                var op = Template.SpellOperator;

                for (var i = 0; i < client.Aisling.SpellBook.Spells.Count; i++)
                {
                    var spell = client.Aisling.SpellBook.FindInSlot(i);

                    if (spell == null)
                        continue;

                    if (spell.Template == null)
                        continue;

                    spell.Lines = spell.Template.BaseLines;

                    if (spell.Lines > spell.Template.MaxLines)
                        spell.Lines = spell.Template.MaxLines;

                    UpdateSpellSlot(client, spell.Slot);
                }
            }

            #endregion

            #region MR

            if (Template.MrModifer != null)
            {
                if (Template.MrModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusMr -= (byte)Template.MrModifer.Value;
                if (Template.MrModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusMr += (byte)Template.MrModifer.Value;

                if (client.Aisling.BonusMr < 0)
                    client.Aisling.BonusMr = 0;
            }

            #endregion

            #region Health

            if (Template.HealthModifer != null)
            {
                if (Template.HealthModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusHp -= Template.HealthModifer.Value;
                if (Template.HealthModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusHp += Template.HealthModifer.Value;

                if (client.Aisling.BonusHp < 0)
                    client.Aisling.BonusHp = ServerContext.Config.MinimumHp;
            }

            #endregion

            #region Mana

            if (Template.ManaModifer != null)
            {
                if (Template.ManaModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusMp -= Template.ManaModifer.Value;
                if (Template.ManaModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusMp += Template.ManaModifer.Value;

                if (client.Aisling.BonusMp < 0)
                    client.Aisling.BonusMp = ServerContext.Config.MinimumHp;
            }

            #endregion

            #region Str

            if (Template.StrModifer != null)
            {
                if (Template.StrModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusStr -= (byte)Template.StrModifer.Value;
                if (Template.StrModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusStr += (byte)Template.StrModifer.Value;
            }

            #endregion

            #region Int

            if (Template.IntModifer != null)
            {
                if (Template.IntModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusInt -= (byte)Template.IntModifer.Value;
                if (Template.IntModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusInt += (byte)Template.IntModifer.Value;
            }

            #endregion

            #region Wis

            if (Template.WisModifer != null)
            {
                if (Template.WisModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusWis -= (byte)Template.WisModifer.Value;
                if (Template.WisModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusWis += (byte)Template.WisModifer.Value;
            }

            #endregion

            #region Con

            if (Template.ConModifer != null)
            {
                if (Template.ConModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusCon -= (byte)Template.ConModifer.Value;
                if (Template.ConModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusCon += (byte)Template.ConModifer.Value;

                if (client.Aisling.BonusCon < 0)
                    client.Aisling.BonusCon = ServerContext.Config.BaseStatAttribute;
                if (client.Aisling.BonusCon > 255)
                    client.Aisling.BonusCon = 255;
            }

            #endregion

            #region Dex

            if (Template.DexModifer != null)
            {
                if (Template.DexModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusDex -= (byte)Template.DexModifer.Value;
                if (Template.DexModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusDex += (byte)Template.DexModifer.Value;
            }

            #endregion

            #region Hit

            if (Template.HitModifer != null)
            {
                if (Template.HitModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusHit -= (byte)Template.HitModifer.Value;
                if (Template.HitModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusHit += (byte)Template.HitModifer.Value;
            }

            #endregion

            #region Dmg

            if (Template.DmgModifer != null)
            {
                if (Template.DmgModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusDmg -= (byte)Template.DmgModifer.Value;
                if (Template.DmgModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusDmg += (byte)Template.DmgModifer.Value;
            }

            #endregion
        }

        public void ApplyModifers(GameClient client)
        {
            if (client == null || client.Aisling == null)
                return;

            #region Armor class Modifers

            if (Template.AcModifer != null)
            {
                //Inverted
                if (Template.AcModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusAc -= Template.AcModifer.Value;
                if (Template.AcModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusAc += Template.AcModifer.Value;

                client.SendMessage(0x03, string.Format("E: {0}, AC: {1}", Template.Name, client.Aisling.Ac));
                client.SendStats(StatusFlags.StructD);
            }

            #endregion

            #region Lines

            if (Template.SpellOperator != null)
            {
                var op = Template.SpellOperator;

                for (var i = 0; i < client.Aisling.SpellBook.Spells.Count; i++)
                {
                    var spell = client.Aisling.SpellBook.FindInSlot(i);

                    if (spell == null || spell.Template == null)
                        continue;

                    if (op.Option == SpellOperator.SpellOperatorPolicy.Decrease)
                    {
                        spell.Lines = spell.Template.BaseLines;

                        if (op.Scope == SpellOperator.SpellOperatorScope.cradh)
                        {
                            if (spell.Template.Name.Contains("cradh")) spell.Lines -= op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.ioc)
                        {
                            if (spell.Template.Name.Contains("ioc")) spell.Lines -= op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.nadur)
                        {
                            if (spell.Template.Name.Contains("nadur")) spell.Lines -= op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.all)
                        {
                            spell.Lines -= op.Value;
                        }
                    }

                    if (op.Option == SpellOperator.SpellOperatorPolicy.Set)
                        if (op.Scope == SpellOperator.SpellOperatorScope.cradh)
                        {
                            if (spell.Template.Name.Contains("cradh")) spell.Lines = op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.ioc)
                        {
                            if (spell.Template.Name.Contains("ioc")) spell.Lines = op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.nadur)
                        {
                            if (spell.Template.Name.Contains("nadur")) spell.Lines = op.Value;
                        }
                        else if (op.Scope == SpellOperator.SpellOperatorScope.all)
                        {
                            spell.Lines = op.Value;
                        }

                    if (spell.Lines < spell.Template.MinLines)
                        spell.Lines = spell.Template.MinLines;

                    UpdateSpellSlot(client, spell.Slot);
                }
            }

            #endregion

            #region MR

            if (Template.MrModifer != null)
            {
                if (Template.MrModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusMr += (byte)Template.MrModifer.Value;
                if (Template.MrModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusMr -= (byte)Template.MrModifer.Value;

                if (client.Aisling.BonusMr < 0)
                    client.Aisling.BonusMr = 0;
                if (client.Aisling.BonusMr > 70)
                    client.Aisling.BonusMr = 70;
            }

            #endregion

            #region Health

            if (Template.HealthModifer != null)
            {
                if (Template.HealthModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusHp += Template.HealthModifer.Value;
                if (Template.HealthModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusHp -= Template.HealthModifer.Value;

                if (client.Aisling.BonusHp < 0)
                    client.Aisling.BonusHp = ServerContext.Config.MinimumHp;
            }

            #endregion

            #region Mana

            if (Template.ManaModifer != null)
            {
                if (Template.ManaModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusMp += Template.ManaModifer.Value;
                if (Template.ManaModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusMp -= Template.ManaModifer.Value;

                if (client.Aisling.BonusMp < 0)
                    client.Aisling.BonusMp = ServerContext.Config.MinimumHp;
            }

            #endregion

            #region Str

            if (Template.StrModifer != null)
            {
                if (Template.StrModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusStr += (byte)Template.StrModifer.Value;
                if (Template.StrModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusStr -= (byte)Template.StrModifer.Value;

            }

            #endregion

            #region Int

            if (Template.IntModifer != null)
            {
                if (Template.IntModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusInt += (byte)Template.IntModifer.Value;
                if (Template.IntModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusInt -= (byte)Template.IntModifer.Value;
            }

            #endregion

            #region Wis

            if (Template.WisModifer != null)
            {
                if (Template.WisModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusWis += (byte)Template.WisModifer.Value;
                if (Template.WisModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusWis -= (byte)Template.WisModifer.Value;
            }

            #endregion

            #region Con

            if (Template.ConModifer != null)
            {
                if (Template.ConModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusCon += (byte)Template.ConModifer.Value;
                if (Template.ConModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusCon -= (byte)Template.ConModifer.Value;
            }

            #endregion

            #region Dex

            if (Template.DexModifer != null)
            {
                if (Template.DexModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusDex += (byte)Template.DexModifer.Value;
                if (Template.DexModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusDex -= (byte)Template.DexModifer.Value;
            }

            #endregion

            #region Hit

            if (Template.HitModifer != null)
            {
                if (Template.HitModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusHit += (byte)Template.HitModifer.Value;
                if (Template.HitModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusHit -= (byte)Template.HitModifer.Value;
            }

            #endregion

            #region Dmg

            if (Template.DmgModifer != null)
            {
                if (Template.DmgModifer.Option == StatusOperator.Operator.Add)
                    client.Aisling.BonusDmg += (byte)Template.DmgModifer.Value;
                if (Template.DmgModifer.Option == StatusOperator.Operator.Remove)
                    client.Aisling.BonusDmg -= (byte)Template.DmgModifer.Value;
            }

            #endregion
        }

        public void UpdateSpellSlot(GameClient client, byte slot)
        {
            var a = client.Aisling.SpellBook.Remove(slot);
            client.Send(new ServerFormat18(slot));

            if (a != null)
            {
                a.Slot = slot;
                client.Aisling.SpellBook.Set(a, false);
                client.Send(new ServerFormat17(a));
            }
        }

        public static Item Create(Sprite owner, string item, bool curse = false)
        {
            if (!ServerContext.GlobalItemTemplateCache.ContainsKey(item))
                return null;

            var template = ServerContext.GlobalItemTemplateCache[item];
            return Create(owner, template, curse);
        }

        public static Item Create(Sprite Owner, ItemTemplate itemtemplate, bool curse = false)
        {
            if (Owner == null)
                return null;

            var template =
                (ItemTemplate)StorageManager.ItemBucket.LoadFromStorage(itemtemplate);

            if (itemtemplate != null && template == null)
                template = itemtemplate;

            var obj = new Item
            {
                AbandonedDate = DateTime.UtcNow,
                Template = template,
                X = Owner.X,
                Y = Owner.Y,
                Image = template.Image,
                DisplayImage = template.DisplayImage,
                CurrentMapId = Owner.CurrentMapId,
                Cursed = curse,
                Owner = (uint)Owner.Serial,
                Durability = template.MaxDurability,
                OffenseElement = template.OffenseElement,
                DefenseElement = template.DefenseElement
            };

            if (obj.Template == null)
                obj.Template = template;

            obj.Warnings = new[] { false, false, false };

            obj.AuthenticatedAislings = null;

            if (obj.Color == 0)
                obj.Color = (byte)ServerContext.Config.DefaultItemColor;

            if (obj.Template.Flags.HasFlag(ItemFlags.Repairable))
            {
                if (obj.Template.MaxDurability == uint.MinValue)
                {
                    obj.Template.MaxDurability = ServerContext.Config.DefaultItemDurability;
                    obj.Durability = ServerContext.Config.DefaultItemDurability;
                }

                if (obj.Template.Value == uint.MinValue)
                    obj.Template.Value = ServerContext.Config.DefaultItemValue;
            }

            if (obj.Template.Flags.HasFlag(ItemFlags.QuestRelated))
            {
                obj.Template.MaxDurability = 0;
                obj.Durability = 0;
            }


            lock (Generator.Random)
            {
                obj.Serial = Generator.GenerateNumber();
            }

            obj.Script = ScriptManager.Load<ItemScript>(template.ScriptName, obj);
            if (!string.IsNullOrEmpty(obj.Template.WeaponScript))
            {
                obj.WeaponScript = ScriptManager.Load<WeaponScript>(obj.Template.WeaponScript, obj);
            }
            return obj;
        }

        public static void ApplyQuality(Item obj)
        {
            try
            {
                if (obj.Template == null)
                    return;

                lock (ServerContext.SyncObj)
                {

                    var template = (ItemTemplate)StorageManager.ItemBucket.LoadFromStorage(obj.Template);
                    if (template == null)
                        return;


                    obj.Template = new ItemTemplate();
                    obj.Template.AcModifer = template.AcModifer;
                    obj.Template.CanStack = template.CanStack;
                    obj.Template.CarryWeight = template.CarryWeight;
                    obj.Template.Class = template.Class;
                    obj.Template.Color = template.Color;
                    obj.Template.ConModifer = template.ConModifer;
                    obj.Template.DefenseElement = template.DefenseElement;
                    obj.Template.DexModifer = template.DexModifer;
                    obj.Template.DisplayImage = template.DisplayImage;
                    obj.Template.DmgMax = template.DmgMax;
                    obj.Template.DmgMin = template.DmgMin;
                    obj.Template.DmgModifer = template.DmgModifer;
                    obj.Template.DropRate = template.DropRate;
                    obj.Template.EquipmentSlot = template.EquipmentSlot;
                    obj.Template.Flags = template.Flags;
                    obj.Template.Gender = template.Gender;
                    obj.Template.HasPants = template.HasPants;
                    obj.Template.HealthModifer = template.HealthModifer;
                    obj.Template.HitModifer = template.HitModifer;
                    obj.Template.ID = template.ID;
                    obj.Template.Image = template.Image;
                    obj.Template.IntModifer = template.IntModifer;
                    obj.Template.LevelRequired = template.LevelRequired;
                    obj.Template.ManaModifer = template.ManaModifer;
                    obj.Template.MaxDurability = template.MaxDurability;
                    obj.Template.MaxStack = template.MaxStack;
                    obj.Template.MrModifer = template.MrModifer;
                    obj.Template.Name = template.Name;
                    obj.Template.NpcKey = template.NpcKey;
                    obj.Template.OffenseElement = template.OffenseElement;
                    obj.Template.ScriptName = template.ScriptName;
                    obj.Template.SpellOperator = template.SpellOperator;
                    obj.Template.StageRequired = template.StageRequired;
                    obj.Template.StrModifer = template.StrModifer;
                    obj.Template.Value = template.Value;
                    obj.Template.Weight = template.Weight;
                    obj.Template.WisModifer = template.WisModifer;
                }

                if (obj.Upgrades > 0)
                {
                    if (obj.Template.AcModifer != null)
                    {
                        if (obj.Template.AcModifer.Option == StatusOperator.Operator.Remove)
                        {
                            obj.Template.AcModifer.Value -= -(obj.Upgrades);
                        }
                    }

                    if (obj.Template.MrModifer != null)
                    {
                        if (obj.Template.MrModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.MrModifer.Value += (obj.Upgrades * 10);
                    }

                    if (obj.Template.HealthModifer != null)
                    {
                        if (obj.Template.HealthModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.HealthModifer.Value += (500 * obj.Upgrades);
                    }

                    if (obj.Template.ManaModifer != null)
                    {
                        if (obj.Template.ManaModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.ManaModifer.Value += (300 * obj.Upgrades);
                    }

                    if (obj.Template.StrModifer != null)
                    {
                        if (obj.Template.StrModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.StrModifer.Value += obj.Upgrades;
                    }

                    if (obj.Template.IntModifer != null)
                    {
                        if (obj.Template.IntModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.IntModifer.Value += obj.Upgrades;
                    }

                    if (obj.Template.WisModifer != null)
                    {
                        if (obj.Template.WisModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.WisModifer.Value += obj.Upgrades;
                    }
                    if (obj.Template.ConModifer != null)
                    {
                        if (obj.Template.ConModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.ConModifer.Value += obj.Upgrades;
                    }

                    if (obj.Template.DexModifer != null)
                    {
                        if (obj.Template.DexModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.DexModifer.Value += obj.Upgrades;
                    }

                    if (obj.Template.DmgModifer != null)
                    {
                        if (obj.Template.DmgModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.DmgModifer.Value += obj.Upgrades;
                    }

                    if (obj.Template.HitModifer != null)
                    {
                        if (obj.Template.HitModifer.Option == StatusOperator.Operator.Add)
                            obj.Template.HitModifer.Value += obj.Upgrades;
                    }

                    obj.Template.LevelRequired -= (byte)obj.Upgrades;
                    obj.Template.Value *= (byte)obj.Upgrades;
                    obj.Template.MaxDurability += (byte)(1500 * obj.Upgrades);
                    obj.Template.DmgMax += (100 * obj.Upgrades);
                    obj.Template.DmgMin += (20 * obj.Upgrades);


                    if (obj.Template.LevelRequired <= 0 || obj.Template.LevelRequired > 99)
                    {
                        obj.Template.LevelRequired = 1;
                    }

                }
            }
            catch (Exception error)
            {
                ServerContext.Info?.Error("Error: ItemAddQuality.", error);
            }
        }

        public void Release(Sprite owner, Position position)
        {
            X = position.X;
            Y = position.Y;


            lock (Generator.Random)
            {
                Serial = Generator.GenerateNumber();
            }

            CurrentMapId = owner.CurrentMapId;
            AbandonedDate = DateTime.UtcNow;

            if (owner is Aisling)
            {
                AuthenticatedAislings = new Sprite[0];
                Cursed = false;
            }

            AddObject(this);

            if (owner is Aisling)
                ShowTo(owner as Aisling);
        }
    }
}
