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

        public bool Confirmed { get; set; }
        public int Gold { get; set; }
        public List<Item> Items { get; set; }
        public Aisling Trader { get; set; }
        public int Weight { get; set; }
    }
}