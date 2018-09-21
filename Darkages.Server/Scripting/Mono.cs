using System.Collections.Generic;

namespace Darkages.Script.Context
{
    public static class Context
    {
        static Context()
        {
            Items = new Dictionary<string, object>();
        }

        public static Dictionary<string, object> Items { get; set; }
    }
}