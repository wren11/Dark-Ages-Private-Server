#region

using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;

#endregion

namespace Darkages.Network.Game.Components
{
    public class SaveComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public SaveComponent(GameServer server) : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromSeconds(45));
        }

        protected internal override void Update(TimeSpan elapsedTime)
        {
            if (_timer.Update(elapsedTime))
            {
                ServerContext.SaveCommunityAssets();

                if (ServerContext.Game != null)
                    if (ServerContext.Game.Clients != null)
                        foreach (var client in ServerContext.Game.Clients)
                            client?.Save();
            }
        }
    }
}