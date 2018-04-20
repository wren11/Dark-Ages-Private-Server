using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("armachd", "Dean")]
    public class armachd : SpellScript
    {
        private readonly Random rand = new Random();

        public armachd(Spell spell) : base(spell)
        {

        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {

        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                 
                client.TrainSpell(Spell);

                var buff = Clone(Spell.Template.Buff);

                if (!target.HasBuff(buff.Name))
                {
                    buff.OnApplied(target, buff);

                    if (target is Aisling)
                        (target as Aisling).Client
                            .SendMessage(0x02,
                                string.Format("{0} casts {1} on you.", client.Aisling.Username,
                                    Spell.Template.Name));

                    client.SendMessage(0x02, string.Format("you cast {0}", Spell.Template.Name));
                    client.SendAnimation(Spell.Template.Animation, target, sprite);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x80,
                        Speed = 30
                    };

                    var hpbar = new ServerFormat13
                    {
                        Serial = sprite.Serial,
                        Health = 255,
                        Sound = 1
                    };

                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.Aisling.Show(Scope.NearbyAislings, hpbar);
                }
                else
                {
                    client.SendMessage(0x02, "You already cast this.");
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
                    {
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    }
                    return;

                }


                client.SendStats(StatusFlags.StructB);
            }
            else
            {
                var buff = Clone(Spell.Template.Buff);

                if (!sprite.HasBuff(buff.Name))
                {

                    buff.OnApplied(sprite, buff);
                    sprite.SendAnimation(Spell.Template.Animation, sprite, sprite);
                }
            }
        }
    }
}