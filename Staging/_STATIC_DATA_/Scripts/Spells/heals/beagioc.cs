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
using Darkages.Types;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("beag ioc", "Dean")]
    public class beagioc : SpellScript
    {
        public beagioc(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
        }


        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (target is Aisling && sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling.CurrentMp >= Spell.Template.ManaCost)
                {
                    client.TrainSpell(Spell);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = (byte)(client.Aisling.Path == Class.Priest ? 0x80 : client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                        Speed = 30
                    };

                    sprite.CurrentMp -= Spell.Template.ManaCost;
                    target.CurrentHp += (200 + ((Spell.Level + sprite.Wis) * 26));

                    if (target.CurrentHp > target.MaximumHp)
                        target.CurrentHp = target.MaximumHp;

                    if (client.Aisling.CurrentMp < 0)
                        client.Aisling.CurrentMp = 0;

                    if (target.CurrentHp > 0)
                    {
                        var hpbar = new ServerFormat13
                        {
                            Serial = target.Serial,
                            Health = (ushort)(100 * target.CurrentHp / target.MaximumHp),
                            Sound = 8
                        };
                        target.Show(Scope.NearbyAislings, hpbar);
                    }

                    sprite.Client.SendStats(StatusFlags.StructB);
                    target.Client.SendAnimation(Spell.Template.Animation, target, client.Aisling);

                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.SendMessage(0x02, "you cast " + Spell.Template.Name + ".");
                    client.SendStats(StatusFlags.All);
                }
                else
                {
                    if (sprite is Aisling)
                    {
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    }
                    return;

                }
            }
            else
            {
                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 80,
                    Speed = 30
                };


                if (sprite is Mundane)
                {
                    var nearby = target.GetObjects(i => i.CurrentMapId == sprite.CurrentMapId &&
                                    i.CurrentHp != i.MaximumHp, Get.Aislings);

                    if (nearby.Count() > 0)
                    {
                        foreach (var s in nearby)
                        {
                            target = s;

                            target.CurrentHp += (200 * ((Spell.Level + sprite.Wis) + 26));
                            if (target.CurrentHp > target.MaximumHp)
                                target.CurrentHp = target.MaximumHp;

                            var hpbar = new ServerFormat13
                            {
                                Serial = target.Serial,
                                Health = (ushort)(100 * target.CurrentHp / target.MaximumHp),
                                Sound = 8
                            };

                            target.Show(Scope.NearbyAislings, hpbar);
                            sprite.Show(Scope.NearbyAislings, action);
                            target.SendAnimation(Spell.Template.Animation, target, target);

                            if (target is Aisling)
                            {
                                (target as Aisling).Client.SendStats(StatusFlags.StructB);
                            }
                        }
                    }
                }
                else
                {
                    target.CurrentHp += (200 * ((Spell.Level + sprite.Wis) + 26));
                    if (target.CurrentHp > target.MaximumHp)
                        target.CurrentHp = target.MaximumHp;

                    var hpbar = new ServerFormat13
                    {
                        Serial = target.Serial,
                        Health = (ushort)(100 * target.CurrentHp / target.MaximumHp),
                        Sound = 8
                    };

                    target.Show(Scope.NearbyAislings, hpbar);
                    sprite.Show(Scope.NearbyAislings, action);
                    target.SendAnimation(Spell.Template.Animation, target, sprite);

                    if (target is Aisling)
                    {
                        (target as Aisling).Client.SendStats(StatusFlags.StructB);
                    }
                }
            }
        }
    }
}

