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
    [Script("mor fas nadur", "Dean")]
    public class morfasnadur : SpellScript
    {
        public morfasnadur(Spell spell) : base(spell)
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

                var debuff = new debuff_morfasnadur();

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
                        Number = (byte)(client.Aisling.Path == Class.Priest ? 0x80 : client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
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

                if (target.HasDebuff("mor fas nadur") || target.HasDebuff("fas nadur"))
                {
                    return;
                }

                var debuff = new debuff_morfasnadur();

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
