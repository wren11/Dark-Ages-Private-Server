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
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Mileth Scroll", "Dean")]
    public class milethscroll : ItemScript
    {
        public milethscroll(Item item) : base(item)
        {

        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {

        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {

        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite is Aisling)
            {
                if (!ServerContext.GlobalWarpTemplateCache.ContainsKey(500) || ServerContext.GlobalWarpTemplateCache[500].Count == 0)
                {
                    (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, "You can't travel there now.");
                    return;
                }

                (sprite as Aisling).Client.WarpTo(ServerContext.GlobalWarpTemplateCache[509][0]);
                (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, "You have scrolled to mileth.");
            }
        }
    }
}
