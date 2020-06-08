// ************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using Darkages.Common;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using MenuInterpreter;
using Newtonsoft.Json;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Darkages.Network.Game
{
    /// <summary>
    ///     Class GameClient.
    ///     Implements the <see cref="GameClient" />
    /// </summary>
    /// <seealso cref="GameClient" />
    public partial class GameClient : NetworkClient<GameClient>, IDisposable
    {
        private readonly object _syncObj = new object();

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
        ///     The last walk detected
        /// </summary>
        private DateTime _lastmovement;

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
            HpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContextBase.GlobalConfig.RegenRate));

            MpRegenTimer = new GameServerTimer(
                TimeSpan.FromMilliseconds(ServerContextBase.GlobalConfig.RegenRate));

            PropertyChanged += GameClient_PropertyChanged;
        }

        public void GameClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedNodeIndex"))
            {
                var node = SelectedNodeIndex;

                if (node > 0)
                {
                    LastSelectedNodeIndex = node;

                    LastWarp = DateTime.UtcNow;
                    ShouldUpdateMap = true;

                    GameServer.HandleMapNodeSelection(this, node);
                }
            }
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
        public GameServerTimer HpRegenTimer
        {
            get => _hpRegenTimer;
            set => _hpRegenTimer = value;
        }

        /// <summary>
        ///     Gets or sets the mp regen timer.
        /// </summary>
        /// <value>The mp regen timer.</value>
        public GameServerTimer MpRegenTimer
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

        public DateTime LastMovement
        {
            get => _lastmovement;
            set => _lastmovement = value;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value><c>true</c> if this instance is refreshing; otherwise, <c>false</c>.</value>
        public bool IsRefreshing =>
            DateTime.UtcNow - LastClientRefresh < new TimeSpan(0, 0, 0, 0, ServerContextBase.GlobalConfig.RefreshRate);


        /// <summary>
        ///     Gets a value indicating whether this instance is warping.
        /// </summary>
        /// <value><c>true</c> if this instance is warping; otherwise, <c>false</c>.</value>
        public bool IsWarping =>
            DateTime.UtcNow - LastWarp < new TimeSpan(0, 0, 0, 0, ServerContextBase.GlobalConfig.WarpCheckRate);

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

        public TimeSpan LastMenuStarted { get; private set; }

        public void BuildSettings()
        {
            if (ServerContextBase.GlobalConfig.Settings == null || ServerContextBase.GlobalConfig.Settings.Length == 0)
                return;

            if (Aisling.GameSettings == null || Aisling.GameSettings.Count == 0)
            {
                Aisling.GameSettings = new List<ClientGameSettings>();

                foreach (var settings in ServerContextBase.GlobalConfig.Settings)
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

            if (ServerContextBase.GlobalMapCache.Values.Any(i => i.ID == warps.ActivationMapId))
            {
                if (warps.LevelRequired > 0 && Aisling.ExpLevel < warps.LevelRequired)
                {
                    var msgTier = Math.Abs(Aisling.ExpLevel - warps.LevelRequired);

                    SendMessage(0x02, msgTier <= 10
                        ? string.Format(CultureInfo.CurrentCulture, "You can't enter there just yet. ({0} req)",
                            warps.LevelRequired)
                        : string.Format(CultureInfo.CurrentCulture,
                            "Nightmarish visions of your own death repel you. ({0} Req)", warps.LevelRequired));

                    //TODO: move player somewhere near the warp. but not ontop of it.
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
        /// <param name="source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public GameClient LearnSpell(Mundane source, SpellTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Spell.GiveTo(this, subject.Name);
                SendOptionsDialog(source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint) Aisling.Serial, (uint) source.Serial,
                        subject?.TargetAnimation ?? 124,
                        subject?.TargetAnimation ?? 124, 100));
            }

            return this;
        }

        /// <summary>
        ///     Learns the skill.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public GameClient LearnSkill(Mundane source, SkillTemplate subject, string message)
        {
            if (PayPrerequisites(subject.Prerequisites))
            {
                Skill.GiveTo(this, subject.Name);
                SendOptionsDialog(source, message);

                Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint) Aisling.Serial, (uint) source.Serial,
                        subject?.TargetAnimation ?? 124,
                        subject?.TargetAnimation ?? 124, 100));
            }

            return this;
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

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
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

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
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

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public GameClient CloseDialog()
        {
            Send(new byte[] {0x30, 0x00, 0x0A, 0x00});
            MenuInterpter = null;

            return this;
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

            if ((DateTime.UtcNow - Aisling.LastLogged).TotalMilliseconds < ServerContextBase.GlobalConfig.LingerState)
                return;

            #endregion

            var distance = Aisling.Position.DistanceFrom(Aisling.LastPosition.X, Aisling.LastPosition.Y);

            if (distance > 2 && !MapOpen && !IsWarping && (DateTime.UtcNow - LastMapUpdated).TotalMilliseconds > 2000)
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

                if (nextTrap != null)
                {
                    Trap.Activate(nextTrap, Aisling);
                }
            }

            DoUpdate(elapsedTime);
        }

        public GameClient DoUpdate(TimeSpan elapsedTime)
        {
            return HandleTimeOuts()
                .StatusCheck()
                .Regen(elapsedTime)
                .UpdateStatusBar(elapsedTime)
                .UpdateReactors(elapsedTime);
        }

        /// <summary>
        ///     Updates the reactors.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
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

            //Remove inactive reactors.
            foreach (var reactor in inactive)
                if (Aisling.ActiveReactors.ContainsKey(reactor.YamlKey))
                    Aisling.ActiveReactors.Remove(reactor.YamlKey);

            return this;
        }

        /// <summary>
        ///     Systems the message.
        /// </summary>
        /// <param name="lpmessage">The lpmessage.</param>
        public GameClient SystemMessage(string lpmessage)
        {
            SendMessage(0x02, lpmessage);
            return this;
        }

        /// <summary>
        ///     Statuses the check.
        /// </summary>
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

                    return this;
                }


                if (!Aisling.Skulled)
                {
                    if (Aisling.CurrentMapId == ServerContextBase.GlobalConfig.DeathMap)
                        return this;

                    var debuff = new debuff_reeping();
                    {
                        debuff.OnApplied(Aisling, debuff);
                    }
                }
            }

            return this;
        }

        /// <summary>
        ///     Handles the time outs.
        /// </summary>
        public GameClient HandleTimeOuts()
        {
            if (Aisling.Exchange != null)
                if (Aisling.Exchange.Trader != null)
                    if (!Aisling.Exchange.Trader.LoggedIn
                        || !Aisling.WithinRangeOf(Aisling.Exchange.Trader))
                        Aisling.CancelExchange();

            if (Aisling.PortalSession != null)
                if ((DateTime.UtcNow - Aisling.PortalSession.DateOpened).TotalSeconds > 10)
                        GameServer.HandleMapNodeSelection(this, LastSelectedNodeIndex);

            return this;
        }

        /// <summary>
        ///     Updates the status bar.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public GameClient UpdateStatusBar(TimeSpan elapsedTime)
        {
            lock (_syncObj)
            {
                Aisling.UpdateBuffs(elapsedTime);
                Aisling.UpdateDebuffs(elapsedTime);
            }

            return this;
        }


        /// <summary>
        ///     load as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public GameClient Load()
        {
            if (Aisling == null || Aisling.AreaID == 0)
                return null;
            if (!ServerContextBase.GlobalMapCache.ContainsKey(Aisling.AreaID))
                return null;

            SetAislingStartupVariables();

            lock (_syncObj)
            {
                try
                {
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
                    ServerContextBase.Report(e);
                }
            }

            return null;
        }

        /// <summary>
        ///     Sets the aisling startup variables.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient SetAislingStartupVariables()
        {
            InMapTransition = false;
            MapOpen = false;
            LastSave = DateTime.UtcNow;
            LastPingResponse = DateTime.UtcNow;
            PendingItemSessions = null;
            LastLocationSent = DateTime.UtcNow;

            BoardOpened = DateTime.UtcNow;
            {
                Aisling.BonusAc = (int) (70 - Aisling.Level * 0.5 / 1.0);
                Aisling.Exchange = null;
                Aisling.LastMapId = short.MaxValue;
            }
            BuildSettings();
            ServerContextBase.Report($"[GameSettings Loaded by {Aisling.Username} ({Aisling.GameSettings.Dump()})]");
            return this;
        }

        /// <summary>
        ///     Regens the specified elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public GameClient Regen(TimeSpan elapsedTime)
        {
            if (Aisling.Con > Aisling.ExpLevel + 1)
                HpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContextBase.GlobalConfig.RegenRate / 2);

            if (Aisling.Wis > Aisling.ExpLevel + 1)
                MpRegenTimer.Delay = TimeSpan.FromMilliseconds(ServerContextBase.GlobalConfig.RegenRate / 2);

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

                Aisling.CurrentHp = (Aisling.CurrentHp + (int) hpRegenAmount).Clamp(0, Aisling.MaximumHp);
                SendStats(StatusFlags.StructB);
            }

            if (MpRegenTimer.Elapsed && !MpRegenTimer.Disabled)
            {
                MpRegenTimer.Reset();

                var mpRegenSeed = (Aisling.Wis - Aisling.ExpLevel).Clamp(0, 10) * 0.01;
                var mpRegenAmount = Aisling.MaximumMp * (mpRegenSeed + 0.10);

                mpRegenAmount += mpRegenAmount / 100 * (3 + Aisling.Regen);

                Aisling.CurrentMp = (Aisling.CurrentMp + (int) mpRegenAmount).Clamp(0, Aisling.MaximumMp);
                SendStats(StatusFlags.StructB);
            }

            return this;
        }

        /// <summary>
        ///     Initializes the spell bar.
        /// </summary>
        /// <returns>GameClient.</returns>
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

                ServerContextBase.Report($"[SpellBar [Buffs] Loaded by {Aisling.Username} ({Aisling.Buffs.Dump()})]");
                ServerContextBase.Report(
                    $"[SpellBar [Debuffs] Loaded by {Aisling.Username} ({Aisling.Debuffs.Dump()})]");
            }

            return this;
        }

        /// <summary>
        ///     Loads the equipment.
        /// </summary>
        /// <returns>GameClient.</returns>
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
                        script.Equipped(Aisling, (byte) equipment.Slot);

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


            ServerContextBase.Report($"[Equipment Loaded by {Aisling.Username} ({Aisling.EquipmentManager.Dump()})]");
            return this;
        }

        public GameClient AislingToGhostForm()
        {
            Aisling.Flags = AislingFlags.Dead;
            {
                HpRegenTimer.Disabled = true;
                MpRegenTimer.Disabled = true;
            }

            ServerContextBase.Report($"[Ghost Called by {Aisling.Username} ({Aisling.Flags.Dump()})]");
            Refresh(true);

            return this;
        }

        public GameClient GhostFormToAisling()
        {
            Aisling.Flags = AislingFlags.Normal;
            {
                HpRegenTimer.Disabled = false;
                MpRegenTimer.Disabled = false;
            }

            ServerContextBase.Report(
                $"[Back to Normal -> From Ghost Called by {Aisling.Username} ({Aisling.Flags.Dump()})]");
            Refresh(true);

            return this;
        }

        /// <summary>
        ///     Loads the spell book.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient LoadSpellBook()
        {
            lock (_syncObj)
            {
                var spellsAvailable = Aisling.SpellBook.Spells.Values
                    .Where(i => i != null && i.Template != null).ToArray();

                for (var i = 0; i < spellsAvailable.Length; i++)
                {
                    var spell = spellsAvailable[i];

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
                        Task.Delay(1000).ContinueWith((ct) =>
                        {
                            var delta = (int) Math.Abs((DateTime.UtcNow - spell.NextAvailableUse).TotalSeconds);
                            var offset = (int) Math.Abs(spell.Template.Cooldown - delta);

                            if (delta <= spell.Template.Cooldown)
                                Send(new ServerFormat3F((byte) 0,
                                    spell.Slot,
                                    delta));
                        });
                    else
                        spell.NextAvailableUse = DateTime.UtcNow;
                }
            }

            ServerContextBase.Report($"[Spellbook Loaded: ({Aisling.SpellBook.Dump()})]");
            return this;
        }

        /// <summary>
        ///     Loads the skill book.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient LoadSkillBook()
        {
            lock (_syncObj)
            {
                var skillsAvailable = Aisling.SkillBook.Skills.Values
                    .Where(i => i != null && i.Template != null).ToArray();

                var formats = new List<NetworkFormat>();

                for (var i = 0; i < skillsAvailable.Length; i++)
                {
                    var skill = skillsAvailable[i];

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

                    ServerContextBase.Report($"[Skillbook Loaded: ({Aisling.SkillBook.Dump()})]");
                }
            }

            return this;
        }

        /// <summary>
        ///     Loads the inventory.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient LoadInventory()
        {
            lock (_syncObj)
            {
                var itemsAvailable = Aisling.Inventory.Items.Values
                    .Where(i => i != null && i.Template != null).ToArray();

                for (var i = 0; i < itemsAvailable.Length; i++)
                {
                    var item = itemsAvailable[i];

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
            }

            ServerContextBase.Report($"[Inventory Loaded: ({Aisling.Inventory.Dump()})]");
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
                        {
                            otherplayer.ShowTo(myplayer);
                        }
                        else
                        {
                            myplayer.ShowTo(otherplayer);
                        }

                        if (otherplayer.Invisible)
                        {
                            myplayer.ShowTo(otherplayer);
                        }
                        else
                        {
                            otherplayer.ShowTo(myplayer);
                        }
                    }
                    else
                    {
                        if (myplayer.Dead)
                        {
                            if (otherplayer.CanSeeGhosts())
                            {
                                myplayer.ShowTo(otherplayer);
                            }
                        }

                        if (otherplayer.Dead)
                        {
                            if (myplayer.CanSeeGhosts())
                            {
                                otherplayer.ShowTo(myplayer);
                            }
                        }
                    }
                }
            }


            return this;
        }

        /// <summary>
        ///     Refreshes the specified delete.
        /// </summary>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        public GameClient Refresh(bool delete = false)
        {
            LeaveArea(delete);
            EnterArea();

            return this;
        }

        public GameClient LeaveArea(bool update = false, bool delete = false)
        {
            if (Aisling.LastMapId == short.MaxValue)
            {
                Aisling.LastMapId = Aisling.CurrentMapId;
            }

            Aisling.Remove(update, delete);

            return this;
        }

        /// <summary>
        ///     Enters the area.
        /// </summary>
        public GameClient EnterArea()
        {
            return Enter();
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
            UpdateDisplay();
            SendLocation();

            ServerContextBase.Report($"[Player entered lorule. {Aisling.Username} ({Aisling.Dump()})]");
            return this;
        }

        /// <summary>
        ///     Sends the music.
        /// </summary>
        public GameClient SendMusic()
        {
            Aisling.Client.Send(new byte[]
            {
                0x19, 0x00, 0xFF,
                (byte) Aisling.Map.Music
            });

            ServerContextBase.Report($"[Music Sent to player {Aisling.Username} {Aisling.Map.Music}]");
            return this;
        }

        /// <summary>
        ///     Sends the sound.
        /// </summary>
        /// <param name="sound">The sound.</param>
        /// <param name="scope">The scope.</param>
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

        public GameClient Insert()
        {
            AddObject(Aisling);
            return this;
        }

        /// <summary>
        ///     Refreshes the map.
        /// </summary>
        public GameClient RefreshMap()
        {
            MapOpen = false;
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

                if (Aisling.Blind == 1)
                {
                    Aisling.Map.Flags |= MapFlags.Darkness;
                }

                Send(new ServerFormat15(Aisling.Map));
            }
            else
            {
                Aisling.View.Clear();
            }

            return this;
        }


        /// <summary>
        ///     Sends the serial.
        /// </summary>
        public GameClient SendSerial()
        {
            Send(new ServerFormat05(Aisling));

            ServerContextBase.Report($"[Player Serial Assigned to {Aisling.Username} Serial: {Aisling.Serial}]");

            return this;
        }

        /// <summary>
        ///     Sends the location.
        /// </summary>
        public GameClient SendLocation()
        {
            Send(new ServerFormat04(Aisling));
            LastLocationSent = DateTime.UtcNow;
            MapOpen = false;



            return CloseDialog();
        }

        /// <summary>
        ///     Saves this instance.
        /// </summary>
        public GameClient Save()
        {
            StorageManager.AislingBucket.Save(Aisling);
            LastSave = DateTime.UtcNow;

            ServerContextBase.Report($"[Player data saved. {Aisling.Username}]");

            return this;
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        public GameClient SendMessage(byte type, string text)
        {
            FlushAndSend(new ServerFormat0A(type, text));
            LastMessageSent = DateTime.UtcNow;


            ServerContextBase.Report($"[Player message: {Aisling.Username} ({text} type {type})]");

            return this;
        }

        public GameClient SendMessage(string text)
        {
            FlushAndSend(new ServerFormat0A(0x02, text));
            LastMessageSent = DateTime.UtcNow;

            ServerContextBase.Report($"[Player message: {Aisling.Username} ({text} type {0x02})]");

            return this;
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

            ServerContextBase.Report($"[Player system message: {Aisling.Username} ({text})]");
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
        /// <param name="animation">The animation.</param>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="speed">The speed.</param>
        public void SendAnimation(ushort animation, Sprite to, Sprite @from, byte speed = 100)
        {
            var format = new ServerFormat29((uint) @from.Serial, (uint) to.Serial, animation, 0, speed);
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

            ServerContextBase.Report(
                $"[Popup Sent to Player {Aisling.Username} ({options.Dump()}) From ({mundane.Dump()})]");
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

            ServerContextBase.Report(
                $"[Popup Sent to Player {Aisling.Username} ({options.Dump()}) From ({popup.Dump()})]");
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
            ServerContextBase.Report(
                $"[Options Sent to Player {Aisling.Username} ({options.Dump()}) From ({mundane.Dump()})]");
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

            ServerContextBase.Report($"[Dialog Sent to Player {Aisling.Username} ({text})]");
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

            ServerContextBase.Report($"[Dialog Sent to Player {Aisling.Username} ({text})]");
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

            ServerContextBase.Report($"[Dialog Sent to Player {Aisling.Username} ({text})]");
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

            ServerContextBase.Report($"[Dialog Sent to Player {Aisling.Username} ({text})]");
        }

        /// <summary>
        ///     Sends the stats.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public GameClient SendStats(StatusFlags flags)
        {
            Send(new ServerFormat08(Aisling, flags));

            return this;
        }

        /// <summary>
        ///     Sends the profile update.
        /// </summary>
        public GameClient SendProfileUpdate()
        {
            Send(new byte[] {73, 0x00});
            ServerContextBase.Report($"[Profile Update Sent by {Aisling.Username} ({Aisling.ProfileMessage.Dump()})]");

            return this;
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

                    ServerContextBase.Report($"[Spell improved by Player {Aisling.Username} ({spell.Dump()})]");
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
            Aisling.XPos = position.X;
            Aisling.YPos = position.Y;

            Refresh();
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
            HpRegenTimer.Disabled = false;
            MpRegenTimer.Disabled = false;

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
                message = ServerContextBase.GlobalConfig.CantWearYetMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (item.Durability <= 0)
            {
                message = ServerContextBase.GlobalConfig.RepairItemMessage;
                if (!(message != null && string.IsNullOrEmpty(message)))
                {
                    client.SendMessage(0x02, message);
                    return false;
                }
            }

            if (client.Aisling.Path != item.Template.Class && !(item.Template.Class == Class.Peasant))
            {
                if (client.Aisling.ExpLevel >= item.Template.LevelRequired)
                    message = ServerContextBase.GlobalConfig.WrongClassMessage;
                else
                    message = ServerContextBase.GlobalConfig.CantWearYetMessage;
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
                        client.SendMessage(0x02, ServerContextBase.GlobalConfig.DoesNotFitMessage);
                        return false;
                    }
                }

                return true;
            }

            client.SendMessage(0x02, ServerContextBase.GlobalConfig.CantEquipThatMessage);
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
                }


                SendPopupDialog(popup, nextitem.Text, options.ToArray());
            }
        }

        #region IDisposable Implementation

        protected bool Disposed;

        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                // Do nothing if the object has already been disposed of.
                if (Disposed)
                    return;

                if (disposing)
                    if (_server != null)
                        _server.Dispose();
                Disposed = true;
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