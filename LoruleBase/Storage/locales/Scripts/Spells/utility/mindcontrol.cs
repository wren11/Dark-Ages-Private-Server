#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System;

#endregion

namespace Darkages.Assets.locales.Scripts.Spells.utility
{
    [Script("mind control", "Dean")]
    public class mindcontrol : SpellScript
    {
        private readonly Random rand = new Random();

        public mindcontrol(Spell spell) : base(spell)
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
            if (sprite is Aisling _playerAisling)
            {
                if (target is Aisling _targetAisling)
                {
                    var copy = (Sprite)sprite;

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
}