#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("beag ioc", "Dean")]
    public class Beagioc : SpellScript
    {
        public Beagioc(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            var healValue = (int) (200 + Spell.Level * sprite.Wis * 0.05);

            Sprite.Aisling(sprite)
                ?.HasManaFor(Spell)
                ?.Cast(Spell, target)
                ?.GiveHealth(target, healValue)
                ?.UpdateStats(Spell)
                ?.TrainSpell(Spell);
        }
    }
}