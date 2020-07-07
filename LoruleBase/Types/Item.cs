#region

using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public class Item : Sprite
    {
        public enum Variance
        {
            None,
            Enchanted,
            Blessed,
            Magic,
            Gramail,
            Deoch,
            Ceannlaidir,
            Cail,
            Fiosachd,
            Glioca,
            Luathas,
            Sgrios
        }

        [JsonIgnore] public Sprite[] AuthenticatedAislings { get; set; }
        public byte Color { get; set; }
        public bool Cursed { get; set; }
        public ushort DisplayImage { get; set; }
        [JsonIgnore] public string DisplayName => getDisplayName();
        public uint Durability { get; set; }
        [JsonIgnore] public bool Equipped { get; set; }
        [JsonProperty] public bool Identifed { get; set; }
        public ushort Image { get; set; }
        [JsonProperty] public Variance ItemVariance { get; set; } = Variance.None;
        public uint Owner { get; set; }
        [JsonIgnore] public Dictionary<string, ItemScript> Scripts { get; set; }
        public byte Slot { get; set; }
        public ushort Stacks { get; set; }
        public ItemTemplate Template { get; set; }
        public Type Type { get; set; }
        public int Upgrades { get; set; }
        public bool[] Warnings { get; set; }
        [JsonIgnore] public Dictionary<string, WeaponScript> WeaponScripts { get; set; }

        public static void ApplyQuality(Item obj)
        {
            try
            {
                if (obj.Template == null)
                    return;

                var template = (ItemTemplate)StorageManager.ItemBucket.LoadFromStorage(obj.Template);
                if (template == null)
                    return;

                obj.Template = new ItemTemplate
                {
                    AcModifer = template.AcModifer,
                    CanStack = template.CanStack,
                    CarryWeight = template.CarryWeight,
                    Class = template.Class,
                    Color = template.Color,
                    ConModifer = template.ConModifer,
                    DefenseElement = template.DefenseElement,
                    DexModifer = template.DexModifer,
                    DisplayImage = template.DisplayImage,
                    DmgMax = template.DmgMax,
                    DmgMin = template.DmgMin,
                    DmgModifer = template.DmgModifer,
                    DropRate = template.DropRate,
                    EquipmentSlot = template.EquipmentSlot,
                    Flags = template.Flags,
                    Gender = template.Gender,
                    HasPants = template.HasPants,
                    HealthModifer = template.HealthModifer,
                    HitModifer = template.HitModifer,
                    ID = template.ID,
                    Image = template.Image,
                    IntModifer = template.IntModifer,
                    LevelRequired = template.LevelRequired,
                    ManaModifer = template.ManaModifer,
                    MaxDurability = template.MaxDurability,
                    MaxStack = template.MaxStack,
                    MrModifer = template.MrModifer,
                    Name = template.Name,
                    NpcKey = template.NpcKey,
                    OffenseElement = template.OffenseElement,
                    ScriptName = template.ScriptName,
                    SpellOperator = template.SpellOperator,
                    StageRequired = template.StageRequired,
                    StrModifer = template.StrModifer,
                    Value = template.Value,
                    Weight = template.Weight,
                    WisModifer = template.WisModifer
                };

                if (obj.Upgrades > 0)
                {
                    if (obj.Template.AcModifer != null)
                        if (obj.Template.AcModifer.Option == Operator.Remove)
                            obj.Template.AcModifer.Value -= -obj.Upgrades;

                    if (obj.Template.MrModifer != null)
                        if (obj.Template.MrModifer.Option == Operator.Add)
                            obj.Template.MrModifer.Value += obj.Upgrades * 10;

                    if (obj.Template.HealthModifer != null)
                        if (obj.Template.HealthModifer.Option == Operator.Add)
                            obj.Template.HealthModifer.Value += 500 * obj.Upgrades;

                    if (obj.Template.ManaModifer != null)
                        if (obj.Template.ManaModifer.Option == Operator.Add)
                            obj.Template.ManaModifer.Value += 300 * obj.Upgrades;

                    if (obj.Template.StrModifer != null)
                        if (obj.Template.StrModifer.Option == Operator.Add)
                            obj.Template.StrModifer.Value += obj.Upgrades;

                    if (obj.Template.IntModifer != null)
                        if (obj.Template.IntModifer.Option == Operator.Add)
                            obj.Template.IntModifer.Value += obj.Upgrades;

                    if (obj.Template.WisModifer != null)
                        if (obj.Template.WisModifer.Option == Operator.Add)
                            obj.Template.WisModifer.Value += obj.Upgrades;
                    if (obj.Template.ConModifer != null)
                        if (obj.Template.ConModifer.Option == Operator.Add)
                            obj.Template.ConModifer.Value += obj.Upgrades;

                    if (obj.Template.DexModifer != null)
                        if (obj.Template.DexModifer.Option == Operator.Add)
                            obj.Template.DexModifer.Value += obj.Upgrades;

                    if (obj.Template.DmgModifer != null)
                        if (obj.Template.DmgModifer.Option == Operator.Add)
                            obj.Template.DmgModifer.Value += obj.Upgrades;

                    if (obj.Template.HitModifer != null)
                        if (obj.Template.HitModifer.Option == Operator.Add)
                            obj.Template.HitModifer.Value += obj.Upgrades;

                    obj.Template.LevelRequired -= (byte)obj.Upgrades;
                    obj.Template.Value *= (byte)obj.Upgrades;
                    obj.Template.MaxDurability += (byte)(1500 * obj.Upgrades);
                    obj.Template.DmgMax += 100 * obj.Upgrades;
                    obj.Template.DmgMin += 20 * obj.Upgrades;

                    if (obj.Template.LevelRequired <= 0 || obj.Template.LevelRequired > 99)
                        obj.Template.LevelRequired = 1;
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
        }

        public static Item Create(Sprite owner, string item, bool curse = false)
        {
            if (!ServerContextBase.GlobalItemTemplateCache.ContainsKey(item))
                return null;

            var template = ServerContextBase.GlobalItemTemplateCache[item];
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
                XPos = Owner.XPos,
                YPos = Owner.YPos,
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

            if (obj.Color != 0)
            {
                obj.Color = (byte)template.Color;
            }
            else
            {
                obj.Color = (byte)ServerContextBase.Config.DefaultItemColor;
            }

            if (obj.Template.Flags.HasFlag(ItemFlags.Repairable))
            {
                if (obj.Template.MaxDurability == uint.MinValue)
                {
                    obj.Template.MaxDurability = ServerContextBase.Config.DefaultItemDurability;
                    obj.Durability = ServerContextBase.Config.DefaultItemDurability;
                }

                if (obj.Template.Value == uint.MinValue)
                    obj.Template.Value = ServerContextBase.Config.DefaultItemValue;
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

            obj.Scripts = ScriptManager.Load<ItemScript>(template.ScriptName, obj);
            if (!string.IsNullOrEmpty(obj.Template.WeaponScript))
                obj.WeaponScripts = ScriptManager.Load<WeaponScript>(obj.Template.WeaponScript, obj);

            return obj;
        }

        public void ApplyModifers(GameClient client)
        {
            if (client == null || client.Aisling == null)
                return;

            #region Armor class Modifers

            if (Template.AcModifer != null)
            {
                if (Template.AcModifer.Option == Operator.Add)
                    client.Aisling.BonusAc += Template.AcModifer.Value;
                if (Template.AcModifer.Option == Operator.Remove)
                    client.Aisling.BonusAc -= Template.AcModifer.Value;

                client.SendMessage(0x03, $"E: {Template.Name}, AC: {client.Aisling.Ac}");
                client.SendStats(flags: StatusFlags.StructD);
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
                if (Template.MrModifer.Option == Operator.Add)
                    client.Aisling.BonusMr += (byte)Template.MrModifer.Value;
                if (Template.MrModifer.Option == Operator.Remove)
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
                if (Template.HealthModifer.Option == Operator.Add)
                    client.Aisling.BonusHp += Template.HealthModifer.Value;
                if (Template.HealthModifer.Option == Operator.Remove)
                    client.Aisling.BonusHp -= Template.HealthModifer.Value;

                if (client.Aisling.BonusHp < 0)
                    client.Aisling.BonusHp = ServerContextBase.Config.MinimumHp;
            }

            #endregion

            #region Mana

            if (Template.ManaModifer != null)
            {
                if (Template.ManaModifer.Option == Operator.Add)
                    client.Aisling.BonusMp += Template.ManaModifer.Value;
                if (Template.ManaModifer.Option == Operator.Remove)
                    client.Aisling.BonusMp -= Template.ManaModifer.Value;
            }

            #endregion

            #region Regen

            if (Template.RegenModifer != null)
            {
                if (Template.RegenModifer.Option == Operator.Add)
                    client.Aisling.BonusRegen += Template.RegenModifer.Value;
                if (Template.RegenModifer.Option == Operator.Remove)
                    client.Aisling.BonusRegen -= Template.RegenModifer.Value;
            }

            #endregion

            #region Str

            if (Template.StrModifer != null)
            {
                if (Template.StrModifer.Option == Operator.Add)
                    client.Aisling.BonusStr += (byte)Template.StrModifer.Value;
                if (Template.StrModifer.Option == Operator.Remove)
                    client.Aisling.BonusStr -= (byte)Template.StrModifer.Value;
            }

            #endregion

            #region Int

            if (Template.IntModifer != null)
            {
                if (Template.IntModifer.Option == Operator.Add)
                    client.Aisling.BonusInt += (byte)Template.IntModifer.Value;
                if (Template.IntModifer.Option == Operator.Remove)
                    client.Aisling.BonusInt -= (byte)Template.IntModifer.Value;
            }

            #endregion

            #region Wis

            if (Template.WisModifer != null)
            {
                if (Template.WisModifer.Option == Operator.Add)
                    client.Aisling.BonusWis += (byte)Template.WisModifer.Value;
                if (Template.WisModifer.Option == Operator.Remove)
                    client.Aisling.BonusWis -= (byte)Template.WisModifer.Value;
            }

            #endregion

            #region Con

            if (Template.ConModifer != null)
            {
                if (Template.ConModifer.Option == Operator.Add)
                    client.Aisling.BonusCon += (byte)Template.ConModifer.Value;
                if (Template.ConModifer.Option == Operator.Remove)
                    client.Aisling.BonusCon -= (byte)Template.ConModifer.Value;
            }

            #endregion

            #region Dex

            if (Template.DexModifer != null)
            {
                if (Template.DexModifer.Option == Operator.Add)
                    client.Aisling.BonusDex += (byte)Template.DexModifer.Value;
                if (Template.DexModifer.Option == Operator.Remove)
                    client.Aisling.BonusDex -= (byte)Template.DexModifer.Value;
            }

            #endregion

            #region Hit

            if (Template.HitModifer != null)
            {
                if (Template.HitModifer.Option == Operator.Add)
                    client.Aisling.BonusHit += (byte)Template.HitModifer.Value;
                if (Template.HitModifer.Option == Operator.Remove)
                    client.Aisling.BonusHit -= (byte)Template.HitModifer.Value;
            }

            #endregion

            #region Dmg

            if (Template.DmgModifer != null)
            {
                if (Template.DmgModifer.Option == Operator.Add)
                    client.Aisling.BonusDmg += (byte)Template.DmgModifer.Value;
                if (Template.DmgModifer.Option == Operator.Remove)
                    client.Aisling.BonusDmg -= (byte)Template.DmgModifer.Value;
            }

            #endregion
        }

        public bool CanCarry(Sprite sprite)
        {
            if (((Aisling)sprite).CurrentWeight + Template.CarryWeight > (sprite as Aisling).MaximumWeight)
            {
                ((Aisling)sprite)?.Client.SendMessage(Scope.Self, 0x02, ServerContextBase.Config.ToWeakToLift);
                return false;
            }

            return true;
        }

        public bool GiveTo(Sprite sprite, bool checkWeight = true)
        {
            if (sprite is Aisling)
            {
                Owner = (uint)sprite.Serial;

                #region stackable items

                if (Template.Flags.HasFlag(ItemFlags.Stackable))
                {
                    var numStacks = (byte)Stacks;

                    if (numStacks <= 0)
                        numStacks = 1;

                    var item = ((Aisling)sprite).Inventory.Get(i => i != null && i.Template.Name == Template.Name
                                                                              && i.Stacks + numStacks <
                                                                              i.Template.MaxStack).FirstOrDefault();

                    if (item != null)
                    {
                        Slot = item.Slot;

                        item.Stacks += numStacks;

                        ((Aisling)sprite).Client.Aisling.Inventory.Set(item, false);

                        ((Aisling)sprite).Client.Send(new ServerFormat10(item.Slot));

                        ((Aisling)sprite).Client.Send(new ServerFormat0F(item));

                        ((Aisling)sprite).Client.SendMessage(Scope.Self, 0x02,
                            $"Received {DisplayName}, You now have ({(item.Stacks == 0 ? item.Stacks + 1 : item.Stacks)})");

                        return true;
                    }

                    if (Stacks <= 0)
                        Stacks = 1;

                    if (checkWeight)
                        if (!CanCarry(sprite))
                            return false;

                    Slot = ((Aisling)sprite).Inventory.FindEmpty();

                    if (Slot == byte.MaxValue)
                    {
                        ((Aisling)sprite).Client.SendMessage(Scope.Self, 0x02,
                            ServerContextBase.Config.CantCarryMoreMsg);
                        return false;
                    }

                    ((Aisling)sprite).Inventory.Set(this, false);
                    var format = new ServerFormat0F(this);
                    ((Aisling)sprite).Show(Scope.Self, format);
                    ((Aisling)sprite).Client.SendMessage(Scope.Self, 0x02,
                        $"{DisplayName} Received.");

                    if (checkWeight)
                    {
                        ((Aisling)sprite).CurrentWeight += Template.CarryWeight;
                        ((Aisling)sprite).Client.SendStats(StatusFlags.StructA);
                    }

                    return true;
                }

                #endregion

                #region not stackable items

                {
                    Slot = ((Aisling)sprite).Inventory.FindEmpty();

                    if (Slot == byte.MaxValue)
                    {
                        ((Aisling)sprite).Client.SendMessage(Scope.Self, 0x02,
                            ServerContextBase.Config.CantCarryMoreMsg);
                        return false;
                    }

                    if (checkWeight)
                        if (!CanCarry(sprite))
                            return false;

                    ((Aisling)sprite).Inventory.Assign(this);
                    var format = new ServerFormat0F(this);
                    ((Aisling)sprite).Show(Scope.Self, format);

                    if (checkWeight)
                    {
                        ((Aisling)sprite).CurrentWeight += Template.CarryWeight;
                        ((Aisling)sprite).Client?.SendStats(StatusFlags.StructA);
                    }

                    return true;
                }

                #endregion
            }

            return false;
        }

        public void Release(Sprite owner, Position position)
        {
            XPos = position.X;
            YPos = position.Y;

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

            if (owner is Aisling aisling)
                ShowTo(aisling);
        }

        public void RemoveModifiers(GameClient client)
        {
            if (client == null || client.Aisling == null)
                return;

            #region Armor class Modifers

            if (Template.AcModifer != null)
            {
                switch (Template.AcModifer.Option)
                {
                    case Operator.Add:
                        client.Aisling.BonusAc -= Template.AcModifer.Value;
                        break;

                    case Operator.Remove:
                        client.Aisling.BonusAc += Template.AcModifer.Value;
                        break;
                }

                client.SendMessage(0x03, $"E: {Template.Name}, AC: {client.Aisling.Ac}");
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

                    if (spell?.Template == null)
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
                if (Template.MrModifer.Option == Operator.Add)
                    client.Aisling.BonusMr -= (byte)Template.MrModifer.Value;
                if (Template.MrModifer.Option == Operator.Remove)
                    client.Aisling.BonusMr += (byte)Template.MrModifer.Value;

                if (client.Aisling.BonusMr < 0)
                    client.Aisling.BonusMr = 0;
            }

            #endregion

            #region Health

            if (Template.HealthModifer != null)
            {
                if (Template.HealthModifer.Option == Operator.Add)
                    client.Aisling.BonusHp -= Template.HealthModifer.Value;
                if (Template.HealthModifer.Option == Operator.Remove)
                    client.Aisling.BonusHp += Template.HealthModifer.Value;

                if (client.Aisling.BonusHp < 0)
                    client.Aisling.BonusHp = ServerContextBase.Config.MinimumHp;
            }

            #endregion

            #region Mana

            if (Template.ManaModifer != null)
            {
                if (Template.ManaModifer.Option == Operator.Add)
                    client.Aisling.BonusMp -= Template.ManaModifer.Value;
                if (Template.ManaModifer.Option == Operator.Remove)
                    client.Aisling.BonusMp += Template.ManaModifer.Value;
            }

            #endregion

            #region Regen

            if (Template.RegenModifer != null)
            {
                if (Template.RegenModifer.Option == Operator.Add)
                    client.Aisling.BonusRegen -= Template.RegenModifer.Value;
                if (Template.RegenModifer.Option == Operator.Remove)
                    client.Aisling.BonusRegen += Template.RegenModifer.Value;
            }

            #endregion

            #region Str

            if (Template.StrModifer != null)
            {
                if (Template.StrModifer.Option == Operator.Add)
                    client.Aisling.BonusStr -= (byte)Template.StrModifer.Value;
                if (Template.StrModifer.Option == Operator.Remove)
                    client.Aisling.BonusStr += (byte)Template.StrModifer.Value;
            }

            #endregion

            #region Int

            if (Template.IntModifer != null)
            {
                if (Template.IntModifer.Option == Operator.Add)
                    client.Aisling.BonusInt -= (byte)Template.IntModifer.Value;
                if (Template.IntModifer.Option == Operator.Remove)
                    client.Aisling.BonusInt += (byte)Template.IntModifer.Value;
            }

            #endregion

            #region Wis

            if (Template.WisModifer != null)
            {
                if (Template.WisModifer.Option == Operator.Add)
                    client.Aisling.BonusWis -= (byte)Template.WisModifer.Value;
                if (Template.WisModifer.Option == Operator.Remove)
                    client.Aisling.BonusWis += (byte)Template.WisModifer.Value;
            }

            #endregion

            #region Con

            if (Template.ConModifer != null)
            {
                if (Template.ConModifer.Option == Operator.Add)
                    client.Aisling.BonusCon -= (byte)Template.ConModifer.Value;
                if (Template.ConModifer.Option == Operator.Remove)
                    client.Aisling.BonusCon += (byte)Template.ConModifer.Value;

                if (client.Aisling.BonusCon < 0)
                    client.Aisling.BonusCon = ServerContextBase.Config.BaseStatAttribute;
                if (client.Aisling.BonusCon > 255)
                    client.Aisling.BonusCon = 255;
            }

            #endregion

            #region Dex

            if (Template.DexModifer != null)
            {
                if (Template.DexModifer.Option == Operator.Add)
                    client.Aisling.BonusDex -= (byte)Template.DexModifer.Value;
                if (Template.DexModifer.Option == Operator.Remove)
                    client.Aisling.BonusDex += (byte)Template.DexModifer.Value;
            }

            #endregion

            #region Hit

            if (Template.HitModifer != null)
            {
                if (Template.HitModifer.Option == Operator.Add)
                    client.Aisling.BonusHit -= (byte)Template.HitModifer.Value;
                if (Template.HitModifer.Option == Operator.Remove)
                    client.Aisling.BonusHit += (byte)Template.HitModifer.Value;
            }

            #endregion

            #region Dmg

            if (Template.DmgModifer != null)
            {
                if (Template.DmgModifer.Option == Operator.Add)
                    client.Aisling.BonusDmg -= (byte)Template.DmgModifer.Value;
                if (Template.DmgModifer.Option == Operator.Remove)
                    client.Aisling.BonusDmg += (byte)Template.DmgModifer.Value;
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

        private string getDisplayName()
        {
            var upgradeName = "";

            if (Upgrades == 4) upgradeName = "{=fRare";

            if (Upgrades == 5) upgradeName = "{=pEpic";

            if (Upgrades == 6) upgradeName = "{=sLegendary";

            if (Upgrades == 7) upgradeName = "{=bGodly";

            if (Upgrades == 8) upgradeName = "{=uForsaken";

            if (ItemVariance != Variance.None && Identifed)
                return $"{upgradeName} {ItemVariance.ToString()} {Template.Name}";

            if (upgradeName != "") return $"{upgradeName} {Template.Name}";

            return Template.Name;
        }
    }
}