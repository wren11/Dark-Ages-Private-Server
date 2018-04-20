using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class MundaneScript : ObjectManager
    {
        public MundaneScript(GameServer server, Mundane mundane)
        {
            Server = server;
            Mundane = mundane;
        }

        public GameServer Server { get; set; }
        public Mundane Mundane { get; set; }

        public abstract void OnClick(GameServer server, GameClient client);
        public abstract void OnResponse(GameServer server, GameClient client, ushort responseID, string args);
        public abstract void OnGossip(GameServer server, GameClient client, string message);
        public abstract void TargetAcquired(Sprite Target);
    }
}