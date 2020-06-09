///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Locate Monster", "Test")]
    public class LocateMonster : SkillScript
    {
        public LocateMonster(Skill skill) : base(skill)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var objects = aisling.GetObjects(null, i => true, Get.All);
                var sb = new StringBuilder();

                foreach (var obj in objects)
                {
                    sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5}", obj.Position.X, obj.Position.Y, obj.Map.Name, obj.CurrentMapId,
                        obj.Direction, obj.EntityType));
                }

                File.WriteAllText("objdump.txt", sb.ToString());
            }
        }

        public override void OnFailed(Sprite sprite)
        {
        }

        public override void OnSuccess(Sprite sprite)
        {
        }
    }
}