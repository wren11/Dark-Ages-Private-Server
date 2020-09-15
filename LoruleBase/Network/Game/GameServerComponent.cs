#region

using System;
using Darkages.Network.Object;
using Newtonsoft.Json;

#endregion

namespace Darkages.Network.Game
{
    public abstract class GameServerComponent : ObjectManager
    {
        protected GameServerComponent(GameServer server)
        {
            Server = server;
        }

        [JsonIgnore] public GameServer Server { get; }

        protected internal abstract void Update(TimeSpan elapsedTime);
    }
}