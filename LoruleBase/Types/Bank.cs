#region

using Darkages.Network.Game;
using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class Bank
    {
        public Bank()
        {
            Items = new Dictionary<string, int>();
        }

        public Dictionary<string, int> Items { get; set; }

        public void Deposit(Item lpItem)
        {
            if (!Items.ContainsKey(lpItem.DisplayName))
                Items[lpItem.DisplayName] = 1;
            else Items[lpItem.DisplayName]++;
        }

        public bool Withdraw(GameClient client, string itemName)
        {
            if (ServerContextBase.GlobalItemTemplateCache.ContainsKey(itemName))
            {
                var template = ServerContextBase.GlobalItemTemplateCache[itemName];
                var item = Item.Create(client.Aisling, template);

                if (item.GiveTo(client.Aisling))
                {
                    if (Items[itemName] - 1 <= 0)
                        Items.Remove(itemName);
                    else
                        Items[itemName]--;

                    return true;
                }
            }

            return false;
        }
    }
}