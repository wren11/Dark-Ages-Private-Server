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

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("beag suain ia", author: "Wren")]
    public class beagsuainia : SkillScript
    {
        public Random rand = new Random();
        public Sprite Target;

        public beagsuainia(Skill skill) : base(skill)
        {

        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29(Skill.Template.MissAnimation, (ushort)sprite.X, (ushort)sprite.Y));

                client.SendMessage(0x02, "The enemy has made it through.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            var a = sprite.AislingsNearby().ToList();
            var b = sprite.MonstersNearby().ToList();

            var i = a.Concat<Sprite>(b);

            if (i == null || !i.Any())
            {
                if (sprite is Aisling)
                {
                    var client = (sprite as Aisling).Client;
                    client.SendMessage(0x02, "The enemy has made it through.");
                    return;
                }
            }

            foreach (var target in i)
            {
                if (target.Serial == sprite.Serial)
                    continue;


                var debuff = new debuff_beagsuain();

                if (!target.HasDebuff(debuff.Name))
                {
                    if (sprite is Aisling)
                    {
                        var client = (sprite as Aisling).Client;
                        var action = new ServerFormat1A
                        {
                            Serial = client.Aisling.Serial,
                            Number = 0x81,
                            Speed = 20
                        };

                        client.Aisling.Show(Scope.NearbyAislings, action);
                        {
                            target.ApplyDamage(client.Aisling, 0, false, Skill.Template.Sound);
                            debuff.OnApplied(target, debuff);
                        }
                    }
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
                    client.TrainSkill(Skill);

                    if (client.Aisling.Invisible && Skill.Template.PostQualifers == PostQualifer.BreakInvisible)
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    if (rand.Next(1, 101) >= 10)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                OnSuccess(sprite);
            }
        }
    }
}
