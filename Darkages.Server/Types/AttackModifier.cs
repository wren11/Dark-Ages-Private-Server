namespace Darkages.Types
{
    public enum AttackModifier : byte
    {
        Undefined,
        Defined,
        LevelTable,
        Linear,
        Percentage,
        Random
    }

    public enum DamageModifier : byte
    {
        Undefined,
        Defined,
        Script,
        Fixed
    }
}
