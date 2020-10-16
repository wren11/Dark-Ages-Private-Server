#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

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

                if (target.HasDebuff("mor fas nadur") || target.HasDebuff("fas nadur"))
                {
                    client.SendMessage(0x02, "You have already casted that spell.");
                    return;
                }

                client.TrainSpell(Spell);

                var debuff = new debuff_fasnadur();

                if (!target.HasDebuff(debuff.Name))
                {
                    debuff.OnApplied(target, debuff);

                    if (target is Aisling)
                        (target as Aisling).Client
                            .SendMessage(0x02,
                                $"{client.Aisling.Username} Casts {Spell.Template.Name} on you. Elements augmented.");

                    client.SendMessage(0x02, $"you cast {Spell.Template.Name}");
                    client.SendAnimation(126, target, sprite);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                            client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
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
                    client.SendMessage(0x02, "You have already casted that spell.");
                }
            }
            else
            {
                var debuff = new debuff_fasnadur();

                if (target.HasDebuff("mor fas nadur") || target.HasDebuff("fas nadur")) return;

                if (!target.HasDebuff(debuff.Name))
                {
                    debuff.OnApplied(target, debuff);

                    if (target is Aisling)
                        (target as Aisling).Client
                            .SendMessage(0x02,
                                $"{(sprite is Monster ? (sprite as Monster).Template.Name : (sprite as Mundane).Template.Name) ?? "Monster"} Casts {Spell.Template.Name} on you. Elements augmented.");

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
            {
                sprite.CurrentMp -= Spell.Template.ManaCost;
            }
            else
            {
                if (sprite is Aisling)
                    (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
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