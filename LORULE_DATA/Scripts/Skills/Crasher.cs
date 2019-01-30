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

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Crasher", "Huy")]
    public class Crasher : SkillScript
    {
        private Skill _skill;
        public Random rand = new Random();
        public Sprite Target;

        public Crasher(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    !string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
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
                    Number = 0x82,
                    Speed = 20
                };

                var enemy = client.Aisling.GetInfront(1);

                if (enemy == null) return;
                foreach (var i in enemy)
                {
                    if (i == null)
                        continue;
                    if (client.Aisling.Serial == i.Serial)
                        continue;
                    if (i is Money)
                        continue;

                    Target = i;

                    var dmg = sprite.MaximumHp * 300 / 100;
                    i.ApplyDamage(sprite, dmg, false, 44);


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

                client.Aisling.CurrentHp = 1;
                client.SendStats(StatusFlags.All);
                client.Aisling.Show(Scope.NearbyAislings, action);
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
                    client.TrainSkill(Skill);
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers == PostQualifer.BreakInvisible)
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    var success = Skill.RollDice(rand);

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                var target = sprite.Target;
                if (target == null)
                    return;

                sprite.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint)target.Serial, (uint)sprite.Serial,
                        Skill.Template.TargetAnimation, 0, 100));

                var dmg = sprite.MaximumHp * 300 / 100;
                target.ApplyDamage(sprite, dmg, false, 44);


                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 0x82,
                    Speed = 20
                };


                sprite.CurrentHp = 1;
                sprite.Show(Scope.NearbyAislings, action);
            }
        }
    }
}
