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
using System;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Cursed Sachel")]
    public class Sachel : ItemScript
    {
        public Sachel(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {

        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var name = Item.Template.Name.Replace("'s Lost Sachel.", string.Empty);

                if (name == string.Empty)
                {
                    client.SendMessage(0x02, ServerContext.Config.CantUseThat);
                    return;
                }

                if (name.Trim().Equals(client.Aisling.Username, StringComparison.OrdinalIgnoreCase))
                {
                    if (client.Aisling.Remains.ReaperBag != null)
                    {
                        client.Aisling.Remains.RecoverItems(client.Aisling);
                    }
                }
                else
                {
                    sprite._MaximumHp += 50;
                    client.SendStats(StatusFlags.All);

                    client.SendMessage(Scope.All, 0x02,
                        string.Format("{0} broke open {1}'s Cursed Sachel. (Granted +50 hp!)", client.Aisling.Username, name));
                }
            }
        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {

        }
    }
}
