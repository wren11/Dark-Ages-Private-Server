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
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Assail", "Test")]
    public class Assail : SkillScript
    {
        public Skill _skill;


        public Sprite Target;

        public Assail(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (Target != null)
                if (sprite is Aisling)
                {
                    var client = (sprite as Aisling).Client;
                    client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29(Skill.Template.MissAnimation, (ushort)Target.X, (ushort)Target.Y));
                }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = (byte)(client.Aisling.Path == Class.Warrior ? client.Aisling.UsingTwoHanded ? 0x81 : 0x01 : 0x01),
                    Speed = 20
                };


                var enemy = client.Aisling.GetInfront();
                var success = false;
                if (enemy != null)
                {
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (client.Aisling.Serial == i.Serial)
                            continue;
                        if (i is Money)
                            continue;

                        if (!i.Attackable)
                            continue;

                        if (!sprite.CanHitTarget(i))
                            continue;

                        Target = i;



                        var imp = (Skill.Level * sprite.Level / 70);
                        var dmg = ((client.Aisling.Str * 4) + client.Aisling.Dex * 2) * imp;
                        i.ApplyDamage(sprite, dmg, false, Skill.Template.Sound);
                        success = true;

                        if (i is Aisling)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint)client.Aisling.Serial, (uint)i.Serial, byte.MinValue,
                                    Skill.Template.TargetAnimation, 100));
                            (i as Aisling).Client.Send(new ServerFormat08(i as Aisling, StatusFlags.All));
                        }

                        if (i is Monster || i is Mundane || i is Aisling)
                            client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint)client.Aisling.Serial, (uint)i.Serial,
                                    Skill.Template.TargetAnimation, 0, 100));
                    }
                }

                if (!success)
                    client.Aisling.Show(Scope.VeryNearbyAislings, new ServerFormat13(0, 0, Skill.Template.Sound));

                client.Aisling.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.TrainSkill(Skill);
                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible)
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }
                    OnSuccess(sprite);
                }
            }
            else
            {
                if (!Skill.Ready)
                    return;

                var enemy = sprite.GetInfront();


                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 0x01,
                    Speed = 30
                };

                if (enemy != null)
                {
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (sprite.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        if (!i.Attackable)
                            continue;

                        Target = i;

                        var dmg = sprite.GetBaseDamage(Target);
                        {
                            i.ApplyDamage(sprite, dmg);
                        }

                        if (Skill.Template.TargetAnimation > 0)
                        {
                            if (i is Monster || i is Mundane || i is Aisling)
                                sprite.Show(Scope.NearbyAislings,
                                    new ServerFormat29((uint)sprite.Serial, (uint)i.Serial,
                                        Skill.Template.TargetAnimation, 0, 100));
                        }

                        sprite.Show(Scope.NearbyAislings, action);
                    }
                }
            }
        }
    }
}
