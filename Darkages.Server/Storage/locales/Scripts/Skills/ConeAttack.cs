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
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Cone Attack", "Dean")]
    public class ConeAttack : SkillScript
    {
        public Skill _skill;

        public int Damage => (150 * _skill.Level) / 100;

        public ConeAttack(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public List<Sprite> GetInCone(Sprite sprite, int distance)
        {
            var result = new List<Sprite>();
            var objects = GetObjects(i => i.WithinRangeOf(sprite, distance), Get.Aislings | Get.Monsters | Get.Mundanes);
            foreach (var obj in objects)
            {
                if (sprite.Position.DistanceSquared(obj.Position) <= distance)
                {
                    if ((Direction)sprite.Direction == Direction.North)
                    {
                        if (obj.Y <= sprite.Y)
                            result.Add(obj);
                    }
                    else if ((Direction)sprite.Direction == Direction.South)
                    {
                        if (obj.Y >= sprite.Y)
                            result.Add(obj);
                    }
                    else if ((Direction)sprite.Direction == Direction.East)
                    {
                        if (obj.X >= sprite.X)
                            result.Add(obj);
                    }
                    else if ((Direction)sprite.Direction == Direction.West)
                    {
                        if (obj.X <= sprite.X)
                            result.Add(obj);
                    }

                }
            }

            return result;
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte)(client.Aisling.Path == Class.Warrior ? client.Aisling.UsingTwoHanded ? 0x81 : 0x01 : 0x01),
                    Speed = 20
                };

                var enemy = GetInCone(sprite, 3);

                if (enemy != null)
                {
                    foreach (var i in enemy.OfType<Monster>())
                    {
                        if (i == null)
                            continue;



                        if (client.Aisling.Serial == i.Serial)
                            continue;


                        var dmg = Damage;
                        i.ApplyDamage(sprite, dmg, false, Skill.Template.Sound);
                        i.Target = client.Aisling;

                        client.Aisling.Show(Scope.NearbyAislings,
                            new ServerFormat29(Skill.Template.TargetAnimation, (ushort)i.X, (ushort)i.Y));
                    }

                    client.Aisling.Show(Scope.NearbyAislings, action);
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (!Skill.Ready)
                return;

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers.HasFlag(PostQualifer.BreakInvisible))
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    client.TrainSkill(Skill);

                    var success = true;

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                var enemy = GetInCone(sprite, 3);

                if (enemy != null)
                {
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (sprite.Serial == i.Serial)
                            continue;

                        if (i is Aisling || i is Monster)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29(Skill.Template.TargetAnimation, (ushort)i.X, (ushort)i.Y));

                            var dmg = (50 * (sprite.Str+ Skill.Level)) / 100;
                            i.ApplyDamage(sprite, dmg, true, Skill.Template.Sound);
                        }
                    }
                }
            }
        }
    }
}
