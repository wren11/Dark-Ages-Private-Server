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
