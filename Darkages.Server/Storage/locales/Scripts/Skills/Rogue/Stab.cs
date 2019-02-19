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
    [Script("Stab", "Dean")]
    public class Stab : SkillScript
    {
        private readonly Random rand = new Random();
        public Skill _skill;

        public Sprite Target;

        public Stab(Skill skill) : base(skill)
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

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = (byte)(client.Aisling.Path == Class.Rogue ? 0x82 : 0x84),
                    Speed = 25
                };

                var enemy = client.Aisling.GetInfront();

                if (enemy != null)
                {
                    foreach (var i in enemy.Cast<Sprite>())
                    {
                        if (i == null)
                            continue;

                        if (client.Aisling.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        if (i is Item)
                            continue;

                        var dmg = (client.Aisling.Invisible ? 2 : 1) * client.Aisling.Dex * 40 * Skill.Level;
                        i.ApplyDamage(sprite, dmg, false, Skill.Template.Sound);

                        if (i is Monster)
                        {
                            (i as Monster).Target = client.Aisling;
                        }

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

                    client.Aisling.Show(Scope.NearbyAislings, action);
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
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

                    var success = Skill.RollDice(rand);

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                if (Skill.Ready)
                {
                    var target = sprite.Target;
                    if (target == null)
                        return;


                    target.Show(Scope.NearbyAislings,
                         new ServerFormat29((uint)target.Serial, (uint)target.Serial,
                             Skill.Template.TargetAnimation, 0, 100));

                    var dmg = sprite.GetBaseDamage(target, MonsterDamageType.Physical);
                    target.ApplyDamage(sprite, dmg, true, Skill.Template.Sound);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x82,
                        Speed = 20
                    };


                    target.Show(Scope.NearbyAislings, action);
                }
            }
        }
    }
}
