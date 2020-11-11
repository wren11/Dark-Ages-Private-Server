#region

using System;
using Darkages.Network.Game;
using Darkages.Network.Object;

#endregion

namespace Darkages.Scripting
{
    public abstract class GlobalScript : ObjectManager, IScriptBase
    {
        public GameClient Client;

        protected GlobalScript(GameClient client)
        {
            Client = client;
        }

        public GameServerTimer Timer { get; set; }

        public abstract void Update(TimeSpan elapsedTime);
    }
}