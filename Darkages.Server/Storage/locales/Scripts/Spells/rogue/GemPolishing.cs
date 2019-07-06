using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Spells.rogue
{
    [Script("gem_polishing", "Dean")]
    public class GemPolishing : SpellScript
    {
        public GemPolishing(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling aisling)
            {
                aisling.MakeReactor("gem_polishing", 30);
                aisling.Client.SystemMessage(string.Format("You cast {0}.", Spell.Template.Name));
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            OnSuccess(sprite, target);
        }
    }
}