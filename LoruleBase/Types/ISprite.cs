#region

using System;

#endregion

namespace Darkages.Types
{
    public interface ISprite
    {
        void ApplyDamage(Sprite source, int dmg, ElementManager.Element element, byte sound = 1);

        void ApplyDamage(Sprite damageDealingSprite, int dmg, bool penetrating = false, byte sound = 1,
            Action<int> dmgcb = null, bool forceTarget = false);

        void OnDamaged(Sprite source, int dmg);

        bool DamageTarget(Sprite damageDealingSprite, ref int dmg, bool penetrating, byte sound, Action<int> dmgcb,
            bool forced);

        int ApplyWeaponBonuses(Sprite source, int dmg);
        void ApplyEquipmentDurability(int dmg);
        double GetElementalModifier(Sprite damageDealingSprite);
        double CalculateElementalDamageMod(ElementManager.Element element);
        int CompleteDamageApplication(int dmg, byte sound, Action<int> dmgcb, double amplifier);
    }
}