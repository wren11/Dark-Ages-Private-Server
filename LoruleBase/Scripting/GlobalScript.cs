#region

using Darkages.Network.Game;
using Darkages.Network.Object;
using System;

#endregion

namespace Darkages.Scripting
{
    public abstract class GlobalScript : ObjectManager
    {
        public GameClient Client;

        public GlobalScript(GameClient client)
        {
            Client = client;
        }

        public GameServerTimer Timer { get; set; }

        public abstract void Update(TimeSpan elapsedTime);
    }
}