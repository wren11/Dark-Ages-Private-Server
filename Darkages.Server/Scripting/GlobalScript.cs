using Darkages.Network.Game;
using Darkages.Network.Object;
using System;

namespace Darkages.Scripting
{
    public abstract class GlobalScript : ObjectManager
    {
        private GameClient client;

        public GlobalScript(GameClient client)
        {
            this.client = client;
        }

        public abstract void OnDeath(GameClient client, TimeSpan elapsedTime);
        public abstract void Update(TimeSpan elapsedTime);
    }
}