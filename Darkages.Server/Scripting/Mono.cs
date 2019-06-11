using System.Collections.Generic;

namespace Darkages.Script.Context
{
    public class _Interop
    {
        private static Dictionary<string, object> _vars = new Dictionary<string, object>();

        public delegate List<string> ReturnStringListMethod();

        public delegate void VoidMethod();

        public delegate bool Call();

        public static Dictionary<string, object> Storage
        {
            get
            {
                return _vars;
            }
        }
    }
}