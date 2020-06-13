#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

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
                aisling.MakeReactor("GramailPrayer", 30);
                aisling.Client.SystemMessage($"You cast {Spell.Template.Name}.");
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            OnSuccess(sprite, target);
        }
    }
}