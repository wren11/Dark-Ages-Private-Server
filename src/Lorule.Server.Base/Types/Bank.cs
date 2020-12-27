#region

using System.Collections.Generic;
using Darkages.Network.Game;

#endregion

namespace Darkages.Types
{
    public class Bank
    {
        public Bank()
        {
            Items = new Dictionary<string, Stack<Item>>();
        }

        public Dictionary<string, Stack<Item>> Items { get; set; }

        public void Deposit(Item lpItem)
        {
            if (!Items.ContainsKey(lpItem.DisplayName))
            {
                Items[lpItem.DisplayName] = new Stack<Item>();
            }

            Items[lpItem.DisplayName].Push(lpItem);
        }

        public bool Withdraw(IGameClient client, string itemName)
        {
            if (!Items.ContainsKey(itemName))
                return false;

            var itemObj = Items[itemName].Pop();
            return itemObj?.GiveTo(client.Aisling) ?? false;

        }
    }
}