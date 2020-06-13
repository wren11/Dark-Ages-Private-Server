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
        GameServer Server { get; set; }

        Aisling Aisling { get; set; }

        GameServerTimer HpRegenTimer { get; set; }

        GameServerTimer MpRegenTimer { get; set; }

        Interpreter MenuInterpter { get; set; }

        DialogSession DlgSession { get; set; }

        Item LastItemDropped { get; set; }

        DateTime BoardOpened { get; set; }

        DateTime LastWhisperMessageSent { get; set; }

        DateTime LastAssail { get; set; }

        DateTime LastMessageSent { get; set; }

        DateTime LastPingResponse { get; set; }

        DateTime LastWarp { get; set; }

        DateTime LastScriptExecuted { get; set; }

        DateTime LastPing { get; set; }

        DateTime LastSave { get; set; }

        DateTime LastClientRefresh { get; set; }

        DateTime LastMovement { get; set; }

        bool IsRefreshing { get; }

        bool IsWarping { get; }

        bool CanSendLocation { get; }

        bool WasUpdatingMapRecently { get; }

        DateTime LastLocationSent { get; set; }

        DateTime LastMapUpdated { get; set; }

        ushort LastBoardActivated { get; set; }

        bool ShouldUpdateMap { get; set; }

        byte LastActivatedLost { get; set; }

        PendingSell PendingItemSessions { get; set; }

        TimeSpan LastMenuStarted { get; }
        void Port(int i, int x = 0, int y = 0);

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

        void BuildSettings();

        void WarpTo(WarpTemplate warps);

        GameClient LearnSpell(Mundane source, SpellTemplate subject, string message);

        GameClient LearnSkill(Mundane source, SkillTemplate subject, string message);

        bool PayPrerequisites(LearningPredicate prerequisites);

        GameClient PayItemPrerequisites(LearningPredicate prerequisites);

        GameClient TransitionToMap(Area area, Position position);

        GameClient TransitionToMap(int area, Position position);

        GameClient CloseDialog();

        void Update(TimeSpan elapsedTime);

        GameClient DoUpdate(TimeSpan elapsedTime);

        GameClient UpdateReactors(TimeSpan elapsedTime);

        GameClient SystemMessage(string lpmessage);

        GameClient StatusCheck();

        GameClient HandleTimeOuts();

        GameClient UpdateStatusBar(TimeSpan elapsedTime);

        GameClient Load();

        GameClient SetAislingStartupVariables();

        GameClient Regen(TimeSpan elapsedTime);

        GameClient InitSpellBar();

        GameClient LoadEquipment();

        GameClient AislingToGhostForm();

        GameClient GhostFormToAisling();

        GameClient LoadSpellBook();

        GameClient LoadSkillBook();

        GameClient LoadInventory();

        GameClient UpdateDisplay();

        GameClient Refresh(bool delete = false);

        GameClient LeaveArea(bool update = false, bool delete = false);

        GameClient EnterArea();

        GameClient SendMusic();

        GameClient SendSound(byte sound, Scope scope = Scope.Self);

        GameClient Insert();

        GameClient RefreshMap();

        GameClient SendSerial();

        GameClient SendLocation();

        GameClient Save();

        GameClient SendMessage(byte type, string text);

        GameClient SendMessage(string text);

        void SendMessage(Scope scope, byte type, string text);

        void Say(string message, byte type = 0x00);

        void SendAnimation(ushort animation, Sprite to, Sprite from, byte speed = 100);

        void SendItemShopDialog(Mundane mundane, string text, ushort step, IEnumerable<ItemTemplate> items);

        void SendItemSellDialog(Mundane mundane, string text, ushort step, IEnumerable<byte> items);

        void SendOptionsDialog(Mundane mundane, string text, params OptionsDataItem[] options);

        void SendPopupDialog(Popup popup, string text, params OptionsDataItem[] options);

        void SendOptionsDialog(Mundane mundane, string text, string args, params OptionsDataItem[] options);

        void SendSkillLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SkillTemplate> skills);

        void SendSpellLearnDialog(Mundane mundane, string text, ushort step, IEnumerable<SpellTemplate> spells);

        void SendSkillForgetDialog(Mundane mundane, string text, ushort step);

        void SendSpellForgetDialog(Mundane mundane, string text, ushort step);

        GameClient SendStats(StatusFlags flags);

        GameClient SendProfileUpdate();

        void TrainSpell(Spell spell);

        void TrainSkill(Skill skill);

        void Interupt();

        void WarpTo(Position position);

        void RepairEquipment(IEnumerable<Item> gear);

        bool Revive();

        bool CheckReqs(GameClient client, Item item);

        void ShowCurrentMenu(Sprite obj, MenuItem currentitem, MenuItem nextitem);

        void ShowCurrentMenu(Popup popup, MenuItem currentitem, MenuItem nextitem);
    }
}