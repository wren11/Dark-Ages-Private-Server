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
using System.Threading.Tasks;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Charge", "Dean")]
    public class Charge : SkillScript
    {
        private readonly Random rand = new Random();
        public Skill _skill;

        public Sprite Target;

        public Charge(Skill skill) : base(skill)
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
            var collided = false;

            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = 0x82,
                Speed = 20
            };

            int steps = 0;
            for (int i = 0; i < 5; i++)
            {
                sprite.Walk();
                steps++;

                var targets = sprite.GetInfront(1, true);

                foreach (var target in targets)
                {
                    if (target.Serial == sprite.Serial)
                        continue;

                    if (target != null && sprite.Position.IsNextTo(target.Position))
                    {
                        var imp = (Skill.Level * 5 / 100);
                        var dmg = 15 * (((sprite.Str* 2) + sprite.Dex * imp));
                        target.ApplyDamage(sprite, dmg, false, Skill.Template.Sound);
                        {
                            Target = target;
                            collided = true;
                        }
                    }
                }

                if (collided)
                    break;
            }

            if (sprite is Aisling)
            {
                (sprite as Aisling).Client.Refresh();
            }

            Task.Delay(150).ContinueWith((dc) =>
            {
                if (Target != null && collided)
                {
                    if (Target is Monster || Target is Mundane || Target is Aisling)
                        Target.Show(Scope.NearbyAislings,
                            new ServerFormat29((uint)sprite.Serial, (uint)Target.Serial,
                                Skill.Template.TargetAnimation, 0, 100));

                }

                sprite.Show(Scope.NearbyAislings, action);
            }).Wait();

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
                }
            }

            var success = Skill.RollDice(rand);

            if (success)
                OnSuccess(sprite);
            else
                OnFailed(sprite);
        }
    }
}
