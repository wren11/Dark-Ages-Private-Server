#region

using Darkages.Types;

#endregion

namespace Darkages.Network.Game
{
    public class ScriptGlobals
    {
        public Sprite actor { get; set; }
        public GameClient client { get; set; }

        public bool result { get; set; }
        public Aisling user { get; set; }
    }
}