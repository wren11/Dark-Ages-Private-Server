#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("spell_reflect", "Dean")]
    public class spellreflect : SpellScript
    {
        public spellreflect(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.SendMessage(0x02, "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (target is Aisling aobj)
            {
                if (aobj.HasBuff("deireas faileas"))
                {
                    aobj.Client.SendMessage(0x02, "Spells are already being reflected.");
                    return;
                }

                Sprite.Aisling(sprite)
                    ?.HasManaFor(Spell)
                    ?.Cast(Spell, target)
                    ?.ApplyBuff("buff_spell_reflect").Cast<Aisling>()?.UpdateStats(Spell)?.TrainSpell(Spell);
            }
        }
    }
}