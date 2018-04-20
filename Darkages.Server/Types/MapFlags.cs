namespace Darkages.Types
{
    /// <summary>
    /// CREDITS: Enum Borrowed from... i forgot where... the creator i guess.)
    /// </summary>
    public enum MapFlags : uint
    {
        ArenaTeam = 8192,
        CanLocate = 512,
        CanSummon = 256,
        CanTeleport = 1024,
        CanUseSkill = 2048,
        CanUseSpell = 4096,
        Darkness = Snow | Rain,
        Default = CanSummon | CanLocate | CanTeleport | CanUseSkill | CanUseSpell | SendToHell | ShouldComa,
        HasDayNight = 131072,
        NoMap = 64,
        PlayerKill = 16384,
        Rain = 2,
        SendToHell = 32768,
        ShouldComa = 65536,
        Snow = 1,
        Winter = 128
    }
}