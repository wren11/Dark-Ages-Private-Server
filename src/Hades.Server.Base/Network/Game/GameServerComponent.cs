#region

using System;
using Newtonsoft.Json;
using Darkages.Network.Object;


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