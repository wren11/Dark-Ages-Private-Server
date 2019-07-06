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

using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Inspect Item", "Dean")]
    public class Inspect : SkillScript
    {
        public Inspect(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;
                client.SystemMessage("Failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;
                var itemFirstSlot = aisling.Inventory.Has(i => i.Slot == 1);

                if (itemFirstSlot != null)
                {
                    itemFirstSlot.Identifed = true;
                    {
                        client.SystemMessage(string.Format("Success! Item is {0}", itemFirstSlot.DisplayName));
                    }
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;

                if (client != null && Skill.CanUse())
                {
                    client.TrainSkill(Skill);
                    OnSuccess(sprite);
                }
                else
                {
                    OnFailed(sprite);
                }
            }
        }
    }
}