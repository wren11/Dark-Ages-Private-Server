#region

using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class ExchangeSession
    {
        public ExchangeSession(Aisling user)
        {
            Trader = user;
            Items = new List<Item>();
        }

        public Aisling Trader { get; set; }

        public List<Item> Items { get; set; }

        public int Gold { get; set; }

        public bool Confirmed { get; set; }

        public int Weight { get; set; }
    }
}