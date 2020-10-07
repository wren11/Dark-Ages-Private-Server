#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.Buffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("aite", "Dean")]
    public class aite : SpellScript
    {
        public aite(Spell spell) : base(spell)
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
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var buff = new BuffAite();

                client.TrainSpell(Spell);

                if (!target.HasBuff(buff.Name))
                {
                    buff.OnApplied(target, buff);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                            client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                        Speed = 30
                    };

                    client.SendAnimation(Spell.Template.Animation, target, client.Aisling);
                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.SendMessage(0x02, "you cast " + Spell.Template.Name + ".");
                    client.SendStats(StatusFlags.All);
                }
                else
                {
                    client.SendMessage(0x02, "That target is already empowered.");
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

                    OnSuccess(sprite, target);
                }
                else
                {
                    if (sprite is Aisling)
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    return;
                }

                client.SendStats(StatusFlags.StructB);
            }
            else
            {
                var buff = new BuffAite();

                if (!target.HasBuff(buff.Name))
                {
                    buff.OnApplied(target, buff);
                    sprite.SendAnimation(Spell.Template.Animation, target, sprite);
                }
            }
        }
    }
}