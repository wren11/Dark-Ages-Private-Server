#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using MenuInterpreter;

#endregion

namespace Darkages.Network.Game
{
    public interface IGameClient
    {
        Aisling Aisling { get; set; }
        DateTime BoardOpened { get; set; }
        bool CanSendLocation { get; }
        DialogSession DlgSession { get; set; }
        GameServerTimer HpRegenTimer { get; set; }
        bool IsRefreshing { get; }
        bool IsWarping { get; }
        byte LastActivatedLost { get; set; }
        DateTime LastAssail { get; set; }
        ushort LastBoardActivated { get; set; }
        DateTime LastClientRefresh { get; set; }
        Item LastItemDropped { get; set; }
        DateTime LastLocationSent { get; set; }
        DateTime LastMapUpdated { get; set; }
        TimeSpan LastMenuStarted { get; }
        DateTime LastMessageSent { get; set; }
        DateTime LastMovement { get; set; }
        DateTime LastPing { get; set; }
        DateTime LastPingResponse { get; set; }
        DateTime LastSave { get; set; }
        DateTime LastScriptExecuted { get; set; }
        DateTime LastWarp { get; set; }
        DateTime LastWhisperMessageSent { get; set; }
        Interpreter MenuInterpter { get; set; }
        GameServerTimer MpRegenTimer { get; set; }
        PendingSell PendingItemSessions { get; set; }
        GameServer Server { get; set; }
        bool ShouldUpdateMap { get; set; }
        bool WasUpdatingMapRecently { get; }

        GameClient AislingToGhostForm();

        void BuildSettings();

        bool CastSpell(string spellName, Sprite caster, Sprite target);

        bool CheckReqs(GameClient client, Item item);

        GameClient CloseDialog();

        GameClient DoUpdate(TimeSpan elapsedTime);

        Task Effect(ushort n, int d = 1000, int r = 1);

        GameClient EnterArea();

        void ForgetSkill(string s);

        void ForgetSpell(string s);

        GameClient GhostFormToAisling();

        void GiveCon(byte v = 1);

        void GiveDex(byte v = 1);

        void GiveExp(int a);

        void GiveHp(int v = 1);

        void GiveInt(byte v = 1);

        bool GiveItem(string itemName);

        void GiveMp(int v = 1);

        void GiveScar();

        void GiveStr(byte v = 1);

        bool GiveTutorialArmor();

        void GiveWis(byte v = 1);

        GameClient HandleTimeOuts();

        GameClient InitSpellBar();

        GameClient Insert();

        void Interupt();

        void KillPlayer(string u);

        void LearnEverything();

        GameClient LearnSkill(Mundane source, SkillTemplate subject, string message);

        GameClient LearnSpell(Mundane source, SpellTemplate subject, string message);

        GameClient LeaveArea(bool update = false, bool delete = false);

        GameClient Load();

        GameClient LoadEquipment();

        GameClient LoadInventory();

        GameClient LoadSkillBook();

        GameClient LoadSpellBook();

        GameClient LoggedIn(bool state);

        void OpenBoard(string n);

        GameClient PayItemPrerequisites(LearningPredicate prerequisites);

        bool PayPrerequisites(LearningPredicate prerequisites);

        bool PlayerUseSkill(string spellname);

        bool PlayerUseSpell(string spellname, Sprite target);

        void Port(int i, int x = 0, int y = 0);

        void Recover();

        GameClient Refresh(bool delete = false);

        GameClient RefreshMap(bool updateView = false);

        GameClient Regen(TimeSpan elapsedTime);

        void ReloadObjects(bool all = false);

        void RepairEquipment(IEnumerable<Item> gear);

        bool Revive();

        void RevivePlayer(string u);

        GameClient Save();

        void Say(string message, byte type = 0x00);

        void SendAnimation(ushort animation, Sprite to, Sprite from, byte speed = 100);

        void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items);

        void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items);

        GameClient SendLocation();

        GameClient SendMessage(byte type, string text);

        GameClient SendMessage(string text);

        void SendMessage(Scope scope, byte type, string text);

        GameClient SendMusic();

        void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options);

        void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options);

        void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options);

        GameClient SendProfileUpdate();

        GameClient SendSerial();

        void SendSkillForgetDialog(Mundane mundane, string text, ushort step);

        void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills);

        GameClient SendSound(byte sound, Scope scope = Scope.Self);

        void SendSpellForgetDialog(Mundane mundane, string text, ushort step);

        void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells);

        GameClient SendStats(StatusFlags flags);

        GameClient SetAislingStartupVariables();

        void ShowCurrentMenu(Sprite obj, MenuItem currentitem, MenuItem nextitem);

        void ShowCurrentMenu(Popup popup, MenuItem currentitem, MenuItem nextitem);

        void Spawn(string t, int x, int y, int c);

        GameClient StatusCheck();

        void StressTest();

        GameClient SystemMessage(string lpmessage);

        bool TakeAwayItem(string item);

        bool TakeAwayItem(Item item);

        void TrainSkill(Skill skill);

        void TrainSpell(Spell spell);

        GameClient TransitionToMap(Area area, Position position);

        GameClient TransitionToMap(int area, Position position);

        void Update(TimeSpan elapsedTime);

        GameClient UpdateDisplay();

        GameClient UpdateReactors(TimeSpan elapsedTime);

        GameClient UpdateStatusBar(TimeSpan elapsedTime);

        void WarpTo(WarpTemplate warps);

        void WarpTo(Position position);
    }
}