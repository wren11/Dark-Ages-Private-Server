using System;
using System.Linq;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Spells.priest
{
    [Script("Summon Group", "Pillanious")]
    public class SummonGroup : SpellScript
    {
        public SummonGroup(Spell spell) : base(spell)
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
            var spellArgs = Arguments ?? throw new ArgumentNullException(nameof(Arguments));
            spellArgs = spellArgs.Trim();

            if (string.IsNullOrEmpty(spellArgs))
                return;

            var unavailableAreas = ServerContext.GlobalMonsterTemplateCache.Select(i => ServerContext.GlobalMapCache[i.AreaID].Name).Distinct();
            var availableAreas   = ServerContext.GlobalMapCache.Select(i => i.Value.Name).Except(unavailableAreas);
            var summoningTarget  = GetObject<Aisling>(null, i => i.Username.ToLower().Equals(spellArgs.ToLower()));

            if (summoningTarget != null)
                Sprite.Aisling(sprite)?.Client.SummonGroup(summoningTarget, availableAreas.ToArray());
        }
    }
}
