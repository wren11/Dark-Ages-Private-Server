///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("ao sith", "Dean")]
    public class ao_sith : SpellScript
    {
        private readonly Random rand = new Random();

        public ao_sith(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
                (sprite as Aisling)
                    .Client
                    .SendMessage(0x02, "failed.");
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (target == null)
                return;

            target.RemoveBuffsAndDebuffs();
            target.SendAnimation(Spell.Template.Animation, target, sprite);

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.TrainSpell(Spell);
                client.SendMessage(0x02, $"you cast {Spell.Template.Name}");

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
                    Health = 255,
                    Sound = Spell.Template.Sound
                };

                client.Aisling.Show(Scope.NearbyAislings, action);
                client.Aisling.Show(Scope.NearbyAislings, hpbar);

                if (target is Aisling)
                {
                    (target as Aisling).Client.SendStats(StatusFlags.All);

                    (target as Aisling).Client
                        .SendMessage(0x02,
                            $"{client.Aisling.Username} Attacks you with {Spell.Template.Name}.");
                }
            }
            else
            {
                if (target is Aisling)
                {
                    (target as Aisling).Client.SendStats(StatusFlags.All);
                    (target as Aisling).Client
                        .SendMessage(0x02,
                            $"{(sprite is Monster ? (sprite as Monster).Template.Name : (sprite as Mundane).Template.Name) ?? "Monster"} Attacks you with {Spell.Template.Name}.");
                }


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

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                if (sprite.CurrentMp - Spell.Template.ManaCost > 0)
                    sprite.CurrentMp -= Spell.Template.ManaCost;

                if (sprite.CurrentMp < 0)
                    sprite.CurrentMp = 0;


                (sprite as Aisling).Client.SendMessage(0x02, ServerContextBase.GlobalConfig.NoManaMessage);
            }

            var success = Spell.RollDice(rand);

            if (success)
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