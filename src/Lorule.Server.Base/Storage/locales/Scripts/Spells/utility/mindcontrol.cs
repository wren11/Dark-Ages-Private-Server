#region

using System;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Assets.locales.Scripts.Spells.utility
{
    [Script("mind control", "Dean")]
    public class Mindcontrol : SpellScript
    {
        private readonly Random rand = new Random();

        public Mindcontrol(Spell spell) : base(spell)
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
            if (sprite is Aisling playerAisling)
                if (target is Aisling targetAisling)
                {
                    var copy = sprite;

                    sprite = target;
                    target = copy;

                    sprite.Serial = target.Serial;
                    target.Serial = copy.Serial;

                    target.Client.Refresh();
                    sprite.Client.Refresh();
                }
        }
    }
}