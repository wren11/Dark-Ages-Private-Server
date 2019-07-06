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

using System;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Rush", "Dean")]
    public class Rush : SkillScript
    {
        private readonly Random rand = new Random();
        public Skill _skill;

        public Sprite Target;

        public Rush(Skill skill) : base(skill)
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

            var debuff = new debuff_frozen();
            Sprite t = null;

            for (var i = 0; i < 5; i++)
            {
                var targets = sprite.GetInfront(i, true);
                var hits = 0;
                foreach (var target in targets)
                {
                    if (target.Serial == sprite.Serial)
                        continue;

                    if (target != null)
                        if (sprite is Aisling aisling)
                        {
                            var position = target.Position;

                            if (sprite.Direction == 0)
                                position.Y++;
                            if (sprite.Direction == 1)
                                position.X--;
                            if (sprite.Direction == 2)
                                position.Y--;
                            if (sprite.Direction == 3)
                                position.X++;


                            aisling.Client.WarpTo(position);
                        }

                    t = target;
                    hits++;
                }

                if (hits > 0)
                {
                    if (t != null)
                    {
                        t.RemoveDebuff("sleep");
                        t.RemoveDebuff("frozen");

                        if (!t.HasDebuff(debuff.Name))
                        {
                            sprite.Show(Scope.NearbyAislings, action);
                            t.ApplyDamage(sprite, 0, false, Skill.Template.Sound);
                            debuff.OnApplied(t, debuff);
                        }
                    }

                    collided = true;
                    break;
                }
            }

            Task.Delay(50).ContinueWith(dc =>
            {
                if (Target != null && collided)
                {
                    if (Target is Monster || Target is Mundane || Target is Aisling)
                        Target.Show(Scope.NearbyAislings,
                            new ServerFormat29((uint) sprite.Serial, (uint) Target.Serial,
                                Skill.Template.TargetAnimation, 0, 100));


                    sprite.Show(Scope.NearbyAislings, action);
                }
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