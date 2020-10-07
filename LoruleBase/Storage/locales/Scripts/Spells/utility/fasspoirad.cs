#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Assets.locales.Scripts.Spells.utility
{
    [Script("fas spiorad", "Dean")]
    public class fasspoirad : SpellScript
    {
        private readonly Random rand = new Random();

        public fasspoirad(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.SendMessage(0x02, ServerContext.Config.SomethingWentWrong);
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var debuff = new debuff_fasspoirad();

                client.TrainSpell(Spell);

                if (!sprite.HasDebuff(debuff.Name))
                {
                    debuff.OnApplied(sprite, debuff);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                            client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                        Speed = 30
                    };

                    client.Aisling.CurrentHp -= client.Aisling.MaximumMp / 3;
                    client.Aisling.CurrentMp = client.Aisling.MaximumMp;

                    if (client.Aisling.CurrentHp <= 0)
                        client.Aisling.CurrentHp = 1;

                    client.SendAnimation(1, client.Aisling, client.Aisling);
                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.SendMessage(0x02, "you invoke your will.");
                    client.SendStats(StatusFlags.All);
                }
                else
                {
                    client.SendMessage(0x02, "you failed to concretrate.");
                }
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling.CurrentMp >= Spell.Template.ManaCost)
                {
                    client.Aisling.CurrentMp -= Spell.Template.ManaCost;
                    if (client.Aisling.CurrentMp < 0)
                        client.Aisling.CurrentMp = 0;

                    if (rand.Next(1, 101) >= 25) OnSuccess(sprite, target);
                    else
                        OnFailed(sprite, target);
                }
            }
        }
    }
}