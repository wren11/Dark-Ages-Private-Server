using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("fas nadur", "Dean")]
    public class fasnadur : SpellScript
    {
        public fasnadur(Spell spell) : base(spell)
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

                if (target.HasDebuff("mor fas nadur") | target.HasDebuff("fas nadur"))
                {
                    client.SendMessage(0x02, "You have already casted that spell.");
                    return;
                }

                client.TrainSpell(Spell);

                var debuff = Clone(Spell.Template.Debuff);

                if (!target.HasDebuff(debuff.Name))
                {
                    debuff.OnApplied(target, debuff);

                    if (target is Aisling)
                        (target as Aisling).Client
                            .SendMessage(0x02,
                                string.Format("{0} Casts {1} on you. Elements augmented.", client.Aisling.Username,
                                    Spell.Template.Name));

                    client.SendMessage(0x02, string.Format("you cast {0}", Spell.Template.Name));
                    client.SendAnimation(126, target, sprite);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x80,
                        Speed = 30
                    };

                    var hpbar = new ServerFormat13
                    {
                        Serial = client.Aisling.Serial,
                        Health = byte.MaxValue,
                        Sound = 20
                    };

                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.Aisling.Show(Scope.NearbyAislings, hpbar);
                }
                else
                {
                    client.SendMessage(0x02,  "You have already casted that spell.");
                }
            }
            else
            {

                var debuff = Clone(Spell.Template.Debuff);

                if (target.HasDebuff("mor fas nadur") || target.HasDebuff("fas nadur"))
                {
                    return;
                }

                if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        if (target is Aisling)
                        {
                            (target as Aisling).Client
                                .SendMessage(0x02,
                                    string.Format("{0} Casts {1} on you. Elements augmented.",
                                        (sprite is Monster
                                            ? (sprite as Monster).Template.Name
                                            : (sprite as Mundane).Template.Name) ?? "Monster",
                                        Spell.Template.Name));
                        }

                        target.SendAnimation(126, target, sprite);

                        var action = new ServerFormat1A
                        {
                            Serial = sprite.Serial,
                            Number = 1,
                            Speed = 30
                        };

                        var hpbar = new ServerFormat13
                        {
                            Serial = target.Serial,
                            Health = byte.MaxValue,
                            Sound = 20
                        };

                        sprite.Show(Scope.NearbyAislings, action);
                        target.Show(Scope.NearbyAislings, hpbar);
                    }
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite.CurrentMp - Spell.Template.ManaCost > 0)
                sprite.CurrentMp -= Spell.Template.ManaCost;
            else
            {
                if (sprite is Aisling)
                {
                    (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                }
                return;

            }

            if (sprite.CurrentMp < 0)
                sprite.CurrentMp = 0;


            OnSuccess(sprite, target);            

            if (sprite is Aisling)
                (sprite as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructB);
        }
    }
}