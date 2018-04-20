using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("puinsein", "Dean")]
    public class puinsein : SpellScript
    {
        private readonly Random rand = new Random();

        public puinsein(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                (sprite as Aisling)
                    .Client
                    .SendMessage(0x02, "Your spell has been deflected.");
                (sprite as Aisling)
                    .Client
                    .SendAnimation(33, target, sprite);
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.TrainSpell(Spell);


                var debuff = Clone<debuff_poison>(Spell.Template.Debuff);
                var curses = target.Debuffs.Values.OfType<debuff_poison>().ToList();

                if (curses.Count == 0)
                {
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        if (target is Aisling)
                            (target as Aisling).Client
                                .SendMessage(0x02,
                                    string.Format("{0} Attacks you with {1}.", client.Aisling.Username,
                                        Spell.Template.Name));

                        client.SendMessage(0x02, string.Format("you cast {0}", Spell.Template.Name));
                        client.SendAnimation(Spell.Template.Animation, target, sprite);

                        var action = new ServerFormat1A
                        {
                            Serial = client.Aisling.Serial,
                            Number = 0x80,
                            Speed = 30
                        };

                        var hpbar = new ServerFormat13
                        {
                            Serial = sprite.Serial,
                            Health = 255,
                            Sound = Spell.Template.Sound
                        };

                        client.Aisling.Show(Scope.NearbyAislings, action);
                        client.Aisling.Show(Scope.NearbyAislings, hpbar);
                    }
                }
                else
                {
                    var c = curses.FirstOrDefault();
                    if (c != null)
                        client.SendMessage(0x02, string.Format("Another poison is already applied. [{0}].", c.Name));
                }
            }
            else
            {
                var debuff = Clone<debuff_poison>(Spell.Template.Debuff);
                var curses = target.Debuffs.Values.OfType<debuff_poison>().ToList();

                if (curses.Count == 0)
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        if (target is Aisling)
                        {
                            (target as Aisling).Client
                                .SendMessage(0x02,
                                    string.Format("{0} Attacks you with {1}.",
                                        (sprite is Monster
                                            ? (sprite as Monster).Template.Name
                                            : (sprite as Mundane).Template.Name) ?? "Monster",
                                        Spell.Template.Name));
                        }

                        target.SendAnimation(Spell.Template.Animation, target, sprite);

                        var action = new ServerFormat1A
                        {
                            Serial = sprite.Serial,
                            Number = 1,
                            Speed = 30
                        };

                        var hpbar = new ServerFormat13
                        {
                            Serial = target.Serial,
                            Health = 255,
                            Sound = Spell.Template.Sound
                        };

                        sprite.Show(Scope.NearbyAislings, action);
                        target.Show(Scope.NearbyAislings, hpbar);
                    }
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
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
            }

            if (rand.Next(0, 100) > target.Mr)
                OnSuccess(sprite, target);
            else
                OnFailed(sprite, target);

            if (sprite is Aisling)
                (sprite as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructB);
        }
    }
}