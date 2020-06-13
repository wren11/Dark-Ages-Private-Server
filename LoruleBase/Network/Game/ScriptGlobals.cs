#region

using Darkages.Types;

#endregion

namespace Darkages.Network.Game
{
    public class ScriptGlobals
    {
        public GameClient client { get; set; }

        public Aisling user { get; set; }

        public Sprite actor { get; set; }

        public bool result { get; set; }
    }
}