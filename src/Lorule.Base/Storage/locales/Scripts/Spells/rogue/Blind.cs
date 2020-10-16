#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells.rogue
{
    [Script("Blind", "Dean")]
    public class Blind : SpellScript
    {
        public Debuff debuff = new debuff_blind();
        public Random rnd = new Random();

        public Blind(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling aisling)
                aisling.Client.SendMessage(0x02, ServerContext.Config.SpellFailedMessage);
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            var targets = target.MonstersNearby().Where(i => i.WithinRangeOf(target, 4) && i.Serial != target.Serial);

            foreach (var obj in targets)
            {
                var affect = Clone<debuff_blind>(debuff);
                affect.OnApplied(obj, affect);
            }

            if (sprite is Aisling aisling)
            {
                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte) (aisling.Path
                                     == Class.Priest ? 0x80 :
                        aisling.Path
                        == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };
                sprite.Show(Scope.NearbyAislings, action);
                aisling.Client.SendMessage(0x02, "you cast " + Spell.Template.Name + ".");
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling aisling)
            {
                if (aisling.CurrentMp > Spell.Template.ManaCost)
                {
                    aisling.CurrentMp -= Spell.Template.ManaCost;

                    aisling.Client.SendStats(StatusFlags.All);
                    aisling.Client.TrainSpell(Spell);
                }
                else
                {
                    aisling.Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    return;
                }
            }

            if (Spell.RollDice(rnd))
                OnSuccess(sprite, target);
            else
                OnSuccess(sprite, target);
        }
    }
}