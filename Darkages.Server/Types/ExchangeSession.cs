using System.Collections.Generic;

namespace Darkages.Types
{
    /// <summary>
    /// This entire exchange routine was shamelessly copy pasted from Kojasou's Server Project.
    /// (Yes I'm way to lazy to write this myself when it's already been done correctly.)
    /// Credits: https://github.com/kojasou/wewladh
    /// </summary>
    public class ExchangeSession
    {
        public Aisling Trader { get; set; }

        public List<Item> Items { get; set; }

        public int Gold { get; set; }

        public bool Confirmed { get; set; }

        public int Weight { get; set; }

        public ExchangeSession(Aisling user)
        {
            Trader = user;
            Items = new List<Item>();
        }
    }
}
