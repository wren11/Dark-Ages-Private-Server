using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using MenuInterpreter;

namespace Darkages.Network.Game
{
    public interface IGameClient
    {
        /// <summary>
        /// Ports the specified i.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// This Chat command ports the user to a location.
        /// Example chat command: 'port -i:100 -x:5 -y:5'
        void Port(int i, int x = 0, int y = 0);

        /// <summary>
        ///     This chat command spawns a monster.
        /// </summary>
        /// <param name="t">Name of Monster, Case Sensitive.</param>
        /// <param name="x">X Location to Spawn.</param>
        /// <param name="y">Y Location to Spawn.</param>
        /// <param name="c">The c.</param>
        /// <usage>spawnMonster -t:Undead -x:43 -y:16 -c:10</usage>
        void Spawn(string t, int x, int y, int c);

        void LearnEverything();
        void ForgetSkill(string s);
        void ForgetSpell(string s);
        void GiveExp(int a);
        void Recover();
        void GiveStr(byte v = 1);
        void GiveInt(byte v = 1);
        void GiveWis(byte v = 1);
        void GiveCon(byte v = 1);
        void GiveDex(byte v = 1);
        void GiveHp(int v = 1);
        void GiveMp(int v = 1);
        Task Effect(ushort n, int d = 1000, int r = 1);
        void StressTest();
        void GiveScar();
        void RevivePlayer(string u);
        void KillPlayer(string u);
        bool GiveItem(string itemName);
        bool GiveTutorialArmor();
        bool CastSpell(string spellName, Sprite caster, Sprite target);
        bool PlayerUseSpell(string spellname, Sprite target);
        bool PlayerUseSkill(string spellname);
        bool TakeAwayItem(string item);
        bool TakeAwayItem(Item item);
        void OpenBoard(string n);
        void ReloadObjects(bool all = false);
        GameClient LoggedIn(bool state);
        void GameClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e);

        /// <summary>
        ///     Gets or sets the server.
        /// </summary>
        /// <value>The server.</value>
        GameServer Server { get; set; }

        /// <summary>
        ///     Gets or sets the aisling.
        /// </summary>
        /// <value>The aisling.</value>
        Aisling Aisling { get; set; }

        /// <summary>
        ///     Gets or sets the hp regen timer.
        /// </summary>
        /// <value>The hp regen timer.</value>
        GameServerTimer HpRegenTimer { get; set; }

        /// <summary>
        ///     Gets or sets the mp regen timer.
        /// </summary>
        /// <value>The mp regen timer.</value>
        GameServerTimer MpRegenTimer { get; set; }

        /// <summary>
        ///     Gets or sets the menu interpter.
        /// </summary>
        /// <value>The menu interpter.</value>
        Interpreter MenuInterpter { get; set; }

        /// <summary>
        ///     Gets or sets the dialog session.
        /// </summary>
        /// <value>The dialog session.</value>
        DialogSession DlgSession { get; set; }

        /// <summary>
        ///     Gets or sets the last item dropped.
        /// </summary>
        /// <value>The last item dropped.</value>
        Item LastItemDropped { get; set; }

        /// <summary>
        ///     Gets or sets the board opened.
        /// </summary>
        /// <value>The board opened.</value>
        DateTime BoardOpened { get; set; }

        /// <summary>
        ///     Gets or sets the last whisper message sent.
        /// </summary>
        /// <value>The last whisper message sent.</value>
        DateTime LastWhisperMessageSent { get; set; }

        /// <summary>
        ///     Gets or sets the last assail.
        /// </summary>
        /// <value>The last assail.</value>
        DateTime LastAssail { get; set; }

        /// <summary>
        ///     Gets or sets the last message sent.
        /// </summary>
        /// <value>The last message sent.</value>
        DateTime LastMessageSent { get; set; }

        /// <summary>
        ///     Gets or sets the last ping response.
        /// </summary>
        /// <value>The last ping response.</value>
        DateTime LastPingResponse { get; set; }

        /// <summary>
        ///     Gets or sets the last warp.
        /// </summary>
        /// <value>The last warp.</value>
        DateTime LastWarp { get; set; }

        /// <summary>
        ///     Gets or sets the last script executed.
        /// </summary>
        /// <value>The last script executed.</value>
        DateTime LastScriptExecuted { get; set; }

        /// <summary>
        ///     Gets or sets the last ping.
        /// </summary>
        /// <value>The last ping.</value>
        DateTime LastPing { get; set; }

        /// <summary>
        ///     Gets or sets the last save.
        /// </summary>
        /// <value>The last save.</value>
        DateTime LastSave { get; set; }

        /// <summary>
        ///     Gets or sets the last client refresh.
        /// </summary>
        /// <value>The last client refresh.</value>
        DateTime LastClientRefresh { get; set; }

        DateTime LastMovement { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value><c>true</c> if this instance is refreshing; otherwise, <c>false</c>.</value>
        bool IsRefreshing { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is warping.
        /// </summary>
        /// <value><c>true</c> if this instance is warping; otherwise, <c>false</c>.</value>
        bool IsWarping { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance can send location.
        /// </summary>
        /// <value><c>true</c> if this instance can send location; otherwise, <c>false</c>.</value>
        bool CanSendLocation { get; }

        bool WasUpdatingMapRecently { get; }

        /// <summary>
        ///     Gets or sets the last location sent.
        /// </summary>
        /// <value>The last location sent.</value>
        DateTime LastLocationSent { get; set; }

        DateTime LastMapUpdated { get; set; }

        /// <summary>
        ///     Gets or sets the last board activated.
        /// </summary>
        /// <value>The last board activated.</value>
        ushort LastBoardActivated { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [should update map].
        /// </summary>
        /// <value><c>true</c> if [should update map]; otherwise, <c>false</c>.</value>
        bool ShouldUpdateMap { get; set; }

        /// <summary>
        ///     Gets or sets the last activated lost.
        /// </summary>
        /// <value>The last activated lost.</value>
        byte LastActivatedLost { get; set; }

        /// <summary>
        ///     Gets or sets the pending item sessions.
        /// </summary>
        /// <value>The pending item sessions.</value>
        PendingSell PendingItemSessions { get; set; }

        TimeSpan LastMenuStarted { get; }
        void BuildSettings();

        /// <summary>
        ///     Warps to.
        /// </summary>
        /// <param name="warps">The warps.</param>
        void WarpTo(WarpTemplate warps);

        /// <summary>
        ///     Learns the spell.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        GameClient LearnSpell(Mundane source, SpellTemplate subject, string message);

        /// <summary>
        ///     Learns the skill.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        GameClient LearnSkill(Mundane source, SkillTemplate subject, string message);

        /// <summary>
        ///     Pays the prerequisites.
        /// </summary>
        /// <param name="prerequisites">The prerequisites.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool PayPrerequisites(LearningPredicate prerequisites);

        /// <summary>
        ///     Pays the item prerequisites.
        /// </summary>
        /// <param name="prerequisites">The prerequisites.</param>
        GameClient PayItemPrerequisites(LearningPredicate prerequisites);

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
        GameClient TransitionToMap(Area area, Position position);

        /// <summary>
        ///     Transitions to map.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="position">The position.</param>
        GameClient TransitionToMap(int area, Position position);

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        GameClient CloseDialog();

        /// <summary>
        ///     Updates the specified elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        void Update(TimeSpan elapsedTime);

        GameClient DoUpdate(TimeSpan elapsedTime);

        /// <summary>
        ///     Updates the reactors.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        GameClient UpdateReactors(TimeSpan elapsedTime);

        /// <summary>
        ///     Systems the message.
        /// </summary>
        /// <param name="lpmessage">The lpmessage.</param>
        GameClient SystemMessage(string lpmessage);

        /// <summary>
        ///     Statuses the check.
        /// </summary>
        GameClient StatusCheck();

        /// <summary>
        ///     Handles the time outs.
        /// </summary>
        GameClient HandleTimeOuts();

        /// <summary>
        ///     Updates the status bar.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        GameClient UpdateStatusBar(TimeSpan elapsedTime);

        /// <summary>
        ///     load as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        GameClient Load();

        /// <summary>
        ///     Sets the aisling startup variables.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient SetAislingStartupVariables();

        /// <summary>
        ///     Regens the specified elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        GameClient Regen(TimeSpan elapsedTime);

        /// <summary>
        ///     Initializes the spell bar.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient InitSpellBar();

        /// <summary>
        ///     Loads the equipment.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient LoadEquipment();

        GameClient AislingToGhostForm();
        GameClient GhostFormToAisling();

        /// <summary>
        ///     Loads the spell book.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient LoadSpellBook();

        /// <summary>
        ///     Loads the skill book.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient LoadSkillBook();

        /// <summary>
        ///     Loads the inventory.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient LoadInventory();

        /// <summary>
        ///     Updates the display.
        /// </summary>
        /// <returns>GameClient.</returns>
        GameClient UpdateDisplay();

        /// <summary>
        ///     Refreshes the specified delete.
        /// </summary>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        GameClient Refresh(bool delete = false);

        /// <summary>
        ///     Leaves the area.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        GameClient LeaveArea(bool update = false, bool delete = false);

        /// <summary>
        ///     Enters the area.
        /// </summary>
        GameClient EnterArea();

        /// <summary>
        ///     Sends the music.
        /// </summary>
        GameClient SendMusic();

        /// <summary>
        ///     Sends the sound.
        /// </summary>
        /// <param name="sound">The sound.</param>
        /// <param name="scope">The scope.</param>
        GameClient SendSound(byte sound, Scope scope = Scope.Self);

        GameClient Insert();

        /// <summary>
        ///     Refreshes the map.
        /// </summary>
        GameClient RefreshMap();

        /// <summary>
        ///     Sends the serial.
        /// </summary>
        GameClient SendSerial();

        /// <summary>
        ///     Sends the location.
        /// </summary>
        GameClient SendLocation();

        /// <summary>
        ///     Saves this instance.
        /// </summary>
        GameClient Save();

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        GameClient SendMessage(byte type, string text);

        GameClient SendMessage(string text);

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        void SendMessage(Scope scope, byte type, string text);

        /// <summary>
        ///     Says the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        void Say(string message, byte type = 0x00);

        /// <summary>
        ///     Sends the animation.
        /// </summary>
        /// <param name="animation">The animation.</param>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="speed">The speed.</param>
        void SendAnimation(ushort animation, Sprite to, Sprite @from, byte speed = 100);

        /// <summary>
        ///     Sends the item shop dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="items">The items.</param>
        void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items);

        /// <summary>
        ///     Sends the item sell dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="items">The items.</param>
        void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items);

        /// <summary>
        ///     Sends the options dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options);

        /// <summary>
        ///     Sends the popup dialog.
        /// </summary>
        /// <param name="popup">The popup.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options);

        /// <summary>
        ///     Sends the options dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="options">The options.</param>
        void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options);

        /// <summary>
        ///     Sends the skill learn dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="skills">The skills.</param>
        void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills);

        /// <summary>
        ///     Sends the spell learn dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        /// <param name="spells">The spells.</param>
        void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells);

        /// <summary>
        ///     Sends the skill forget dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        void SendSkillForgetDialog(Mundane mundane, string text, ushort step);

        /// <summary>
        ///     Sends the spell forget dialog.
        /// </summary>
        /// <param name="mundane">The mundane.</param>
        /// <param name="text">The text.</param>
        /// <param name="step">The step.</param>
        void SendSpellForgetDialog(Mundane mundane, string text, ushort step);

        /// <summary>
        ///     Sends the stats.
        /// </summary>
        /// <param name="flags">The flags.</param>
        GameClient SendStats(StatusFlags flags);

        /// <summary>
        ///     Sends the profile update.
        /// </summary>
        GameClient SendProfileUpdate();

        /// <summary>
        ///     Trains the spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
        void TrainSpell(Spell spell);

        /// <summary>
        ///     Trains the skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        void TrainSkill(Skill skill);

        /// <summary>
        ///     Stop and Interupt everything this client is doing.
        /// </summary>
        void Interupt();

        /// <summary>
        ///     Warps to.
        /// </summary>
        /// <param name="position">The position.</param>
        void WarpTo(Position position);

        /// <summary>
        ///     Repairs the equipment.
        /// </summary>
        /// <param name="gear">The gear.</param>
        void RepairEquipment(IEnumerable<Item> gear);

        /// <summary>
        ///     Revives this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Revive();

        /// <summary>
        ///     Checks the reqs.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CheckReqs(GameClient client, Item item);

        /// <summary>
        ///     Shows the current menu.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="currentitem">The currentitem.</param>
        /// <param name="nextitem">The nextitem.</param>
        void ShowCurrentMenu(Sprite obj, MenuItem currentitem, MenuItem nextitem);

        /// <summary>
        ///     Shows the current menu.
        /// </summary>
        /// <param name="popup">The popup.</param>
        /// <param name="currentitem">The currentitem.</param>
        /// <param name="nextitem">The nextitem.</param>
        void ShowCurrentMenu(Popup popup, MenuItem currentitem, MenuItem nextitem);
    }
}