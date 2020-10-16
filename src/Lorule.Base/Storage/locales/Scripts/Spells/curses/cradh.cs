#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("cradh", "Dean")]
    public class cradh : SpellScript
    {
        private readonly Random rand = new Random();

        public cradh(Spell spell) : base(spell)
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

                var debuff = new debuff_cradh();
                var curses = target.Debuffs.Values.OfType<debuff_cursed>().ToList();

                if (curses.Count == 0)
                {
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        if (target is Aisling)
                            (target as Aisling).Client
                                .SendMessage(0x02,
                                    $"{client.Aisling.Username} Attacks you with {Spell.Template.Name}.");

                        client.SendMessage(0x02, $"you cast {Spell.Template.Name}");
                        client.SendAnimation(258, target, sprite);

                        var action = new ServerFormat1A
                        {
                            Serial = sprite.Serial,
                            Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                                client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                            Speed = 30
                        };

                        var hpbar = new ServerFormat13
                        {
                            Serial = sprite.Serial,
                            Health = 255,
                            Sound = 27
                        };

                        client.Aisling.Show(Scope.NearbyAislings, action);
                        client.Aisling.Show(Scope.NearbyAislings, hpbar);
                    }
                }
                else
                {
                    var c = curses.FirstOrDefault();
                    if (c != null)
                        client.SendMessage(0x02, $"Another curse is afflicted [{c.Name}].");
                }
            }
            else
            {
                var debuff = new debuff_cradh();
                var curses = target.Debuffs.Values.OfType<debuff_cursed>().ToList();

                if (curses.Count == 0)
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        if (target is Aisling)
                            (target as Aisling).Client
                                .SendMessage(0x02,
                                    $"{(sprite is Monster ? (sprite as Monster).Template.Name : (sprite as Mundane).Template.Name) ?? "Monster"} Attacks you with {Spell.Template.Name}.");

                        target.SendAnimation(258, target, sprite);

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
                            Sound = 27
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