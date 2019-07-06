using System.Collections.Generic;

namespace Darkages.Script.Context
{
    public class _Interop
    {
        public delegate bool Call();

        public delegate List<string> ReturnStringListMethod();

        public delegate void VoidMethod();

        public static Dictionary<string, object> Storage { get; } = new Dictionary<string, object>();
    }
}