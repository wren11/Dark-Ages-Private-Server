#region

using Darkages.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace Darkages
{
    public interface IServerConstants
    {
        bool AssailsCancelSpells { get; set; }
        string BadRequestMessage { get; set; }
        byte BaseAC { get; set; }
        byte BaseMR { get; set; }
        byte BaseStatAttribute { get; set; }
        double BehindDamageMod { get; set; }
        bool CancelCastingWhenWalking { get; set; }
        bool CancelWalkingIfRefreshing { get; set; }
        bool CanMoveDuringReap { get; set; }
        string CantAttack { get; set; }
        string CantCarryMoreMsg { get; set; }
        string CantDoThat { get; set; }
        string CantDropItemMsg { get; set; }
        string CantEquipThatMessage { get; set; }
        string CantUseThat { get; set; }
        string CantWearYetMessage { get; set; }
        string ChantPrefix { get; set; }
        string ChantSuffix { get; set; }
        int ClickLootDistance { get; set; }
        int ClientVersion { get; set; }
        string ConAddedMessage { get; set; }
        int ConnectionCapacity { get; set; }
        string CursedItemMessage { get; set; }
        double DayTimeInterval { get; set; }
        int DeathHPPenalty { get; set; }
        int DeathMap { get; set; }
        int DeathMapX { get; set; }
        int DeathMapY { get; set; }
        string DeathReepingMessage { get; set; }
        bool DebugMode { get; set; }
        ItemColor DefaultItemColor { get; set; }
        uint DefaultItemDurability { get; set; }
        uint DefaultItemValue { get; set; }
        bool DevMode { get; set; }
        string DexAddedMessage { get; set; }
        string DoesNotFitMessage { get; set; }
        int ERRORCAP { get; set; }
        double FasNadurStrength { get; set; }
        List<string> GameMasters { get; set; }
        bool GiveAssailOnCreate { get; }
        double GlobalBaseSkillDelay { get; set; }
        double GlobalSpawnTimer { get; set; }
        double GroupExpBonus { get; set; }
        string GroupRequestDeclinedMsg { get; set; }
        string HandShakeMessage { get; set; }
        int HelperMenuId { get; set; }
        string HelperMenuTemplateKey { get; set; }
        int HpGainFactor { get; set; }
        string IntAddedMessage { get; set; }
        string ItemNotRequiredMsg { get; set; }
        string LevelUpMessage { get; set; }
        double LingerState { get; set; }
        bool LogClientPackets { get; set; }
        int LOGIN_PORT { get; set; }
        bool LogServerPackets { get; set; }
        int LootTableStackSize { get; set; }
        int MaxCarryGold { get; set; }
        int MaxHP { get; set; }
        string MerchantBuy { get; set; }
        string MerchantBuyMessage { get; set; }
        string MerchantCancelMessage { get; set; }
        string MerchantConfirmMessage { get; set; }
        string MerchantRefuseTradeMessage { get; set; }
        string MerchantSell { get; set; }
        string MerchantStackErrorMessage { get; set; }
        string MerchantTradeCompletedMessage { get; set; }
        string MerchantTradeErrorMessage { get; set; }
        string MerchantWarningMessage { get; set; }
        double MessageClearInterval { get; set; }
        int MinimumHp { get; set; }
        int MonsterSpellSuccessRate { get; set; }
        double MorFasNadurStrength { get; set; }
        int MpGainFactor { get; set; }
        bool MultiUserLogin { get; set; }
        double MundaneRespawnInterval { get; set; }
        double NationReturnHours { get; }
        string NoManaMessage { get; set; }
        string NotEnoughGoldToDropMsg { get; set; }
        double PingInterval { get; set; }
        int PlayerLevelCap { get; set; }
        int PVPMap { get; set; }
        string ReapMessage { get; set; }
        string ReapMessageDuringAction { get; set; }
        int RefreshRate { get; set; }
        int RegenRate { get; set; }
        string RepairItemMessage { get; set; }
        double SaveRate { get; set; }
        int SERVER_PORT { get; set; }
        string SERVER_TITLE { get; set; }
        string ServerWelcomeMessage { get; set; }
        List<GameSetting> Settings { get; set; }
        int SkullLength { get; set; }
        string SomethingWentWrong { get; set; }
        string SpellFailedMessage { get; set; }
        int StartingMap { get; set; }
        Position StartingPosition { get; set; }
        byte StatCap { get; set; }
        int StatsPerLevel { get; set; }
        string StrAddedMessage { get; set; }
        string ToWeakToLift { get; set; }
        short TransitionPointX { get; set; }
        short TransitionPointY { get; set; }
        int TransitionZone { get; set; }
        bool UseLobby { get; set; }
        bool UseLoruleItemRarity { get; set; }
        bool UseLoruleVariants { get; set; }
        string UserDroppedGoldMsg { get; set; }
        int VeryNearByProximity { get; set; }
        int WarpCheckRate { get; set; }
        double WeightIncreaseModifer { get; set; }
        string WisAddedMessage { get; set; }
        int WithinRangeProximity { get; set; }
        string WrongClassMessage { get; set; }
        string YouDroppedGoldMsg { get; set; }
    }

    public class GameSetting
    {
        [JsonProperty] public bool Enabled { get; set; }
        [JsonProperty] public string SettingOff { get; set; }
        [JsonProperty] public string SettingOn { get; set; }
    }

    public class ServerConstants : IServerConstants
    {
        public bool AssailsCancelSpells { get; set; }
        public string BadRequestMessage { get; set; }
        public byte BaseAC { get; set; } = 170;
        public byte BaseMR { get; set; } = 70;
        public byte BaseStatAttribute { get; set; }
        public double BehindDamageMod { get; set; }
        public bool CancelCastingWhenWalking { get; set; }
        public bool CancelWalkingIfRefreshing { get; set; }
        public bool CanMoveDuringReap { get; set; }
        public string CantAttack { get; set; }
        public string CantCarryMoreMsg { get; set; }
        public string CantDoThat { get; set; }
        public string CantDropItemMsg { get; set; }
        public string CantEquipThatMessage { get; set; }
        public string CantUseThat { get; set; }
        public string CantWearYetMessage { get; set; }
        public string ChantPrefix { get; set; }
        public string ChantSuffix { get; set; }
        public int ClickLootDistance { get; set; }
        public int ClientVersion { get; set; }
        public string ConAddedMessage { get; set; }
        public int ConnectionCapacity { get; set; }
        public string CursedItemMessage { get; set; }
        public double DayTimeInterval { get; set; }
        public int DeathHPPenalty { get; set; }
        public int DeathMap { get; set; }
        public int DeathMapX { get; set; }
        public int DeathMapY { get; set; }
        public string DeathReepingMessage { get; set; }
        public bool DebugMode { get; set; }
        [JsonProperty] public ItemColor DefaultItemColor { get; set; }
        public uint DefaultItemDurability { get; set; }
        public uint DefaultItemValue { get; set; }
        public bool DevMode { get; set; }
        public string DexAddedMessage { get; set; }
        public string DoesNotFitMessage { get; set; }
        public int ERRORCAP { get; set; }
        public double FasNadurStrength { get; set; }
        public List<string> GameMasters { get; set; }
        public bool GiveAssailOnCreate { get; set; }
        public double GlobalBaseSkillDelay { get; set; }
        public double GlobalSpawnTimer { get; set; }
        public double GroupExpBonus { get; set; }
        public string GroupRequestDeclinedMsg { get; set; }
        public string HandShakeMessage { get; set; }
        public int HelperMenuId { get; set; }
        public string HelperMenuTemplateKey { get; set; }
        public int HpGainFactor { get; set; }
        public string IntAddedMessage { get; set; }
        public string ItemNotRequiredMsg { get; set; }
        public string LevelUpMessage { get; set; }
        public double LingerState { get; set; }
        public bool LogClientPackets { get; set; }
        public int LOGIN_PORT { get; set; }
        public bool LogServerPackets { get; set; }
        public int LootTableStackSize { get; set; }
        public int MaxCarryGold { get; set; }
        public int MaxHP { get; set; }
        public string MerchantBuy { get; set; }
        public string MerchantBuyMessage { get; set; }
        public string MerchantCancelMessage { get; set; }
        public string MerchantConfirmMessage { get; set; }
        public string MerchantRefuseTradeMessage { get; set; }
        public string MerchantSell { get; set; }
        public string MerchantStackErrorMessage { get; set; }
        public string MerchantTradeCompletedMessage { get; set; }
        public string MerchantTradeErrorMessage { get; set; }
        public string MerchantWarningMessage { get; set; }
        public double MessageClearInterval { get; set; }
        public int MinimumHp { get; set; }
        public int MonsterSpellSuccessRate { get; set; }
        public double MorFasNadurStrength { get; set; }
        public int MpGainFactor { get; set; }
        public bool MultiUserLogin { get; set; }
        public double MundaneRespawnInterval { get; set; }
        public double NationReturnHours { get; set; }
        public string NoManaMessage { get; set; }
        public string NotEnoughGoldToDropMsg { get; set; }
        public double PingInterval { get; set; }
        public int PlayerLevelCap { get; set; }
        public int PVPMap { get; set; }
        public string ReapMessage { get; set; }
        public string ReapMessageDuringAction { get; set; }
        public int RefreshRate { get; set; }
        public int RegenRate { get; set; }
        public string RepairItemMessage { get; set; }
        public double SaveRate { get; set; }
        public int SERVER_PORT { get; set; }

        public string SERVER_TITLE { get; set; }

        public string ServerWelcomeMessage { get; set; }

        public List<GameSetting> Settings { get; set; }

        public int SkullLength { get; set; }

        public string SomethingWentWrong { get; set; }

        public string SpellFailedMessage { get; set; }

        public int StartingMap { get; set; }

        [JsonProperty] public Position StartingPosition { get; set; }

        public byte StatCap { get; set; }

        public int StatsPerLevel { get; set; }

        public string StrAddedMessage { get; set; }

        public string ToWeakToLift { get; set; }

        public short TransitionPointX { get; set; }

        public short TransitionPointY { get; set; }

        public int TransitionZone { get; set; }

        public bool UseLobby { get; set; }
        public bool UseLoruleItemRarity { get; set; }
        public bool UseLoruleVariants { get; set; }
        public string UserDroppedGoldMsg { get; set; }

        public int VeryNearByProximity { get; set; }

        public int WarpCheckRate { get; set; }

        public double WeightIncreaseModifer { get; set; }

        public string WisAddedMessage { get; set; }

        public int WithinRangeProximity { get; set; }

        public string WrongClassMessage { get; set; }

        public string YouDroppedGoldMsg { get; set; }
    }
}