using Darkages.Network.Game;

namespace Darkages.Types
{
    public class Redirect
    {
        public int ID;
        public string Name;
        public byte[] Salt;
        public byte Seed;
        public int Serial;

        public int Type { get; internal set; }
        public GameClient Client { get; internal set; }
    }
}