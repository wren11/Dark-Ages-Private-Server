#region

using Darkages.Network.Object;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

#endregion

namespace Darkages.Network.Game
{
    public enum UpdateType
    {
        Async,
        Sync
    }

    public abstract class GameServerComponent : ObjectManager
    {
        public GameServerComponent(GameServer server)
        {
            Server = server;
        }

        [JsonIgnore] public GameServer Server { get; }

        public abstract UpdateType UpdateMethodType { get; }

        public abstract Task Update(TimeSpan elapsedTime);
    }
}