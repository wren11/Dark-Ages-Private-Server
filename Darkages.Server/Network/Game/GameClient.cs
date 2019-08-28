// ************************************************************************
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
//*************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CLAP;
using Darkages.Common;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using MenuInterpreter;
using Newtonsoft.Json;

namespace Darkages.Network.Game
{
    /// <summary>
    ///     Class GameClient.
    ///     Implements the <see cref="Darkages.Network.NetworkClient{Darkages.Network.Game.GameClient}" />
    /// </summary>
    /// <seealso cref="Darkages.Network.NetworkClient{Darkages.Network.Game.GameClient}" />
    public partial class GameClient : NetworkClient<GameClient>, IDisposable
    {
        /// <summary>
        ///     The aisling
        /// </summary>
        private Aisling _aisling;

        /// <summary>
        ///     The board opened
        /// </summary>
        private DateTime _boardOpened;

        /// <summary>
        ///     The dialog session
        /// </summary>
        private DialogSession _dlgSession;

        /// <summary>
        ///     The global scripts
        /// </summary>
        private Collection<GlobalScript> _globalScripts = new Collection<GlobalScript>();

        /// <summary>
        ///     The hp regen timer
        /// </summary>
        private GameServerTimer _hpRegenTimer;

        /// <summary>
        ///     The last activated lost
        /// </summary>
        private byte _lastActivatedLost;

        /// <summary>
        ///     The last assail
        /// </summary>
        private DateTime _lastAssail;

        /// <summary>
        ///     The last board activated
        /// </summary>
        private ushort _lastBoardActivated;

        /// <summary>
        ///     The last client refresh
        /// </summary>
        private DateTime _lastClientRefresh;

        /// <summary>
        ///     The last item dropped
        /// </summary>
        private Item _lastItemDropped;

        /// <summary>
        ///     The last message sent
        /// </summary>
        private DateTime _lastMessageSent;

        /// <summary>
        ///     The last ping
        /// </summary>
        private DateTime _lastPing;

        /// <summary>
        ///     The last ping response
        /// </summary>
        private DateTime _lastPingResponse;

        /// <summary>
        ///     The last save
        /// </summary>
        private DateTime _lastSave;

        /// <summary>
        ///     The last script executed
        /// </summary>
        private DateTime _lastScriptExecuted;

        /// <summary>
        ///     The last warp
        /// </summary>
        private DateTime _lastWarp;

        /// <summary>
        ///     The last whisper message sent
        /// </summary>
        private DateTime _lastWhisperMessageSent;

        /// <summary>
        ///     The menu interpter
        /// </summary>
        private Interpreter _menuInterpter;

        /// <summary>
        ///     The mp regen timer
        /// </summary>
        private GameServerTimer _mpRegenTimer;

        /// <summary>
        ///     The server
        /// </summary>
        private GameServer _server;

        /// <summary>
        ///     The should update map
        /// </summary>
        private bool _shouldUpdateMap;

        /// <summary>
        ///     The map updating
        /// </summary>
        public bool MapUpdating;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        [JsonConstructor]
        public GameClient()
        {
            HPRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate));

            MPRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate));

            PropertyChanged += GameClient_PropertyChanged;
        }

        private void GameClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedNodeIndex"))
            {
                var node = SelectedNodeIndex;

                if (node > 0)
                {

                    LastSelectedNodeIndex = node;

                    FlushBuffers();

                    MapOpen = false;
                    LastWarp = DateTime.UtcNow;
                    ShouldUpdateMap = true;

                    GameServer.HandleMapNodeSelection(this, node);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the global scripts.
        /// </summary>
        /// <value>The global scripts.</value>
        public Collection<GlobalScript> GlobalScripts
        {
            get => _globalScripts;
            set => _globalScripts = value;
        }

        /// <summary>
        ///     Gets or sets the server.
        /// </summary>
        /// <value>The server.</value>
        public GameServer Server
        {
            get => _server;
            set => _server = value;
        }

        /// <summary>
        ///     Gets or sets the aisling.
        /// </summary>
        /// <value>The aisling.</value>
        public Aisling Aisling
        {
            get => _aisling;
            set => _aisling = value;
        }

        /// <summary>
        ///     Gets or sets the hp regen timer.
        /// </summary>
        /// <value>The hp regen timer.</value>
        private GameServerTimer HPRegenTimer
        {
            get => _hpRegenTimer;
            set => _hpRegenTimer = value;
        }

        /// <summary>
        ///     Gets or sets the mp regen timer.
        /// </summary>
        /// <value>The mp regen timer.</value>
        public GameServerTimer MPRegenTimer
        {
            get => _mpRegenTimer;
            set => _mpRegenTimer = value;
        }

        /// <summary>
        ///     Gets or sets the menu interpter.
        /// </summary>
        /// <value>The menu interpter.</value>
        public Interpreter MenuInterpter
        {
            get => _menuInterpter;
            set => _menuInterpter = value;
        }

        /// <summary>
        ///     Gets or sets the dialog session.
        /// </summary>
        /// <value>The dialog session.</value>
        public DialogSession DlgSession
        {
            get => _dlgSession;
            set => _dlgSession = value;
        }

        /// <summary>
        ///     Gets or sets the last item dropped.
        /// </summary>
        /// <value>The last item dropped.</value>
        public Item LastItemDropped
        {
            get => _lastItemDropped;
            set => _lastItemDropped = value;
        }

        /// <summary>
        ///     Gets or sets the board opened.
        /// </summary>
        /// <value>The board opened.</value>
        public DateTime BoardOpened
        {
            get => _boardOpened;
            set => _boardOpened = value;
        }

        /// <summary>
        ///     Gets or sets the last whisper message sent.
        /// </summary>
        /// <value>The last whisper message sent.</value>
        public DateTime LastWhisperMessageSent
        {
            get => _lastWhisperMessageSent;
            set => _lastWhisperMessageSent = value;
        }

        /// <summary>
        ///     Gets or sets the last assail.
        /// </summary>
        /// <value>The last assail.</value>
        public DateTime LastAssail
        {
            get => _lastAssail;
            set => _lastAssail = value;
        }

        /// <summary>
        ///     Gets or sets the last message sent.
        /// </summary>
        /// <value>The last message sent.</value>
        public DateTime LastMessageSent
        {
            get => _lastMessageSent;
            set => _lastMessageSent = value;
        }

        /// <summary>
        ///     Gets or sets the last ping response.
        /// </summary>
        /// <value>The last ping response.</value>
        public DateTime LastPingResponse
        {
            get => _lastPingResponse;
            set => _lastPingResponse = value;
        }

        /// <summary>
        ///     Gets or sets the last warp.
        /// </summary>
        /// <value>The last warp.</value>
        public DateTime LastWarp
        {
            get => _lastWarp;
            set => _lastWarp = value;
        }

        /// <summary>
        ///     Gets or sets the last script executed.
        /// </summary>
        /// <value>The last script executed.</value>
        public DateTime LastScriptExecuted
        {
            get => _lastScriptExecuted;
            set => _lastScriptExecuted = value;
        }

        /// <summary>
        ///     Gets or sets the last ping.
        /// </summary>
        /// <value>The last ping.</value>
        public DateTime LastPing
        {
            get => _lastPing;
            set => _lastPing = value;
        }

        /// <summary>
        ///     Gets or sets the last save.
        /// </summary>
        /// <value>The last save.</value>
        public DateTime LastSave
        {
            get => _lastSave;
            set => _lastSave = value;
        }

        /// <summary>
        ///     Gets or sets the last client refresh.
        /// </summary>
        /// <value>The last client refresh.</value>
        public DateTime LastClientRefresh
        {
            get => _lastClientRefresh;
            set => _lastClientRefresh = value;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value><c>true</c> if this instance is refreshing; otherwise, <c>false</c>.</value>
        public bool IsRefreshing =>
            DateTime.UtcNow - LastClientRefresh < new TimeSpan(0, 0, 0, 0, ServerContext.Config.RefreshRate);

        /// <summary>
        ///     Gets a value indicating whether this instance is warping.
        /// </summary>
        /// <value><c>true</c> if this instance is warping; otherwise, <c>false</c>.</value>
        public bool IsWarping =>
            DateTime.UtcNow - LastWarp < new TimeSpan(0, 0, 0, 0, ServerContext.Config.WarpCheckRate);

        /// <summary>
        ///     Gets a value indicating whether this instance can send location.
        /// </summary>
        /// <value><c>true</c> if this instance can send location; otherwise, <c>false</c>.</value>
        public bool CanSendLocation =>
            DateTime.UtcNow - LastLocationSent < new TimeSpan(0, 0, 0, 2);


        public bool WasUpdatingMapRecently =>
            DateTime.UtcNow - LastMapUpdated < new TimeSpan(0, 0, 0, 0, 100);


        /// <summary>
        ///     Gets or sets the last location sent.
        /// </summary>
        /// <value>The last location sent.</value>
        public DateTime LastLocationSent { get; set; }

        public DateTime LastMapUpdated { get; set; }

        /// <summary>
        ///     Gets or sets the last board activated.
        /// </summary>
        /// <value>The last board activated.</value>
        public ushort LastBoardActivated
        {
            get => _lastBoardActivated;
            set => _lastBoardActivated = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [should update map].
        /// </summary>
        /// <value><c>true</c> if [should update map]; otherwise, <c>false</c>.</value>
        public bool ShouldUpdateMap
        {
            get => _shouldUpdateMap;
            set => _shouldUpdateMap = value;
        }

        /// <summary>
        ///     Gets or sets the last activated lost.
        /// </summary>
        /// <value>The last activated lost.</value>
        public byte LastActivatedLost
        {
            get => _lastActivatedLost;
            set => _lastActivatedLost = value;
        }

        /// <summary>
        ///     Gets or sets the pending item sessions.
        /// </summary>
        /// <value>The pending item sessions.</value>
        [JsonIgnore]
        public PendingSell PendingItemSessions { get; set; }



        public void BuildSettings()
        {
            if (ServerContext.Config.Settings == null || ServerContext.Config.Settings.Length == 0)
                return;

            if (Aisling.GameSettings == null || Aisling.GameSettings.Count == 0)
            {
                Aisling.GameSettings = new List<ClientGameSettings>();

                foreach (var settings in ServerContext.Config.Settings)
                    Aisling.GameSettings.Add(new ClientGameSettings(settings.SettingOff, settings.SettingOn,
                        settings.Enabled));
            }
        }

        /// <summary>
        ///     Warps to.
        /// </summary>
        /// <param name="warps">The warps.</param>
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
                        ? string.Format(CultureInfo.CurrentCulture, "You can't enter there just yet. ({0} req)",
                            warps.LevelRequired)
                        : string.Format(CultureInfo.CurrentCulture,
                            "Nightmarish visions of your own death repel you. ({0} Req)", warps.LevelRequired));
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

        /// <summary>
        ///     Learns the spell.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void LearnSpell(Mundane Source, SpellTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Spell.GiveTo(this, subject.Name);
                SendOptionsDialog(Source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint) Aisling.Serial, (uint) Source.Serial,
                        subject?.TargetAnimation ?? 124,
                        subject?.TargetAnimation ?? 124, 100));
            }
        }

        /// <summary>
        ///     Learns the skill.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void LearnSkill(Mundane Source, SkillTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Skill.GiveTo(this, subject.Name);
                SendOptionsDialog(Source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint) Aisling.Serial, (uint) Source.Serial,
                        subject?.TargetAnimation ?? 124,
                        subject?.TargetAnimation ?? 124, 100));
            }
        }

        /// <summary>
        ///     Pays the prerequisites.
        /// </summary>
        /// <param name="prerequisites">The prerequisites.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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

        /// <summary>
        ///     Pays the item prerequisites.
        /// </summary>
        /// <param name="prerequisites">The prerequisites.</param>
        private void PayItemPrerequisites(LearningPredicate prerequisites)
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
        }

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
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
                LeaveArea(true);
                Aisling.XPos = position.X;
                Aisling.YPos = position.Y;
                EnterArea();
            }

            Aisling.Client.CloseDialog();
        }

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
        public void TransitionToMap(int area, Position position)
        {
            if (ServerContext.GlobalMapCache.ContainsKey(area))
            {
                var target = ServerContext.GlobalMapCache[area];
                if (target != null) TransitionToMap(target, position);
            }
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void CloseDialog()
        {
            Send(new byte[] {0x30, 0x00, 0x0A, 0x00});
            MenuInterpter = null;
        }


        /// <summary>
        ///     Updates the specified elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
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
                LastWarp = DateTime.UtcNow;
                Aisling.LastPosition.X = (ushort) Aisling.XPos;
                Aisling.LastPosition.Y = (ushort) Aisling.YPos;

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

        /// <summary>
        ///     Updates the reactors.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        private void UpdateReactors(TimeSpan elapsedTime)
        {
            var Inactive = new List<EphemeralReactor>();

            lock (Aisling.ActiveReactors)
            {
                var reactors = Aisling.ActiveReactors.Select(i => i.Value).ToArray();

                foreach (var reactor in reactors)
                {
                    reactor.Update(elapsedTime);

                    if (reactor.Expired) Inactive.Add(reactor);
                }
            }

            //Remove inactive reactors.
            foreach (var reactor in Inactive)
                if (Aisling.ActiveReactors.ContainsKey(reactor.YamlKey))
                    Aisling.ActiveReactors.Remove(reactor.YamlKey);
        }

        /// <summary>
        ///     Systems the message.
        /// </summary>
        /// <param name="lpmessage">The lpmessage.</param>
        public void SystemMessage(string lpmessage)
        {
            SendMessage(0x02, lpmessage);
        }

        /// <summary>
        ///     Statuses the check.
        /// </summary>
        private void StatusCheck()
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
                    //dirty: one extra pass here, just to ENSURE all buffs are gone.
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

                    return;
                }


                if (!Aisling.Skulled)
                {
                    if (Aisling.CurrentMapId == ServerContext.Config.DeathMap) return;

                    var debuff = new debuff_reeping();
                    {
                        debuff.OnApplied(Aisling, debuff);
                    }
                }
            }
        }

        /// <summary>
        ///     Handles the time outs.
        /// </summary>
        public void HandleTimeOuts()
        {
            if (Aisling.Exchange != null)
                if (Aisling.Exchange.Trader != null)
                    if (!Aisling.Exchange.Trader.LoggedIn
                        || !Aisling.WithinRangeOf(Aisling.Exchange.Trader))
                        Aisling.CancelExchange();

            if (Aisling.PortalSession != null)
            {
                if ((DateTime.UtcNow - Aisling.PortalSession.DateOpened).TotalSeconds > 10)
                {
                    if (LastSelectedNodeIndex > 0)
                    {
                        GameServer.HandleMapNodeSelection(this, LastSelectedNodeIndex);
                        FlushBuffers();
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the status bar.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public void UpdateStatusBar(TimeSpan elapsedTime)
        {
            lock (Aisling)
            {
                Aisling.UpdateBuffs(elapsedTime);
                Aisling.UpdateDebuffs(elapsedTime);
            }
        }

        /// <summary>
        ///     Updates the global scripts.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public void UpdateGlobalScripts(TimeSpan elapsedTime)
        {
            lock (GlobalScripts)
            {
                foreach (var globalscript in GlobalScripts)
                    globalscript?.Update(elapsedTime);
            }
        }

        /// <summary>
        ///     load as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
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
                ).ContinueWith(ct =>
                {
                    SendStats(StatusFlags.All);
                    return true;
                });
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Sets the aisling startup variables.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient SetAislingStartupVariables()
        {
            InMapTransition     = false;
            MapOpen             = false;
            LastSave            = DateTime.UtcNow;
            LastPingResponse    = DateTime.UtcNow;
            PendingItemSessions = null;
            LastLocationSent    = DateTime.UtcNow;

            BoardOpened = DateTime.UtcNow;
            {
                Aisling.BonusAc = (int) (70 - Aisling.Level * 0.5 / 1.0);
                Aisling.Exchange = null;
                Aisling.LastMapId = short.MaxValue;
            }
            BuildSettings();

            return this;
        }

        /// <summary>
        ///     Loads the global scripts.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient LoadGlobalScripts()
        {
            foreach (var script in ServerContext.Config.GlobalScripts)
                GlobalScripts.Add(ScriptManager.Load<GlobalScript>(script, this));

            return this;
        }

        /// <summary>
        ///     Regens the specified elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        private void Regen(TimeSpan elapsedTime)
        {
            if (Aisling.Con > Aisling.ExpLevel + 1)
                HPRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate / 2);

            if (Aisling.Wis > Aisling.ExpLevel + 1)
                MPRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContext.Config.RegenRate / 2);

            HPRegenTimer.Update(elapsedTime);
            MPRegenTimer.Update(elapsedTime);

            if (HPRegenTimer.Elapsed)
            {
                HPRegenTimer.Reset();

                var hpRegenSeed = (Aisling.Con - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var hpRegenAmount = Aisling.MaximumHp * (hpRegenSeed + 0.10);

                hpRegenAmount += hpRegenAmount / 100 * (1 + Aisling.Regen);

                Aisling.CurrentHp = (Aisling.CurrentHp + (int) hpRegenAmount).Clamp(0, Aisling.MaximumHp);
                SendStats(StatusFlags.StructB);
            }

            if (MPRegenTimer.Elapsed)
            {
                MPRegenTimer.Reset();

                var mpRegenSeed = (Aisling.Wis - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var mpRegenAmount = Aisling.MaximumMp * (mpRegenSeed + 0.10);

                mpRegenAmount += mpRegenAmount / 100 * (3 + Aisling.Regen);

                Aisling.CurrentMp = (Aisling.CurrentMp + (int) mpRegenAmount).Clamp(0, Aisling.MaximumMp);
                SendStats(StatusFlags.StructB);
            }
        }

        /// <summary>
        ///     Initializes the spell bar.
        /// </summary>
        /// <returns>GameClient.</returns>
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

        /// <summary>
        ///     Loads the equipment.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient LoadEquipment()
        {
            var formats = new List<NetworkFormat>();

            foreach (var item in Aisling.EquipmentManager.Equipment)
            {
                var equipment = item.Value;

                if (equipment == null || equipment.Item == null || equipment.Item.Template == null)
                    continue;

                if (equipment.Item.Template != null)
                    if (ServerContext.GlobalItemTemplateCache.ContainsKey(equipment.Item.Template.Name))
                    {
                        var template = ServerContext.GlobalItemTemplateCache[equipment.Item.Template.Name];
                        {
                            item.Value.Item.Template = template;

                            if (item.Value.Item.Upgrades > 0)
                                Item.ApplyQuality(item.Value.Item);
                        }
                    }


                equipment.Item.Script =
                    ScriptManager.Load<ItemScript>(equipment.Item.Template.ScriptName, equipment.Item);
                if (!string.IsNullOrEmpty(equipment.Item.Template.WeaponScript))
                    equipment.Item.WeaponScript =
                        ScriptManager.Load<WeaponScript>(equipment.Item.Template.WeaponScript, equipment.Item);

                equipment.Item.Script?.Equipped(Aisling, (byte) equipment.Slot);

                if (equipment.Item.CanCarry(Aisling))
                {
                    //apply weight to items that are equipped.
                    Aisling.CurrentWeight += equipment.Item.Template.CarryWeight;

                    formats.Add(new ServerFormat37(equipment.Item, (byte) equipment.Slot));
                }
                //for some reason, Aisling is out of Weight!
                else
                {
                    //clone and release item
                    var nitem = Clone<Item>(item.Value.Item);
                    nitem.Release(Aisling, Aisling.Position);

                    //display message
                    SendMessage(0x02,
                        string.Format(CultureInfo.CurrentCulture, "{0} is too heavy to hold.", nitem.Template.Name));

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

        public void Ghost()
        {
            Aisling.Flags = AislingFlags.Dead;
            {
                HPRegenTimer.Disabled = true;
                MPRegenTimer.Disabled = true;
            }

            Refresh();
        }

        /// <summary>
        ///     Loads the spell book.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient LoadSpellBook()
        {
            var spells_Available = Aisling.SpellBook.Spells.Values
                .Where(i => i != null && i.Template != null).ToArray();

            for (var i = 0; i < spells_Available.Length; i++)
            {
                var spell = spells_Available[i];

                if (spell.Template != null)
                    if (ServerContext.GlobalSpellTemplateCache.ContainsKey(spell.Template.Name))
                    {
                        var template = ServerContext.GlobalSpellTemplateCache[spell.Template.Name];
                        {
                            spell.Template = template;
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

        /// <summary>
        ///     Loads the skill book.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient LoadSkillBook()
        {
            var skills_Available = Aisling.SkillBook.Skills.Values
                .Where(i => i != null && i.Template != null).ToArray();

            var formats = new List<NetworkFormat>();

            for (var i = 0; i < skills_Available.Length; i++)
            {
                var skill = skills_Available[i];

                if (skill.Template != null)
                    if (ServerContext.GlobalSkillTemplateCache.ContainsKey(skill.Template.Name))
                    {
                        var template = ServerContext.GlobalSkillTemplateCache[skill.Template.Name];
                        {
                            skill.Template = template;
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

        /// <summary>
        ///     Loads the inventory.
        /// </summary>
        /// <returns>GameClient.</returns>
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
                    item.WeaponScript = ScriptManager.Load<WeaponScript>(item.Template.WeaponScript, item);

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
                            SendMessage(0x02,
                                string.Format(CultureInfo.CurrentCulture, "You stumble and drop {0}",
                                    item.Template.Name));
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        ///     Updates the display.
        /// </summary>
        /// <returns>GameClient.</returns>
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
                var nearby = GetObjects<Aisling>(Aisling.Map, i => i.WithinRangeOf(Aisling) && i.CanSeeGhosts());

                if (nearby != null) Aisling.Show(Scope.NearbyAislingsExludingSelf, response, nearby);

                return this;
            }

            Aisling.Show(Scope.NearbyAislingsExludingSelf, response);

            return this;
        }

        /// <summary>
        ///     Refreshes the specified delete.
        /// </summary>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        public void Refresh(bool delete = false)
        {
            LeaveArea(delete);
            EnterArea();
        }

        /// <summary>
        ///     Leaves the area.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        public void LeaveArea(bool update = false, bool delete = false)
        {
            if (Aisling.LastMapId == short.MaxValue)
            {
                Aisling.LastMapId = Aisling.CurrentMapId;
            }

            Aisling.Remove(update, delete);
        }

        /// <summary>
        ///     Enters the area.
        /// </summary>
        public void EnterArea()
        {
            Enter();
        }

        /// <summary>
        ///     Enters this instance.
        /// </summary>
        /// <returns>GameClient.</returns>
        private GameClient Enter()
        {
            SendSerial();
            Insert();
            RefreshMap();
            SendLocation();
            UpdateDisplay();

            return this;
        }

        /// <summary>
        ///     Sends the music.
        /// </summary>
        public void SendMusic()
        {
            Aisling.Client.Send(new byte[]
            {
                0x19, 0x00, 0xFF,
                (byte) Aisling.Map.Music
            });
        }

        /// <summary>
        ///     Sends the sound.
        /// </summary>
        /// <param name="sound">The sound.</param>
        /// <param name="scope">The scope.</param>
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
        ///     Client.Insert: if map is ready (loaded), Inserts an Aisling onto the map in question.
        ///     condition: if it's not present in the object manager.
        ///     true: inserts the object into the object manager, then updates the Map tile location.
        ///     false: does not insert the object into the object manager.
        ///     Note: It will update the map object grid regardless of the above condition.
        /// </summary>
        public void Insert()
        {
            if (!Aisling.Map.Ready)
                return;

            if (GetObject<Aisling>(Aisling.Map, i => i.Serial == Aisling.Serial) == null)
                AddObject(Aisling);

            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling);
        }

        /// <summary>
        ///     Refreshes the map.
        /// </summary>
        public void RefreshMap()
        {
            ShouldUpdateMap = false;

            if (Aisling.CurrentMapId != Aisling.LastMapId)
            {
                ShouldUpdateMap = true;
                Aisling.LastMapId = Aisling.CurrentMapId;

                if (!Aisling.DiscoveredMaps.Any(i => i == Aisling.CurrentMapId))
                    Aisling.DiscoveredMaps.Add(Aisling.CurrentMapId);

                SendMusic();
            }

            if (ShouldUpdateMap)
            {
                MapUpdating = true;
                Aisling.Client.LastMapUpdated = DateTime.UtcNow;
                Aisling.View.Clear();
                Send(new ServerFormat15(Aisling.Map));
            }
            else
            {
                Aisling.View.Clear();
            }

        }


        /// <summary>
        ///     Sends the serial.
        /// </summary>
        private void SendSerial()
        {
            Send(new ServerFormat05(Aisling));
        }

        /// <summary>
        ///     Sends the location.
        /// </summary>
        public  void SendLocation()
        {
            CloseDialog();
            {
                Send(new ServerFormat04(Aisling));
                LastLocationSent = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Saves this instance.
        /// </summary>
        public void Save()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                StorageManager.AislingBucket.Save(Aisling);
                {
                    LastSave = DateTime.UtcNow;
                }
            });
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        public void SendMessage(byte type, string text)
        {
            Send(new ServerFormat0A(type, text));
            LastMessageSent = DateTime.UtcNow;
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
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

        /// <summary>
        ///     Says the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
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

        /// <summary>
        ///     Sends the animation.
        /// </summary>
        /// <param name="Animation">The animation.</param>
        /// <param name="To">To.</param>
        /// <param name="From">From.</param>
        /// <param name="speed">The speed.</param>
        public void SendAnimation(ushort Animation, Sprite To, Sprite From, byte speed = 100)
        {
            var format = new ServerFormat29((uint) From.Serial, (uint) To.Serial, Animation, 0, speed);
            Aisling.Show(Scope.NearbyAislings, format);
        }

        /// <summary>
        ///     Sends the item shop dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="items">The items.</param>
        public void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemShopData(step, items)));
        }

        /// <summary>
        ///     Sends the item sell dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="items">The items.</param>
        public void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items)
        {
            Send(new ServerFormat2F(mundane, text, new ItemSellData(step, items)));
        }

        /// <summary>
        ///     Sends the options dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        public void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsData(options)));
        }

        /// <summary>
        ///     Sends the popup dialog.
        /// </summary>
        /// <param name="popup">The popup.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        public void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options)
        {
            Send(new PopupFormat(popup, text, new OptionsData(options)));
        }

        /// <summary>
        ///     Sends the options dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="options">The options.</param>
        public void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options)
        {
            Send(new ServerFormat2F(mundane, text, new OptionsPlusArgsData(options, args)));
        }

        /// <summary>
        ///     Sends the skill learn dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="skills">The skills.</param>
        public void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills)
        {
            Send(new ServerFormat2F(mundane, text, new SkillAcquireData(step, skills)));
        }

        /// <summary>
        ///     Sends the spell learn dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="spells">The spells.</param>
        public void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells)
        {
            Send(new ServerFormat2F(mundane, text, new SpellAcquireData(step, spells)));
        }

        /// <summary>
        ///     Sends the skill forget dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        public void SendSkillForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SkillForfeitData(step)));
        }

        /// <summary>
        ///     Sends the spell forget dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        public void SendSpellForgetDialog(Mundane mundane, string text, ushort step)
        {
            Send(new ServerFormat2F(mundane, text, new SpellForfeitData(step)));
        }

        /// <summary>
        ///     Sends the stats.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void SendStats(StatusFlags flags)
        {
            Send(new ServerFormat08(Aisling, flags));
        }

        /// <summary>
        ///     Sends the profile update.
        /// </summary>
        public void SendProfileUpdate()
        {
            Send(new byte[] {73, 0x00});
        }

        /// <summary>
        ///     Trains the spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
        public void TrainSpell(Spell spell)
        {
            if (spell.Level < spell.Template.MaxLevel)
            {
                var toImprove = (int) (0.10 / spell.Template.LevelRate);
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

        /// <summary>
        ///     Trains the skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        public void TrainSkill(Skill skill)
        {
            if (skill.Level < skill.Template.MaxLevel)
            {
                var toImprove = (int) (0.10 / skill.Template.LevelRate);
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

            Send(new ServerFormat3F((byte) skill.Template.Pane,
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

        /// <summary>
        ///     Warps to.
        /// </summary>
        /// <param name="position">The position.</param>
        public void WarpTo(Position position)
        {
            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling, true);

            Aisling.XPos = position.X;
            Aisling.YPos = position.Y;

            Refresh();

            Aisling.Map.Update(Aisling.XPos, Aisling.YPos, Aisling);
        }

        /// <summary>
        ///     Repairs the equipment.
        /// </summary>
        /// <param name="gear">The gear.</param>
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


        /// <summary>
        ///     Revives this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Revive()
        {
            Aisling.Flags = AislingFlags.Normal;
            HPRegenTimer.Disabled = false;
            MPRegenTimer.Disabled = false;

            Aisling.Recover();
            return Aisling.CurrentHp > 0;
        }

        /// <summary>
        ///     Checks the reqs.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CheckReqs(GameClient client, Item item)
        {
            var message = string.Empty;

            if (client.Aisling.ExpLevel < item.Template.LevelRequired)
            {
                message = ServerContext.Config.CantWearYetMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (item.Durability <= 0)
            {
                message = ServerContext.Config.RepairItemMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (client.Aisling.Path != item.Template.Class && !(item.Template.Class == Class.Peasant))
            {
                if (client.Aisling.ExpLevel >= item.Template.LevelRequired)
                    message = ServerContext.Config.WrongClassMessage;
                else
                    message = ServerContext.Config.CantWearYetMessage;
            }

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
                        client.SendMessage(0x02, ServerContext.Config.DoesNotFitMessage);
                        return false;
                    }
                }

                return true;
            }

            client.SendMessage(0x02, ServerContext.Config.CantEquipThatMessage);
            return false;
        }

        /// <summary>
        ///     Shows the current menu.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="currentitem">The currentitem.</param>
        /// <param name="nextitem">The nextitem.</param>
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
                    DisplayImage = (ushort) (obj as Mundane).Template.Image,
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

                    options.Add(new OptionsDataItem((short) ans.Id, ans.Text));
                    ServerContext.Logger.Debug($"{ans.Id}. {ans.Text}");
                }

                SendOptionsDialog(obj as Mundane, nextitem.Text, options.ToArray());
            }
        }

        /// <summary>
        ///     Shows the current menu.
        /// </summary>
        /// <param name="popup">The popup.</param>
        /// <param name="currentitem">The currentitem.</param>
        /// <param name="nextitem">The nextitem.</param>
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

                    options.Add(new OptionsDataItem((short) ans.Id, ans.Text));
                    ServerContext.Logger.Debug($"{ans.Id}. {ans.Text}");
                }


                SendPopupDialog(popup, nextitem.Text, options.ToArray());
            }
        }

        #region IDisposable Implementation

        protected bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                // Do nothing if the object has already been disposed of.
                if (disposed)
                    return;

                if (disposing)
                    if (_server != null)
                        _server.Dispose();
                disposed = true;
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            // Unregister object for finalization.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}