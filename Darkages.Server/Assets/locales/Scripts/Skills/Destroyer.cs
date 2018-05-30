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
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using System.Threading.Tasks;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Destroyer", "Dean")]
    public class Destroyer : SkillScript
    {
        public Skill _skill;
        public Sprite Target;

        public Destroyer(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (Target != null)
                    client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29(Skill.Template.MissAnimation, (ushort)Target.X, (ushort)Target.Y));
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null)
                    DestroyAll(client);
            }
        }

        public void DestroyAll(GameClient client)
        {
            new TaskFactory().StartNew(() =>
            {
                var objects = GetObjects(i => i.WithinRangeOf(client.Aisling), Get.Monsters);

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = 0x02,
                    Speed = 40
                };

                client.Aisling.Show(Scope.NearbyAislings, action);

                foreach (var obj in objects)
                {
                    (obj as Monster).Target = client.Aisling;
                    (obj as Monster).GenerateRewards(client.Aisling);
                    client.SendAnimation(301, obj, client.Aisling);

                    obj.ApplyDamage(client.Aisling, 999999, false, 40);
                }
            });
        }
    }
}
