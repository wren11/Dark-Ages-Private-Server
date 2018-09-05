using Darkages.Network.Object;
using System.Collections.Generic;

namespace Darkages.Script.Context
{
    public static class Context
    {
        static Context()
        {
            Items = new Dictionary<string, object>();
            Store = new Dictionary<int, object>();
        }

        public static Dictionary<string, object> Items { get; set; }
        public static Dictionary<int, object> Store { get; set; }

    }
}