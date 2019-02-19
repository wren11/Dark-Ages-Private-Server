///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("ard puinsein", "Dean")]
    public class ardpuinsein : SpellScript
    {
        private readonly Random rand = new Random();

        public ardpuinsein(Spell spell) : base(spell)
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

                var debuff = new Debuff_poison("ard puinsein", 700, 35, 25, 0.02);
                var curses = target.Debuffs.Values.OfType<Debuff_poison>().ToList();

                if (curses.Count == 0)
                {
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        SendCastSpellOrder(sprite, target, client);

                    }
                }
                else
                {
                    AlreadyCursed(client, curses);
                }
            }
            else
            {
                var debuff = new Debuff_poison("ard puinsein", 700, 35, 25, 0.02);
                var curses = target.Debuffs.Values.OfType<Debuff_poison>().ToList();

                if (curses.Count == 0)
                    if (!target.HasDebuff(debuff.Name))
                    {
                        debuff.OnApplied(target, debuff);

                        SpriteSpellCastOrder(sprite, target);
                    }
            }
        }

        public void SpriteSpellCastOrder(Sprite sprite, Sprite target)
        {
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

            target.Show(Scope.NearbyAislings, hpbar);
            target.SendAnimation(Spell.Template.Animation, target, sprite);

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

            sprite.Show(Scope.NearbyAislings, action);
        }

        public void AlreadyCursed(Network.Game.GameClient client, System.Collections.Generic.List<Debuff_poison> curses)
        {
            var c = curses.FirstOrDefault();
            if (c != null)
                client.SendMessage(0x02, string.Format("Another poison is already applied. [{0}].", c.Name));
        }

        private void SendCastSpellOrder(Sprite sprite, Sprite target, Network.Game.GameClient client, bool IsAttack = false)
        {
            var hpbar = new ServerFormat13
            {
                Serial = sprite.Serial,
                Health = 255,
                Sound = Spell.Template.Sound
            };

            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = (byte)(client.Aisling.Path == Class.Priest ? 0x80 : client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                Speed = 30
            };

            client.SendStats(StatusFlags.StructB | StatusFlags.StructD);
            client.Aisling.Show(Scope.NearbyAislings, hpbar);
            client.SendAnimation(Spell.Template.Animation, target, sprite);
            client.SendMessage(0x02, string.Format("you cast {0}", Spell.Template.Name));

            if (IsAttack)
            {
                if (target is Aisling)
                    (target as Aisling).Client.SendMessage(0x02, string.Format("{0} Attacks you with {1}.", client.Aisling.Username, Spell.Template.Name));
            }

            client.Aisling.Show(Scope.NearbyAislings, action);
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
