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
using System.Collections.Generic;

namespace Darkages.Types
{
    public class Bank
    {
        public Dictionary<string, int> Items { get; set; }

        public void Deposit(Item lpItem)
        {
            if (!Items.ContainsKey(lpItem.DisplayName))
                Items[lpItem.DisplayName] = 1;
            else Items[lpItem.DisplayName]++;
        }

        public bool Withdraw(GameClient client, string itemName)
        {
            if (ServerContext.GlobalItemTemplateCache.ContainsKey(itemName))
            {
                var template = ServerContext.GlobalItemTemplateCache[itemName];
                var item = Item.Create(client.Aisling, template);

                if (item.GiveTo(client.Aisling, true))
                {
                    if (Items[itemName] - 1 <= 0)
                    {
                        Items.Remove(itemName);
                    }
                    else
                    {
                        Items[itemName]--;
                    }

                    return true;
                }
            }

            return false;
        }

        public Bank()
        {
            Items = new Dictionary<string, int>();
        }

    }
}
