#region

using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Types;

#endregion

namespace Darkages.Scripting
{
    public abstract class MundaneScript : ObjectManager, IScriptBase
    {
        protected MundaneScript(GameServer server, Mundane mundane)
        {
            Server = server;
            Mundane = mundane;
        }

        public Mundane Mundane { get; set; }

        public GameServer Server { get; set; }

        public abstract void OnClick(GameServer server, GameClient client);

        public abstract void OnGossip(GameServer server, GameClient client, string message);

        public abstract void OnResponse(GameServer server, GameClient client, ushort responseId, string args);

        public abstract void TargetAcquired(Sprite target);
    }
}