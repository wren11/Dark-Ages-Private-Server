#region

using Darkages.Common;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using MenuInterpreter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameClient : NetworkClient
    {
        public bool MapUpdating;
        private readonly object _syncObj = new object();

        public GameClient()
        {
            HpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContextBase.Config.RegenRate));

            MpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContextBase.Config.RegenRate));
        }

        public Aisling Aisling { get; set; }
        public DateTime BoardOpened { get; set; }

        public bool CanSendLocation =>
            DateTime.UtcNow - LastLocationSent < new TimeSpan(0, 0, 0, 2);

        public DialogSession DlgSession { get; set; }
        public GameServerTimer HpRegenTimer { get; set; }

        public bool IsRefreshing =>
            DateTime.UtcNow - LastClientRefresh < new TimeSpan(0, 0, 0, 0, ServerContextBase.Config.RefreshRate);

        public bool IsWarping =>
            DateTime.UtcNow - LastWarp < new TimeSpan(0, 0, 0, 0, ServerContextBase.Config.WarpCheckRate);

        public byte LastActivatedLost { get; set; }
        public DateTime LastAssail { get; set; }
        public ushort LastBoardActivated { get; set; }
        public DateTime LastClientRefresh { get; set; }
        public Item LastItemDropped { get; set; }
        public DateTime LastLocationSent { get; set; }
        public DateTime LastMapUpdated { get; set; }
        public TimeSpan LastMenuStarted { get; set; }
        public DateTime LastMessageSent { get; set; }
        public DateTime LastMovement { get; set; }
        public DateTime LastPing { get; set; }
        public DateTime LastPingResponse { get; set; }
        public DateTime LastSave { get; set; }
        public DateTime LastScriptExecuted { get; set; }
        public DateTime LastWarp { get; set; }
        public DateTime LastWhisperMessageSent { get; set; }
        public Interpreter MenuInterpter { get; set; }
        public GameServerTimer MpRegenTimer { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public PendingSell PendingItemSessions { get; set; }
        public GameServer Server { get; set; }
        public bool ShouldUpdateMap { get; set; }

        public bool WasUpdatingMapRecently =>
                    DateTime.UtcNow - LastMapUpdated < new TimeSpan(0, 0, 0, 0, 100);

        public GameClient AislingToGhostForm()
        {
            Aisling.Flags = AislingFlags.Ghost;

            HpRegenTimer.Disabled = true;
            MpRegenTimer.Disabled = true;

            Refresh(true);

            return this;
        }

        public void BuildSettings()
        {
            if (ServerContextBase.Config.Settings == null || ServerContextBase.Config.Settings.Count == 0)
                return;

            if (Aisling.GameSettings == null || Aisling.GameSettings.Count == 0)
            {
                Aisling.GameSettings = new List<ClientGameSettings>();

                foreach (var settings in ServerContextBase.Config.Settings)
                    Aisling.GameSettings.Add(new ClientGameSettings(settings.SettingOff, settings.SettingOn,
                        settings.Enabled));
            }
        }

        public bool CheckReqs(GameClient client, Item item)
        {
            var message = string.Empty;

            if (client.Aisling.GameMaster)
            {
                if (item.Durability > 0)
                {
                    client.Aisling.EquipmentManager.Add(item.Template.EquipmentSlot, item);
                    return true;
                }
            }

            if (client.Aisling.ExpLevel < item.Template.LevelRequired)
            {
                message = ServerContextBase.Config.CantWearYetMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (item.Durability <= 0)
            {
                message = ServerContextBase.Config.RepairItemMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (client.Aisling.Path != item.Template.Class && !(item.Template.Class == Class.Peasant))
            {
                if (client.Aisling.ExpLevel >= item.Template.LevelRequired)
                    message = ServerContextBase.Config.WrongClassMessage;
                else
                    message = ServerContextBase.Config.CantWearYetMessage;
            }

            if (!item.Template.Class.HasFlag(client.Aisling.Path) && !(item.Template.Class == Class.Peasant))
                message = "You are forbidden to wear that.";

            if (!(message != null && string.IsNullOrEmpty(message)))
            {
                client.SendMessage(0x02, message);
                return false;
            }

            if (client.Aisling.ExpLevel >= item.Template.LevelRequired
                && (client.Aisling.Path == item.Template.Class || item.Template.Class == Class.Peasant))
            {
                if (item.Template.Gender == Gender.Both)
                {
                    client.Aisling.EquipmentManager.Add(item.Template.EquipmentSlot, item);
                }
                else
                {
                    if (item.Template.Gender == client.Aisling.Gender)
                    {
                        client.Aisling.EquipmentManager.Add(item.Template.EquipmentSlot, item);
                    }
                    else
                    {
                        client.SendMessage(0x02, ServerContextBase.Config.DoesNotFitMessage);
                        return false;
                    }
                }

                return true;
            }

            client.SendMessage(0x02, ServerContextBase.Config.CantEquipThatMessage);
            return false;
        }

        public GameClient CloseDialog()
        {
            Send(new byte[] { 0x30, 0x00, 0x0A, 0x00 });
            MenuInterpter = null;

            return this;
        }

        public GameClient DoUpdate(TimeSpan elapsedTime)
        {
            if (!Socket.Connected)
            {
                Aisling.Remove(true);
                Server.ClientDisconnected(this);

                return this;
            }

            return HandleTimeOuts()
                .StatusCheck()
                .Regen(elapsedTime)
                .UpdateStatusBar(elapsedTime)
                .UpdateReactors(elapsedTime);
        }

        public GameClient EnterArea()
        {
            return Enter();
        }

        public GameClient GhostFormToAisling()
        {
            Aisling.Flags = AislingFlags.Normal;
            HpRegenTimer.Disabled = false;
            MpRegenTimer.Disabled = false;

            Refresh(true);
            return this;
        }

        public GameClient HandleTimeOuts()
        {
            if (Aisling.Exchange != null)
                if (Aisling.Exchange.Trader != null)
                    if (!Aisling.Exchange.Trader.LoggedIn
                        || !Aisling.WithinRangeOf(Aisling.Exchange.Trader))
                        Aisling.CancelExchange();

            return this;
        }

        public GameClient InitSpellBar()
        {
            lock (_syncObj)
            {
                foreach (var buff in Aisling.Buffs.Select(i => i.Value))
                {
                    buff.OnApplied(Aisling, buff);
                    {
                        buff.Display(Aisling);
                    }
                }

                foreach (var debuff in Aisling.Debuffs.Select(i => i.Value))
                {
                    debuff.OnApplied(Aisling, debuff);
                    {
                        debuff.Display(Aisling);
                    }
                }
            }

            return this;
        }

        public GameClient Insert()
        {
            AddObject(Aisling);
            return this;
        }

        public void Interupt()
        {
            GameServer.CancelIfCasting(this);
            SendLocation();
        }

        public GameClient LearnSkill(Mundane source, SkillTemplate subject, string message)
        {
            var canLearn = false;

            if (subject.Prerequisites != null)
            {
                canLearn = PayPrerequisites(subject.Prerequisites);
            }

            if (subject.LearningRequirements != null && subject.LearningRequirements.Any())
            {
                canLearn = subject.LearningRequirements.TrueForAll(PayPrerequisites);
            }

            if (!canLearn)
                return this;

            Skill.GiveTo(this, subject.Name);
            SendOptionsDialog(source, message);

            Aisling.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)Aisling.Serial, (uint)source.Serial,
                    subject?.TargetAnimation ?? 124,
                    subject?.TargetAnimation ?? 124, 100));

            return this;
        }

        public GameClient LearnSpell(Mundane source, SpellTemplate subject, string message)
        {
            var canLearn = false;

            if (subject.Prerequisites != null)
            {
                canLearn = PayPrerequisites(subject.Prerequisites);
            }

            if (subject.LearningRequirements != null && subject.LearningRequirements.Any())
            {
                canLearn = subject.LearningRequirements.TrueForAll(PayPrerequisites);
            }

            if (!canLearn)
                return this;

            Spell.GiveTo(this, subject.Name);
            SendOptionsDialog(source, message);

            Aisling.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)Aisling.Serial, (uint)source.Serial,
                    subject?.TargetAnimation ?? 124,
                    subject?.TargetAnimation ?? 124, 100));

            return this;
        }

        public GameClient LeaveArea(bool update = false, bool delete = false)
        {
            if (Aisling.LastMapId == short.MaxValue) Aisling.LastMapId = Aisling.CurrentMapId;

            Aisling.Remove(update, delete);

            return this;
        }

        public GameClient Load()
        {
            if (Aisling == null || Aisling.AreaId == 0)
                return null;

            if (!ServerContextBase.GlobalMapCache.ContainsKey(Aisling.AreaId))
                return null;

            SetAislingStartupVariables();

            lock (_syncObj)
            {
                try
                {
                    Thread.Sleep(100);

                    return InitSpellBar()
                        .LoadInventory()
                        .LoadSkillBook()
                        .LoadSpellBook()
                        .LoadEquipment()
                        .SendProfileUpdate()
                        .SendStats(StatusFlags.All);
                }
                catch (NullReferenceException e)
                {
                    ServerContext.Error(e);
                }
            }

            return null;
        }

        public GameClient LoadEquipment()
        {
            var formats = new List<NetworkFormat>();

            lock (_syncObj)
            {
                foreach (var item in Aisling.EquipmentManager.Equipment)
                {
                    var equipment = item.Value;

                    if (equipment == null || equipment.Item == null || equipment.Item.Template == null)
                        continue;

                    if (equipment.Item.Template != null)
                        if (ServerContextBase.GlobalItemTemplateCache.ContainsKey(equipment.Item.Template.Name))
                        {
                            var template = ServerContextBase.GlobalItemTemplateCache[equipment.Item.Template.Name];
                            {
                                item.Value.Item.Template = template;

                                if (item.Value.Item.Upgrades > 0)
                                    Item.ApplyQuality(item.Value.Item);
                            }
                        }

                    equipment.Item.Scripts =
                        ScriptManager.Load<ItemScript>(equipment.Item.Template.ScriptName, equipment.Item);
                    if (!string.IsNullOrEmpty(equipment.Item.Template.WeaponScript))
                        equipment.Item.WeaponScripts =
                            ScriptManager.Load<WeaponScript>(equipment.Item.Template.WeaponScript, equipment.Item);

                    foreach (var script in equipment.Item.Scripts?.Values)
                        script.Equipped(Aisling, (byte)equipment.Slot);

                    if (equipment.Item.CanCarry(Aisling))
                    {
                        Aisling.CurrentWeight += equipment.Item.Template.CarryWeight;

                        formats.Add(new ServerFormat37(equipment.Item, (byte)equipment.Slot));
                    }
                    else
                    {
                        var nitem = Clone<Item>(item.Value.Item);
                        nitem.Release(Aisling, Aisling.Position);

                        SendMessage(0x02,
                            string.Format(CultureInfo.CurrentCulture, "{0} is too heavy to hold.",
                                nitem.Template.Name));

                        continue;
                    }

                    if ((equipment.Item.Template.Flags & ItemFlags.Equipable) == ItemFlags.Equipable)
                        for (var i = 0; i < Aisling.SpellBook.Spells.Count; i++)
                        {
                            var spell = Aisling.SpellBook.FindInSlot(i);
                            if (spell != null && spell.Template != null)
                                equipment.Item.UpdateSpellSlot(this, spell.Slot);
                        }
                }
            }

            foreach (var format in formats)
                Aisling.Client.Send(format);

            Thread.Sleep(50);
            return this;
        }

        public GameClient LoadInventory()
        {
            lock (_syncObj)
            {
                var itemsAvailable = Aisling.Inventory.Items.Values
                    .Where(i => i != null && i.Template != null).ToArray();

                foreach (var item in itemsAvailable)
                {
                    if (string.IsNullOrEmpty(item.Template.Name))
                        continue;

                    item.Scripts = ScriptManager.Load<ItemScript>(item.Template.ScriptName, item);

                    if (!string.IsNullOrEmpty(item.Template.WeaponScript))
                        item.WeaponScripts = ScriptManager.Load<WeaponScript>(item.Template.WeaponScript, item);

                    if (ServerContextBase.GlobalItemTemplateCache.ContainsKey(item.Template.Name))
                    {
                        var template = ServerContextBase.GlobalItemTemplateCache[item.Template.Name];
                        {
                            item.Template = template;
                        }

                        if (item.Upgrades > 0)
                            Item.ApplyQuality(item);
                    }

                    if (item.Template != null)
                    {
                        if (Aisling.CurrentWeight + item.Template.CarryWeight < Aisling.MaximumWeight)
                        {
                            var format = new ServerFormat0F(item);
                            Send(format);

                            Aisling.Inventory.Set(item, false);
                            Aisling.CurrentWeight += item.Template.CarryWeight;
                        }
                        else
                        {
                            var copy = Clone<Item>(item);
                            {
                                copy.Release(Aisling, Aisling.Position);

                                SendMessage(0x02,
                                    string.Format(CultureInfo.CurrentCulture, "You stumble and drop {0}",
                                        item.Template.Name));
                            }
                        }
                    }
                }
            }

            Thread.Sleep(50);

            return this;
        }

        public GameClient LoadSkillBook()
        {
            lock (_syncObj)
            {
                var skillsAvailable = Aisling.SkillBook.Skills.Values
                    .Where(i => i != null && i.Template != null).ToArray();

                var formats = new List<NetworkFormat>();

                foreach (var skill in skillsAvailable)
                {
                    if (skill.Template != null)
                        if (ServerContextBase.GlobalSkillTemplateCache.ContainsKey(skill.Template.Name))
                        {
                            var template = ServerContextBase.GlobalSkillTemplateCache[skill.Template.Name];
                            {
                                skill.Template = template;
                            }
                        }

                    skill.InUse = false;
                    skill.NextAvailableUse = DateTime.UtcNow;

                    Send(new ServerFormat2C(skill.Slot,
                        skill.Icon,
                        skill.Name));

                    skill.Scripts = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
                    Aisling.SkillBook.Set(skill, false);
                }
            }

            Thread.Sleep(50);
            return this;
        }

        public GameClient LoadSpellBook()
        {
            if (Aisling.GameMaster)
            {
                foreach (var spell in ServerContextBase.GlobalSpellTemplateCache.Select(i => i.Value))
                {
                    if (!spell.Name.ToLower().StartsWith("[gm]"))
                        continue;

                    if (Spell.GiveTo(Aisling, spell.Name, 1))
                    {
                        ServerContext.Logger(
                            $"[GM] -> Spell {spell.Name} Given to GameMaster {Aisling.Username}.");
                    }
                }
            }

            lock (_syncObj)
            {
                Lorule.Update(() =>
                {
                    var spellsAvailable = Aisling.SpellBook.Spells.Values
                        .Where(i => i != null && i.Template != null).ToArray();

                    foreach (var spell in spellsAvailable)
                    {
                        if (spell.Template != null)
                            if (ServerContextBase.GlobalSpellTemplateCache.ContainsKey(spell.Template.Name))
                            {
                                var template = ServerContextBase.GlobalSpellTemplateCache[spell.Template.Name];
                                {
                                    spell.Template = template;
                                }
                            }

                        spell.Lines = spell.Template.BaseLines;

                        Spell.AttachScript(Aisling, spell);
                        {
                            Aisling.SpellBook.Set(spell, false);
                        }

                        Send(new ServerFormat17(spell));

                        if (spell.NextAvailableUse.Year > 1)
                        {
                            var spell1 = spell;
                            Task.Delay(1000).ContinueWith(ct =>
                            {
                                var delta = (int)Math.Abs((DateTime.UtcNow - spell1.NextAvailableUse).TotalSeconds);
                                var offset = Math.Abs(spell1.Template.Cooldown - delta);

                                if (delta <= spell1.Template.Cooldown)
                                    Send(new ServerFormat3F(0,
                                        spell1.Slot,
                                        delta));
                            });
                        }
                        else
                            spell.NextAvailableUse = DateTime.UtcNow;
                    }
                });
            }

            Thread.Sleep(50);
            return this;
        }

        public GameClient PayItemPrerequisites(LearningPredicate prerequisites)
        {
            if (prerequisites.Items_Required != null && prerequisites.Items_Required.Count > 0)
                foreach (var retainer in prerequisites.Items_Required)
                {
                    var item = Aisling.Inventory.Get(i => i.Template.Name == retainer.Item);

                    foreach (var i in item)
                        if (!i.Template.Flags.HasFlag(ItemFlags.Stackable))
                        {
                            Aisling.EquipmentManager.RemoveFromInventory(i, i.Template.CarryWeight > 0);
                            break;
                        }
                        else
                        {
                            Aisling.Inventory.RemoveRange(Aisling.Client, i, retainer.AmountRequired);
                            break;
                        }
                }

            return this;
        }

        public bool PayPrerequisites(LearningPredicate prerequisites)
        {
            if (prerequisites == null) return false;

            PayItemPrerequisites(prerequisites);
            {
                if (prerequisites.Gold_Required > 0)
                {
                    Aisling.GoldPoints -= prerequisites.Gold_Required;
                    if (Aisling.GoldPoints <= 0)
                        Aisling.GoldPoints = 0;
                }

                SendStats(StatusFlags.All);
                return true;
            }
        }

        public GameClient Refresh(bool delete = false)
        {
            LeaveArea(delete);
            EnterArea();

            return this;
        }

        public GameClient RefreshMap()
        {
            ShouldUpdateMap = false;

            if (Aisling.CurrentMapId != Aisling.LastMapId)
            {
                ShouldUpdateMap = true;
                Aisling.LastMapId = Aisling.CurrentMapId;

                if (Aisling.DiscoveredMaps.All(i => i != Aisling.CurrentMapId))
                    Aisling.DiscoveredMaps.Add(Aisling.CurrentMapId);

                SendMusic();
            }

            if (ShouldUpdateMap)
            {
                MapUpdating = true;
                Aisling.Client.LastMapUpdated = DateTime.UtcNow;
                Aisling.View.Clear();

                if (Aisling.Blind == 1) Aisling.Map.Flags |= MapFlags.Darkness;

                Send(new ServerFormat15(Aisling.Map));
            }
            else
            {
                Aisling.View.Clear();
            }

            return this;
        }

        public GameClient Regen(TimeSpan elapsedTime)
        {
            if (Aisling.Con > Aisling.ExpLevel + 1)
                HpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContextBase.Config.RegenRate / 2);

            if (Aisling.Wis > Aisling.ExpLevel + 1)
                MpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContextBase.Config.RegenRate / 2);

            if (!HpRegenTimer.Disabled)
                HpRegenTimer.Update(elapsedTime);

            if (!MpRegenTimer.Disabled)
                MpRegenTimer.Update(elapsedTime);

            if (HpRegenTimer.Elapsed && !HpRegenTimer.Disabled)
            {
                HpRegenTimer.Reset();

                var hpRegenSeed = (Aisling.Con - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var hpRegenAmount = Aisling.MaximumHp * (hpRegenSeed + 0.10);

                hpRegenAmount += hpRegenAmount / 100 * (1 + Aisling.Regen);

                Aisling.CurrentHp = (Aisling.CurrentHp + (int)hpRegenAmount).Clamp(0, Aisling.MaximumHp);
                SendStats(StatusFlags.StructB);
            }

            if (MpRegenTimer.Elapsed && !MpRegenTimer.Disabled)
            {
                MpRegenTimer.Reset();

                var mpRegenSeed = (Aisling.Wis - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var mpRegenAmount = Aisling.MaximumMp * (mpRegenSeed + 0.10);

                mpRegenAmount += mpRegenAmount / 100 * (3 + Aisling.Regen);

                Aisling.CurrentMp = (Aisling.CurrentMp + (int)mpRegenAmount).Clamp(0, Aisling.MaximumMp);
                SendStats(StatusFlags.StructB);
            }

            return this;
        }

        public void RepairEquipment(IEnumerable<Item> gear)
        {
            foreach (var item in Aisling.Inventory.Items
                .Where(i => i.Value != null).Select(i => i.Value)
                .Concat(gear).Where(i => i != null && i.Template.Flags.HasFlag(ItemFlags.Repairable)))
            {
                item.Durability = item.Template.MaxDurability;

                if (item.Equipped)
                    continue;

                Aisling.Inventory.UpdateSlot(this, item);
            }
        }

        public bool Revive()
        {
            Aisling.Flags = AislingFlags.Normal;
            HpRegenTimer.Disabled = false;
            MpRegenTimer.Disabled = false;

            Aisling.Recover();
            return Aisling.CurrentHp > 0;
        }

        public GameClient Save()
        {
            StorageManager.AislingBucket.Save(Aisling);
            LastSave = DateTime.UtcNow;
            ServerContext.Logger($"Aisling {Aisling.Username} data has been saved.");

            return this;
        }

        public void Say(string message, byte type = 0x00)
        {
            var response = new ServerFormat0D
            {
                Serial = Aisling.Serial,
                Type = type,
                Text = message
            };

            Aisling.Show(Scope.NearbyAislings, response);
        }

        public void SendAnimation(ushort animation, Sprite to, Sprite from, byte speed = 100)
        {
            var format = new ServerFormat29((uint)from.Serial, (uint)to.Serial, animation, 0, speed);
            Aisling.Show(Scope.NearbyAislings, format);
        }

        public void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemSellData(step, items)));
        }

        public void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemShopData(step, items)));
        }

        public GameClient SendLocation()
        {
            Send(new ServerFormat04(Aisling));
            LastLocationSent = DateTime.UtcNow;
            return CloseDialog();
        }

        public GameClient SendMessage(byte type, string text)
        {
            Send(new ServerFormat0A(type, text));
            LastMessageSent = DateTime.UtcNow;

            return this;
        }

        public GameClient SendMessage(string text)
        {
            Send(new ServerFormat0A(0x02, text));
            LastMessageSent = DateTime.UtcNow;

            return this;
        }

        public void SendMessage(Scope scope, byte type, string text)
        {
            switch (scope)
            {
                case Scope.Self:
                    SendMessage(type, text);
                    break;

                case Scope.NearbyAislings:
                    {
                        var nearby = GetObjects<Aisling>(Aisling.Map, i => i.WithinRangeOf(Aisling));

                        foreach (var obj in nearby)
                            obj.Client.SendMessage(type, text);
                    }
                    break;

                case Scope.NearbyAislingsExludingSelf:
                    {
                        var nearby = GetObjects<Aisling>(Aisling.Map, i => i.WithinRangeOf(Aisling));

                        foreach (var obj in nearby)
                        {
                            if (obj.Serial == Aisling.Serial)
                                continue;

                            obj.Client.SendMessage(type, text);
                        }
                    }
                    break;

                case Scope.AislingsOnSameMap:
                    {
                        var nearby = GetObjects<Aisling>(Aisling.Map, i => i.WithinRangeOf(Aisling)
                                                                           && i.CurrentMapId == Aisling.CurrentMapId);

                        foreach (var obj in nearby)
                            obj.Client.SendMessage(type, text);
                    }
                    break;

                case Scope.All:
                    {
                        var nearby = GetObjects<Aisling>(null, i => i.LoggedIn);
                        foreach (var obj in nearby)
                            obj.Client.SendMessage(type, text);
                    }
                    break;
            }
        }

        public GameClient SendMusic()
        {
            Aisling.Client.Send(new byte[]
            {
                0x19, 0x00, 0xFF,
                (byte) Aisling.Map.Music
            });

            return this;
        }

        public void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsData(options)));
        }

        public void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsPlusArgsData(options, args)));
        }

        public void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options)
        {
            Send(new PopupFormat(popup, text, new OptionsData(options)));
        }

        public GameClient SendProfileUpdate()
        {
            Send(new byte[] { 73, 0 });

            return this;
        }

        public GameClient SendSerial()
        {
            Send(new ServerFormat05(Aisling));

            return this;
        }

        public void SendSkillForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SkillForfeitData(step)));
        }

        public void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills)
        {
            Send(new ServerFormat2F(mundane, text, new SkillAcquireData(step, skills)));
        }

        public GameClient SendSound(byte sound, Scope scope = Scope.Self)
        {
            var empty = new ServerFormat13
            {
                Serial = Aisling.Serial,
                Health = byte.MaxValue,
                Sound = sound
            };

            Aisling.Show(scope, empty);
            return this;
        }

        public void SendSpellForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SpellForfeitData(step)));
        }

        public void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells)
        {
            Send(new ServerFormat2F(mundane, text, new SpellAcquireData(step, spells)));
        }

        public GameClient SendStats(StatusFlags flags)
        {
            Send(new ServerFormat08(Aisling, flags));

            return this;
        }

        public GameClient SetAislingStartupVariables()
        {
            InMapTransition = false;
            LastSave = DateTime.UtcNow;
            LastPingResponse = DateTime.UtcNow;
            PendingItemSessions = null;
            LastLocationSent = DateTime.UtcNow;
            LastMovement = DateTime.UtcNow;
            LastClientRefresh = DateTime.UtcNow;
            LastMessageSent = DateTime.UtcNow;

            BoardOpened = DateTime.UtcNow;
            {
                Aisling.BonusAc = (int)(70 - Aisling.Level * 0.5 / 1.0);
                Aisling.Exchange = null;
                Aisling.LastMapId = short.MaxValue;
            }
            BuildSettings();
            return this;
        }

        public void ShowCurrentMenu(Sprite obj, MenuItem currentitem, MenuItem nextitem)
        {
            if (nextitem == null)
                return;

            nextitem.Text = nextitem.Text.Replace("%aisling%", Aisling.Username);

            if (nextitem == null) return;
            if (nextitem.Type == MenuItemType.Step)
            {
                Send(new ReactorSequence(this, new DialogSequence
                {
                    DisplayText = nextitem.Text,
                    HasOptions = false,
                    DisplayImage = (ushort)(obj as Mundane).Template.Image,
                    Title = (obj as Mundane).Template.Name,
                    CanMoveNext = nextitem.Answers.Length > 0,
                    CanMoveBack = nextitem.Answers.Any(i => i.Text == "back"),
                    Id = obj.Serial
                }));
            }
            else if (nextitem.Type == MenuItemType.Menu)
            {
                var options = new List<OptionsDataItem>();

                foreach (var ans in nextitem.Answers)
                {
                    if (ans.Text == "close")
                        continue;

                    options.Add(new OptionsDataItem((short)ans.Id, ans.Text));
                }

                SendOptionsDialog(obj as Mundane, nextitem.Text, options.ToArray());
            }
        }

        public void ShowCurrentMenu(Popup popup, MenuItem currentitem, MenuItem nextitem)
        {
            if (nextitem == null)
                return;

            nextitem.Text = nextitem.Text.Replace("%aisling%", Aisling.Username);

            if (nextitem == null) return;
            if (nextitem.Type == MenuItemType.Step)
            {
                Send(new ReactorSequence(this, new DialogSequence
                {
                    DisplayText = nextitem.Text,
                    HasOptions = false,
                    DisplayImage = popup.Template.SpriteId,
                    Title = popup.Template.Name,
                    CanMoveNext = nextitem.Answers.Length > 0,
                    CanMoveBack = nextitem.Answers.Any(i => i.Text == "back"),
                    Id = popup.Id
                }));
            }
            else if (nextitem.Type == MenuItemType.Menu)
            {
                var options = new List<OptionsDataItem>();

                foreach (var ans in nextitem.Answers)
                {
                    if (ans.Text == "close")
                        continue;

                    options.Add(new OptionsDataItem((short)ans.Id, ans.Text));
                }

                SendPopupDialog(popup, nextitem.Text, options.ToArray());
            }
        }

        public GameClient StatusCheck()
        {
            var proceed = false;

            if (Aisling.CurrentHp <= 0)
            {
                Aisling.CurrentHp = -1;
                proceed = true;
            }

            if (proceed)
            {
                Aisling.CurrentHp = 1;
                SendStats(StatusFlags.StructB);

                if (Aisling.Map.Flags.HasFlag(MapFlags.PlayerKill))
                {
                    for (var i = 0; i < 2; i++)
                        Aisling.RemoveBuffsAndDebuffs();

                    Aisling.CastDeath();

                    var target = Aisling.Target;

                    if (target != null)
                    {
                        if (target is Aisling)
                            SendMessage(Scope.NearbyAislings, 0x02,
                                Aisling.Username + " has been killed by " + (target as Aisling).Username);
                    }
                    else
                    {
                        SendMessage(Scope.NearbyAislings, 0x02,
                            Aisling.Username + " has been killed, somehow.");
                    }

                    return this;
                }

                if (!Aisling.Skulled)
                {
                    if (Aisling.CurrentMapId == ServerContextBase.Config.DeathMap)
                        return this;

                    var debuff = new debuff_reeping();
                    {
                        debuff.OnApplied(Aisling, debuff);
                    }
                }
            }

            return this;
        }

        public GameClient SystemMessage(string lpmessage)
        {
            SendMessage(0x02, lpmessage);
            return this;
        }

        public void TrainSkill(Skill skill)
        {
            if (skill.Level < skill.Template.MaxLevel)
            {
                var toImprove = (int)(0.10 / skill.Template.LevelRate);
                if (skill.Uses++ >= toImprove)
                {
                    skill.Level++;
                    skill.Uses = 0;
                    Send(new ServerFormat2C(skill.Slot, skill.Icon, skill.Name));

                    SendMessage(0x02, string.Format(CultureInfo.CurrentCulture, "{0} has improved. (Lv. {1})",
                        skill.Template.Name,
                        skill.Level));
                }
            }

            Send(new ServerFormat3F((byte)skill.Template.Pane,
                skill.Slot,
                skill.Template.Cooldown));
        }

        public void TrainSpell(Spell spell)
        {
            if (spell.Level < spell.Template.MaxLevel)
            {
                var toImprove = (int)(0.10 / spell.Template.LevelRate);
                if (spell.Casts++ >= toImprove)
                {
                    spell.Level++;
                    spell.Casts = 0;
                    Send(new ServerFormat17(spell));
                    SendMessage(0x02,
                        string.Format(CultureInfo.CurrentCulture, "{0} has improved.", spell.Template.Name));
                }
            }
        }

        public GameClient TransitionToMap(Area area, Position position)
        {
            if (area == null)
                return null;

            if (area.ID != Aisling.CurrentMapId)
            {
                LeaveArea(true, true);

                Aisling.LastPosition = new Position(Aisling.X, Aisling.Y);
                Aisling.XPos = position.X;
                Aisling.YPos = position.Y;
                Aisling.CurrentMapId = area.ID;

                EnterArea();
            }
            else
            {
                LeaveArea(true);

                Aisling.XPos = position.X;
                Aisling.YPos = position.Y;
                EnterArea();
            }

            Aisling.Client.CloseDialog();

            return this;
        }

        public GameClient TransitionToMap(int area, Position position)
        {
            if (ServerContextBase.GlobalMapCache.ContainsKey(area))
            {
                var target = ServerContextBase.GlobalMapCache[area];
                if (target != null)
                    TransitionToMap(target, position);
            }

            return this;
        }

        public void Update(TimeSpan elapsedTime)
        {
            #region Sanity Checks
            if (Aisling == null)
                return;

            if (!Aisling.LoggedIn)
                return;
            #endregion

            var distance = Aisling.Position.DistanceFrom(Aisling.LastPosition.X, Aisling.LastPosition.Y);

            if (distance > 2 && !IsWarping && (DateTime.UtcNow - LastMapUpdated).TotalMilliseconds > 2000)
            {
                LastWarp = DateTime.UtcNow;
                Aisling.LastPosition.X = (ushort)Aisling.XPos;
                Aisling.LastPosition.Y = (ushort)Aisling.YPos;
                LastLocationSent = DateTime.UtcNow;
                Refresh();
                return;
            }

            if (Aisling.TrapsAreNearby())
            {
                var nextTrap = Trap.Traps.Select(i => i.Value)
                    .FirstOrDefault(i => i.Owner.Serial != Aisling.Serial &&
                                         Aisling.Map.Flags.HasFlag(MapFlags.PlayerKill)
                                         && i.Location.X == Aisling.X && i.Location.Y == Aisling.Y);

                if (nextTrap != null) Trap.Activate(nextTrap, Aisling);
            }

            DoUpdate(elapsedTime);
        }

        public GameClient UpdateDisplay()
        {
            var response = new ServerFormat33(this, Aisling);

            Aisling.Show(Scope.Self, response);

            var nearbyAislings = Aisling.AislingsNearby();

            if (nearbyAislings.Any())
            {
                var myplayer = Aisling;
                foreach (var otherplayer in nearbyAislings)
                {
                    if (myplayer.Serial == otherplayer.Serial)
                        continue;

                    if (!myplayer.Dead && !otherplayer.Dead)
                    {
                        if (myplayer.Invisible)
                            otherplayer.ShowTo(myplayer);
                        else
                            myplayer.ShowTo(otherplayer);

                        if (otherplayer.Invisible)
                            myplayer.ShowTo(otherplayer);
                        else
                            otherplayer.ShowTo(myplayer);
                    }
                    else
                    {
                        if (myplayer.Dead)
                            if (otherplayer.CanSeeGhosts())
                                myplayer.ShowTo(otherplayer);

                        if (otherplayer.Dead)
                            if (myplayer.CanSeeGhosts())
                                otherplayer.ShowTo(myplayer);
                    }
                }
            }

            return this;
        }

        public GameClient UpdateReactors(TimeSpan elapsedTime)
        {
            var inactive = new List<EphemeralReactor>();

            lock (Aisling.ActiveReactors)
            {
                var reactors = Aisling.ActiveReactors.Select(i => i.Value).ToArray();

                foreach (var reactor in reactors)
                {
                    reactor.Update(elapsedTime);

                    if (reactor.Expired) inactive.Add(reactor);
                }
            }

            foreach (var reactor in inactive.Where(reactor => Aisling.ActiveReactors.ContainsKey(reactor.YamlKey)))
                Aisling.ActiveReactors.Remove(reactor.YamlKey);

            return this;
        }

        public GameClient UpdateStatusBar(TimeSpan elapsedTime)
        {
            lock (_syncObj)
            {
                Aisling.UpdateBuffs(elapsedTime);
                Aisling.UpdateDebuffs(elapsedTime);
            }

            return this;
        }

        public void WarpTo(WarpTemplate warps)
        {
            if (warps.WarpType == WarpType.World)
                return;

            if (ServerContextBase.GlobalMapCache.Values.Any(i => i.ID == warps.ActivationMapId))
            {
                if (!Aisling.GameMaster)
                {
                    if (warps.LevelRequired > 0 && Aisling.ExpLevel < warps.LevelRequired)
                    {
                        var msgTier = Math.Abs(Aisling.ExpLevel - warps.LevelRequired);

                        SendMessage(0x02, msgTier <= 10
                            ? string.Format(CultureInfo.CurrentCulture, "You can't enter there just yet. ({0} req)",
                                warps.LevelRequired)
                            : string.Format(CultureInfo.CurrentCulture,
                                "Nightmarish visions of your own death repel you. ({0} Req)", warps.LevelRequired));

                        return;
                    }
                }

                if (Aisling.Map.ID != warps.To.AreaID)
                {
                    TransitionToMap(warps.To.AreaID, warps.To.Location);
                }
                else
                {
                    LeaveArea(true);
                    Aisling.XPos = warps.To.Location.X;
                    Aisling.YPos = warps.To.Location.Y;
                    EnterArea();
                    Aisling.Client.CloseDialog();
                }
            }
        }

        public void WarpTo(Position position)
        {
            Aisling.XPos = position.X;
            Aisling.YPos = position.Y;

            Refresh();
        }

        private GameClient Enter()
        {
            SendSerial();
            Insert();
            RefreshMap();
            UpdateDisplay();
            SendLocation();

            return this;
        }
    }
}