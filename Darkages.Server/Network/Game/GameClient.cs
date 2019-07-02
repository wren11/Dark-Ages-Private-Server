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
using CLAP;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Network.Game
{
    public class GameClient : NetworkClient<GameClient>
    {
        public Collection<GlobalScript> GlobalScripts = new Collection<GlobalScript>();

        public GameServer Server;

        public Aisling Aisling;

        public GameServerTimer HpRegenTimer;

        public GameServerTimer MpRegenTimer;

        public Interpreter MenuInterpter;

        public DialogSession DlgSession;

        public Item LastItemDropped;

        public DateTime BoardOpened;

        public DateTime LastWhisperMessageSent;

        public DateTime LastAssail;

        public DateTime LastMessageSent;

        public DateTime LastPingResponse;

        public DateTime LastWarp;

        public DateTime LastScriptExecuted;

        public DateTime LastPing;

        public DateTime LastSave;

        public DateTime LastClientRefresh;

        public bool IsRefreshing =>
            DateTime.UtcNow - LastClientRefresh < new TimeSpan(0, 0, 0, 0, ServerContext.Config.RefreshRate);

        public bool IsWarping =>
            DateTime.UtcNow - LastWarp < new TimeSpan(0, 0, 0, 0, ServerContext.Config.WarpCheckRate);

        public DateTime LastLocationSent { get; set; }

        public bool CanSendLocation =>
            DateTime.UtcNow - LastLocationSent < new TimeSpan(0, 0, 0, 1);

        [JsonIgnore]
        public PendingSell PendingItemSessions { get; set; }

        public ushort LastBoardActivated;

        public bool ShouldUpdateMap;

        public byte LastActivatedLost;

        public GameClient()
        {
            HpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate));

            MpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate / 2));
        }



        [Verb]

        ///
        /// This Chat command ports the user to a location.
        /// Example chat command: 'port -i:100 -x:5 -y:5'
        public void port(int i, int x = 0, int y = 0)
        {
            TransitionToMap(i, new Position(x, y));

            SystemMessage("Port: Success.");
        }

        /// <summary>
        /// This chat command spawns a monster.
        /// </summary>
        /// <param name="t">Name of Monster, Case Sensitive.</param>
        /// <param name="x">X Location to Spawn.</param>
        /// <param name="y">Y Location to Spawn.</param>
        /// <param name="c"></param>
        /// <usage>spawnMonster -t:Undead -x:43 -y:16 -c:10</usage>
        [Verb]
        public void spawn(string t, int x, int y, int c)
        {
            var name = t.Replace("-", string.Empty).Trim();

            var obj = ServerContext.GlobalMonsterTemplateCache
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.CurrentCulture));

            if (obj != null)
            {
                for (int i = 0; i < c; i++)
                {
                    var mon = Monster.Create(obj, Aisling.Map);
                    if (mon != null)
                    {

                        mon.XPos = x;
                        mon.YPos = y;

                        AddObject(mon);

                    }
                }
                SystemMessage("spawnMonster: Success.");
            }
            else
            {
                SystemMessage("spawnMonster: Failed.");
            }
        }



        /// <summary>
        /// Add Exp
        /// </summary>
        /// Example Chat command: addExp -a:10000
        /// <param name="a">Ammount of exp to give</param>
        [Verb]        
        public void addexp(int a)
        {
            Monster.DistributeExperience(Aisling, a);
        }

        [Verb]
        public async void eff(ushort n, int d = 1000, int r = 1)
        {
            if (r <= 0)
                r = 1;

            for (int i = 0; i < r; i++)
            {
                Aisling.SendAnimation(n, Aisling, Aisling);
                await Task.Delay(d);
            }
        }

        [Verb]
        public void life(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                user.Client.Revive();
            }
        }

        [Verb]
        public void death(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                user.CurrentHp = 0; // The system will take care of the rest.
            }
        }


        /// <summary>
        /// This chat command reloads all objects.
        /// </summary>
        /// <param name="all">[Optional] all objects | true or false</param>
        /// <usage>reload -all:true|false</usage> 
        /// <usage>reload</usage> 
        [Verb]
        public void reload(bool all = false)
        {
            lock (ServerContext.SyncObj)
            {
                var objs = GetObjects(null, i => i != null && i.Serial != Aisling.Serial,
                    all ? Get.All : Get.Items | Get.Money | Get.Monsters | Get.Mundanes);

                foreach (var obj in objs)
                {
                    obj.Remove();
                }

                ServerContext.LoadAndCacheStorage();
            }
        }

        public void BuildSettings()
        {
            if (ServerContext.Config.Settings == null || ServerContext.Config.Settings.Length == 0)
                return;

            if (Aisling.GameSettings == null || Aisling.GameSettings.Count == 0)
            {
                Aisling.GameSettings = new List<ClientGameSettings>();

                foreach (var settings in ServerContext.Config.Settings)
                {
                    Aisling.GameSettings.Add(new ClientGameSettings(settings.SettingOff, settings.SettingOn, settings.Enabled));
                }
            }
        }

        public bool IsDead()
        {
            var result = Aisling != null && Aisling.Flags.HasFlag(AislingFlags.Dead);

            return result;
        }

        public bool CanSeeGhosts()
        {
            return IsDead();
        }

        public bool CanSeeHidden()
        {
            return Aisling != null && (Aisling.Flags & AislingFlags.SeeInvisible) == AislingFlags.SeeInvisible;
        }

        public void WarpTo(WarpTemplate warps)
        {
            if (warps.WarpType == WarpType.World)
                return;

            if (ServerContext.GlobalMapCache.Values.Any(i => i.ID == warps.ActivationMapId))
            {
                if (warps.LevelRequired > 0 && Aisling.ExpLevel < warps.LevelRequired)
                {
                    var msgTier = Math.Abs(Aisling.ExpLevel - warps.LevelRequired);

                    SendMessage(0x02, msgTier <= 10
                        ? string.Format("You can't enter there just yet. ({0} req)", warps.LevelRequired)
                        : string.Format("Nightmarish visions of your own death repel you. ({0} Req)", warps.LevelRequired));
                    return;
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

        public void LearnSpell(Mundane Source, SpellTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Spell.GiveTo(this, subject.Name);
                SendOptionsDialog(Source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint)Aisling.Serial, (uint)Source.Serial,
                    subject?.TargetAnimation ?? 124,
                    subject?.TargetAnimation ?? 124, 100));
            }
        }

        public void LearnSkill(Mundane Source, SkillTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Skill.GiveTo(this, subject.Name);
                SendOptionsDialog(Source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint)Aisling.Serial, (uint)Source.Serial,
                    subject?.TargetAnimation ?? 124,
                    subject?.TargetAnimation ?? 124, 100));
            }
        }

        public bool PayPrerequisites(LearningPredicate prerequisites)
        {
            if (prerequisites == null)
            {
                return false;
            }

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

        private void PayItemPrerequisites(LearningPredicate prerequisites)
        {
            if (prerequisites.Items_Required != null && prerequisites.Items_Required.Count > 0)
            {
                foreach (var retainer in prerequisites.Items_Required)
                {
                    var item = Aisling.Inventory.Get(i => i.Template.Name == retainer.Item);

                    foreach (var i in item)
                    {
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
                }
            }
        }

        public void TransitionToMap(Area area, Position position)
        {
            if (area == null)
                return;

            if (area.ID != Aisling.CurrentMapId)
            {
                LeaveArea(true);
                Aisling.XPos = position.X;
                Aisling.YPos = position.Y;
                Aisling.CurrentMapId = area.ID;
                EnterArea();
            }
            else
            {
                LeaveArea(true, false);
                Aisling.XPos = position.X;
                Aisling.YPos = position.Y;
                EnterArea();
            }

            Aisling.Client.CloseDialog();
        }

        public void TransitionToMap(int area, Position position)
        {
            if (ServerContext.GlobalMapCache.ContainsKey(area))
            {
                var target = ServerContext.GlobalMapCache[area];
                if (target != null)
                {
                    TransitionToMap(target, position);
                }
            }
        }

        public void CloseDialog()
        {
            Send(new byte[] { 0x30, 0x00, 0x0A, 0x00 });
            MenuInterpter = null;
        }


        public void Update(TimeSpan elapsedTime)
        {
            #region Sanity Checks
            if (Aisling == null)
                return;

            if (!Aisling.LoggedIn)
                return;

            if ((DateTime.UtcNow - Aisling.LastLogged).TotalMilliseconds < ServerContext.Config.LingerState)
                return;
            #endregion

            #region Warping Sanity Check
            var distance = Aisling.Position.DistanceFrom(Aisling.LastPosition);

            if (distance > 2)
            {
                LastWarp               = DateTime.UtcNow;
                Aisling.LastPosition.X = (ushort)Aisling.XPos;
                Aisling.LastPosition.Y = (ushort)Aisling.YPos;

                Refresh();
                return;
            }
            #endregion

            HandleTimeOuts();
            StatusCheck();
            Regen(elapsedTime);
            UpdateStatusBar(elapsedTime);
            UpdateGlobalScripts(elapsedTime);
            UpdateReactors(elapsedTime);
        }

        private void UpdateReactors(TimeSpan elapsedTime)
        {
            List<EphemeralReactor> Inactive = new List<EphemeralReactor>();

            lock (Aisling.ActiveReactors)
            {
                var reactors = Aisling.ActiveReactors.Select(i => i.Value).ToArray();

                foreach (var reactor in reactors)
                {

                    reactor.Update(elapsedTime);

                    if (reactor.Elapsed)
                    {
                        Inactive.Add(reactor);
                    }
                }
            }

            //Remove inactive reactors.
            foreach (var reactor in Inactive)
            {
                if (Aisling.ActiveReactors.ContainsKey(reactor.YamlKey))
                {
                    Aisling.ActiveReactors.Remove(reactor.YamlKey);
                }
            }
        }

        public void SystemMessage(string lpmessage)
        {
            SendMessage(0x02, lpmessage);
        }

        private void StatusCheck()
        {
            bool proceed = false;

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
                    //dirty: one extra pass here, just to ENSURE all buffs are gone.
                    for (int i = 0; i < 2; i++)
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

                    return;
                }


                if (!Aisling.Skulled)
                {
                    if (Aisling.CurrentMapId == ServerContext.Config.DeathMap)
                    {
                        return;
                    }

                    var debuff = new debuff_reeping();
                    {
                        debuff.OnApplied(Aisling, debuff);
                        return;
                    }
                }
            }
        }

        private void HandleTimeOuts()
        {
            if (Aisling.Exchange != null)
            {
                if (Aisling.Exchange.Trader != null)
                {
                    if (!Aisling.Exchange.Trader.LoggedIn
                    || !Aisling.WithinRangeOf(Aisling.Exchange.Trader))
                    {
                        Aisling.CancelExchange();
                    }
                }
            }

            if (Aisling.PortalSession != null && (DateTime.UtcNow - Aisling.PortalSession.DateOpened).TotalSeconds > 10)
            {
                if (Aisling.PortalSession.IsMapOpen)
                {
                    Aisling.GoHome();
                }
                Aisling.PortalSession = null;
            }
        }

        public void UpdateStatusBar(TimeSpan elapsedTime)
        {
            lock (Aisling)
            {
                Aisling.UpdateBuffs(elapsedTime);
                Aisling.UpdateDebuffs(elapsedTime);
            }
        }

        public void UpdateGlobalScripts(TimeSpan elapsedTime)
        {
            lock (GlobalScripts)
            {
                foreach (var globalscript in GlobalScripts)
                    globalscript?.Update(elapsedTime);
            }
        }

        public async Task<bool> LoadAsync()
        {
            if (Aisling == null || Aisling.AreaID == 0)
                return false;
            if (!ServerContext.GlobalMapCache.ContainsKey(Aisling.AreaID))
                return false;

            SetAislingStartupVariables();

            try
            {
                await Task.Run(() =>
                    LoadGlobalScripts()
                        .InitSpellBar()
                        .LoadInventory()
                        .LoadSkillBook()
                        .LoadSpellBook()
                        .LoadEquipment()
                        .SendProfileUpdate()
                ).ContinueWith((ct) =>
                {
                    SendStats(StatusFlags.All);
                    Thread.Sleep(100);
                    return true;
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        private GameClient SetAislingStartupVariables()
        {
            LastSave            = DateTime.UtcNow;
            LastPingResponse    = DateTime.UtcNow;
            PendingItemSessions = null;

            BoardOpened = DateTime.UtcNow;
            {
                Aisling.BonusAc = (int)(70 - Aisling.Level * 0.5 / 1.0);
                Aisling.Exchange = null;
                Aisling.LastMapId = short.MaxValue;
            }
            BuildSettings();

            return this;
        }

        private GameClient LoadGlobalScripts()
        {
            foreach (var script in ServerContext.Config.GlobalScripts)
                GlobalScripts.Add(ScriptManager.Load<GlobalScript>(script, this));

            return this;
        }

        private void Regen(TimeSpan elapsedTime)
        {
            if (Aisling.Con > Aisling.ExpLevel + 1)
                HpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate / 3);

            if (Aisling.Wis > Aisling.ExpLevel + 1)
                MpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate / 3);

            HpRegenTimer.Update(elapsedTime);
            MpRegenTimer.Update(elapsedTime);

            if (HpRegenTimer.Elapsed)
            {
                HpRegenTimer.Reset();

                var hpRegenSeed = (Aisling.Con - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var hpRegenAmount = (Aisling.MaximumHp * (hpRegenSeed + 0.10));

                Aisling.CurrentHp = (Aisling.CurrentHp + (int)hpRegenAmount).Clamp(0, Aisling.MaximumHp);
                SendStats(StatusFlags.StructB);
            }

            if (MpRegenTimer.Elapsed)
            {
                MpRegenTimer.Reset();
                var mpRegenSeed = (Aisling.Wis - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var mpRegenAmount = (Aisling.MaximumMp * (mpRegenSeed + 0.10));

                Aisling.CurrentMp = (Aisling.CurrentMp + (int)mpRegenAmount).Clamp(0, Aisling.MaximumMp);
                SendStats(StatusFlags.StructB);
            }

        }

        private GameClient InitSpellBar()
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

            return this;
        }

        private GameClient LoadEquipment()
        {
            var formats = new List<NetworkFormat>();

            foreach (var item in Aisling.EquipmentManager.Equipment)
            {
                var equipment = item.Value;

                if (equipment == null || equipment.Item == null || equipment.Item.Template == null)
                    continue;

                if (equipment.Item.Template != null)
                {
                    if (ServerContext.GlobalItemTemplateCache.ContainsKey(equipment.Item.Template.Name))
                    {
                        var template = ServerContext.GlobalItemTemplateCache[equipment.Item.Template.Name];
                        {
                            item.Value.Item.Template = template;

                            if (item.Value.Item.Upgrades > 0)
                                Item.ApplyQuality(item.Value.Item);
                        }
                    }
                }


                equipment.Item.Script = ScriptManager.Load<ItemScript>(equipment.Item.Template.ScriptName, equipment.Item);
                if (!string.IsNullOrEmpty(equipment.Item.Template.WeaponScript))
                    equipment.Item.WeaponScript = ScriptManager.Load<WeaponScript>(equipment.Item.Template.WeaponScript, equipment.Item);

                equipment.Item.Script?.Equipped(Aisling, (byte)equipment.Slot);

                if (equipment.Item.CanCarry(Aisling))
                {
                    //apply weight to items that are equipped.
                    Aisling.CurrentWeight += equipment.Item.Template.CarryWeight;

                    formats.Add(new ServerFormat37(equipment.Item, (byte)equipment.Slot));
                }
                //for some reason, Aisling is out of Weight!
                else
                {
                    //clone and release item
                    var nitem = Clone<Item>(item.Value.Item);
                    nitem.Release(Aisling, Aisling.Position);

                    //display message
                    SendMessage(0x02, string.Format("{0} is too heavy to hold.", nitem.Template.Name));

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

            foreach (var format in formats)
                Aisling.Client.Send(format);


            return this;
        }

        private GameClient LoadSpellBook()
        {
            var spells_Available = Aisling.SpellBook.Spells.Values
                .Where(i => i != null && i.Template != null).ToArray();

            for (var i = 0; i < spells_Available.Length; i++)
            {
                var spell = spells_Available[i];

                if (spell.Template != null)
                {
                    if (ServerContext.GlobalSpellTemplateCache.ContainsKey(spell.Template.Name))
                    {
                        var template = ServerContext.GlobalSpellTemplateCache[spell.Template.Name];
                        {
                            spell.Template = template;
                        }
                    }
                }

                spell.InUse = false;
                spell.NextAvailableUse = DateTime.UtcNow;
                spell.Lines = spell.Template.BaseLines;

                Spell.AttachScript(Aisling, spell);
                {
                    Aisling.SpellBook.Set(spell, false);
                }

                Send(new ServerFormat17(spell));
            }

            return this;
        }

        private GameClient LoadSkillBook()
        {
            var skills_Available = Aisling.SkillBook.Skills.Values
                .Where(i => i != null && i.Template != null).ToArray();

            var formats = new List<NetworkFormat>();

            for (var i = 0; i < skills_Available.Length; i++)
            {
                var skill = skills_Available[i];

                if (skill.Template != null)
                {
                    if (ServerContext.GlobalSkillTemplateCache.ContainsKey(skill.Template.Name))
                    {
                        var template = ServerContext.GlobalSkillTemplateCache[skill.Template.Name];
                        {
                            skill.Template = template;
                        }
                    }
                }

                skill.InUse = false;
                skill.NextAvailableUse = DateTime.UtcNow;

                Send(new ServerFormat2C(skill.Slot,
                    skill.Icon,
                    skill.Name));

                skill.Script = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
                Aisling.SkillBook.Set(skill, false);
            }

            return this;
        }

        private GameClient LoadInventory()
        {
            var items_Available = Aisling.Inventory.Items.Values
                .Where(i => i != null && i.Template != null).ToArray();

            for (var i = 0; i < items_Available.Length; i++)
            {
                var item = items_Available[i];

                if (string.IsNullOrEmpty(item.Template.Name))
                    continue;


                item.Script = ScriptManager.Load<ItemScript>(item.Template.ScriptName, item);

                if (!string.IsNullOrEmpty(item.Template.WeaponScript))
                {
                    item.WeaponScript = ScriptManager.Load<WeaponScript>(item.Template.WeaponScript, item);
                }

                if (ServerContext.GlobalItemTemplateCache.ContainsKey(item.Template.Name))
                {
                    var template = ServerContext.GlobalItemTemplateCache[item.Template.Name];
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
                    //for some reason, Aisling is out of Weight!
                    else
                    {
                        //clone and release item
                        var copy = Clone<Item>(item);
                        {
                            copy.Release(Aisling, Aisling.Position);

                            //display message
                            SendMessage(0x02, string.Format("You stumble and drop {0}", item.Template.Name));
                        }
                    }
                }
            }

            return this;
        }

        public GameClient UpdateDisplay()
        {
            //construct display Format for dispatching out.
            var response = new ServerFormat33(this, Aisling);

            //Display Aisling to self.
            Aisling.Show(Scope.Self, response);

            //Display Aisling to everyone else nearby.
            if (Aisling.Flags.HasFlag(AislingFlags.Dead))
            {
                //only show to clients who can see ghosts.
                var nearby = GetObjects<Aisling>(Aisling.Map, i => i.WithinRangeOf(Aisling) && i.Client.CanSeeGhosts());

                if (nearby != null)
                {
                    Aisling.Show(Scope.NearbyAislingsExludingSelf, response, nearby);
                }

                return this;
            }
            else
            {
                Aisling.Show(Scope.NearbyAislingsExludingSelf, response);
            }

            return this;
        }

        public void Refresh(bool delete = false)
        {
            LeaveArea(delete);
            EnterArea();
        }

        public void LeaveArea(bool update = false, bool delete = false)
        {
            if (Aisling.LastMapId == short.MaxValue)
            {
                Aisling.LastMapId = Aisling.CurrentMapId;
            }

            Aisling.Remove(update, delete);
        }

        public void EnterArea() => Enter();

        private GameClient Enter()
        {
            SendSerial();
            Insert();
            RefreshMap();
            SendLocation();
            UpdateDisplay();

            return this;
        }

        public void SendMusic()
        {
            Aisling.Client.Send(new byte[]
            {
                0x19, 0x00, 0xFF,
                (byte) Aisling.Map.Music
            });
        }

        public void SendSound(byte sound, Scope scope = Scope.Self)
        {
            var empty = new ServerFormat13
            {
                Serial = Aisling.Serial,
                Health = byte.MaxValue,
                Sound = sound
            };

            Aisling.Show(scope, empty);
        }

        /// <summary>
        /// Client.Insert: if map is ready (loaded), Inserts an Aisling onto the map in question.
        /// condition: if it's not present in the object manager.
        /// 
        /// true: inserts the object into the object manager, then updates the Map tile location.
        /// false: does not insert the object into the object manager.
        /// 
        /// Note: It will update the map object grid regardless of the above condition.
        /// </summary>       
        public void Insert()
        {
            if (!Aisling.Map.Ready)
                return;

            if (GetObject<Aisling>(Aisling.Map, i => i.Serial == Aisling.Serial) == null)
                AddObject(Aisling);

            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling);
        }

        public bool MapUpdating;

        public void RefreshMap()
        {
            ShouldUpdateMap = false;

            if (Aisling.CurrentMapId != Aisling.LastMapId)
            {
                ShouldUpdateMap    = true;
                Aisling.LastMapId  = Aisling.CurrentMapId;

                if (!Aisling.DiscoveredMaps.Any(i => i == Aisling.CurrentMapId))
                {
                    Aisling.DiscoveredMaps.Add(Aisling.CurrentMapId);
                }

                SendMusic();
            }


            if (ShouldUpdateMap)
            {
                MapUpdating = true;
                Aisling.View.Clear();
                Send(new ServerFormat15(Aisling.Map));
            }
            else
            {
                Aisling.View.Clear();
            }
        }

        private void SendSerial()
        {
            Send(new ServerFormat05(Aisling));
        }

        public void SendLocation()
        {
            CloseDialog();
            {
                Send(new ServerFormat04(Aisling));
                LastLocationSent = DateTime.UtcNow;
            }
        }

        public void Save()
        {
            lock (ServerContext.SyncObj)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    StorageManager.AislingBucket.Save(Aisling);
                    {
                        LastSave = DateTime.UtcNow;
                    }
                });
            }
        }

        public void SendMessage(byte type, string text)
        {
            Send(new ServerFormat0A(type, text));
            LastMessageSent = DateTime.UtcNow;
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

        public void SendAnimation(ushort Animation, Sprite To, Sprite From, byte speed = 100)
        {
            var format = new ServerFormat29((uint)From.Serial, (uint)To.Serial, Animation, 0, speed);
            Aisling.Show(Scope.NearbyAislings, format);
        }

        public void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemShopData(step, items)));
        }

        public void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemSellData(step, items)));
        }

        public void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsData(options)));
        }

        public void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options)
        {
            Send(new PopupFormat(popup, text, new OptionsData(options)));
        }

        public void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsPlusArgsData(options, args)));
        }

        public void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills)
        {
            Send(new ServerFormat2F(mundane, text, new SkillAcquireData(step, skills)));
        }

        public void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells)
        {
            Send(new ServerFormat2F(mundane, text, new SpellAcquireData(step, spells)));
        }

        public void SendSkillForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SkillForfeitData(step)));
        }

        public void SendSpellForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SpellForfeitData(step)));
        }

        public void SendStats(StatusFlags flags)
        {
            Send(new ServerFormat08(Aisling, flags));
        }

        public void SendProfileUpdate()
        {
            Send(new byte[] { 73, 0x00 });
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
                    SendMessage(0x02, string.Format("{0} has improved.", spell.Template.Name));
                }
            }
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

                    SendMessage(0x02, string.Format("{0} has improved. (Lv. {1})",
                        skill.Template.Name,
                        skill.Level));
                }
            }

            Send(new ServerFormat3F((byte)skill.Template.Pane,
                skill.Slot,
                skill.Template.Cooldown));
        }

        /// <summary>
        ///     Stop and Interupt everything this client is doing.
        /// </summary>
        public void Interupt()
        {
            GameServer.CancelIfCasting(this);
            SendLocation();
        }

        public void WarpTo(Position position)
        {
            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling, true);

            Aisling.XPos = position.X;
            Aisling.YPos = position.Y;

            Refresh();

            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling);
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
            Aisling.CurrentHp     = Aisling.MaximumHp / 6;
            Aisling.Flags         = AislingFlags.Normal;
            HpRegenTimer.Disabled = false;
            MpRegenTimer.Disabled = false;

            Aisling.Recover();
            return Aisling.CurrentHp > 0;
        }

        public bool CheckReqs(GameClient client, Item item)
        {
            var message = string.Empty;

            if (client.Aisling.ExpLevel < item.Template.LevelRequired)
            {
                message = ServerContext.Config.CantWearYetMessage;
                if (message != string.Empty)
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (item.Durability <= 0)
            {
                message = ServerContext.Config.RepairItemMessage;
                if (message != string.Empty)
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (client.Aisling.Path != item.Template.Class && !(item.Template.Class == Class.Peasant))
            {
                if (client.Aisling.ExpLevel >= item.Template.LevelRequired)
                {
                    message = ServerContext.Config.WrongClassMessage;
                }
                else
                {
                    message = ServerContext.Config.CantWearYetMessage;
                }
            }

            if (message != string.Empty)
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
                        client.SendMessage(0x02, ServerContext.Config.DoesNotFitMessage);
                        return false;
                    }
                }

                return true;
            }

            client.SendMessage(0x02, ServerContext.Config.CantEquipThatMessage);
            return false;
        }

        public bool HasInInventory(string item, int count)
        {
            var template = ServerContext.GlobalItemTemplateCache[item];

            if (!ServerContext.GlobalItemTemplateCache.ContainsKey(item))
                return false;

            if (template != null)
            {
                return Aisling.Inventory.Has(template) > 0;
            }

            return false;
        }

        public bool IsWearing(string item)
        {
            return Aisling.EquipmentManager.Equipment.Any(i => i.Value != null && i.Value.Item.Template.Name == item);
        }

        public bool HasKilled(string monster, int count)
        {
            return Aisling.HasKilled(monster, count);
        }

        public bool HasVisitedMap(int mapId)
        {
            return Aisling.DiscoveredMaps.Contains(mapId);
        }

        public void ShowCurrentMenu(Sprite obj, MenuItem currentitem, MenuItem nextitem)
        {
            if (nextitem == null)
                return;

            nextitem.Text = nextitem.Text.Replace("%aisling%", Aisling.Username);

            if (nextitem == null)
            {
                return;
            }
            if (nextitem.Type == MenuItemType.Step)
            {
                Send(new ReactorSequence(this, new DialogSequence()
                {
                    DisplayText = nextitem.Text,
                    HasOptions = false,
                    DisplayImage = (ushort)(obj as Mundane).Template.Image,
                    Title = (obj as Mundane).Template.Name,
                    CanMoveNext = nextitem.Answers.Length > 0,
                    CanMoveBack = nextitem.Answers.Any(i => i.Text == "back"),
                    Id = obj.Serial,
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
                    ServerContext.ILog.Debug($"{ans.Id}. {ans.Text}");
                }

                SendOptionsDialog(obj as Mundane, nextitem.Text, options.ToArray());
            }
        }

        public void ShowCurrentMenu(Popup popup, MenuItem currentitem, MenuItem nextitem)
        {
            if (nextitem == null)
                return;

            nextitem.Text = nextitem.Text.Replace("%aisling%", Aisling.Username);

            if (nextitem == null)
            {
                return;
            }
            if (nextitem.Type == MenuItemType.Step)
            {
                Send(new ReactorSequence(this, new DialogSequence()
                {
                    DisplayText = nextitem.Text,
                    HasOptions = false,
                    DisplayImage = (ushort)popup.Template.SpriteId,
                    Title = popup.Template.Name,
                    CanMoveNext = nextitem.Answers.Length > 0,
                    CanMoveBack = nextitem.Answers.Any(i => i.Text == "back"),
                    Id = popup.Id,
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
                    ServerContext.ILog.Debug($"{ans.Id}. {ans.Text}");
                }

                 SendPopupDialog(popup, nextitem.Text, options.ToArray());
            }
        }
    }
}