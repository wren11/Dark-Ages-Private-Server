#region

using System;
using System.Linq;
using Darkages.Server.Network.WS;
using Darkages.Storage;
using Newtonsoft.Json;

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
            if (!_timer.Update(elapsedTime))
                return;

            if (ServerContext.Game == null || ServerContext.Game.Clients == null)
                return;

            ServerContext.SaveCommunityAssets();

            foreach (var client in ServerContext.Game.Clients.Where(client => client?.Aisling != null))
            {
                client.Save();
            }
        }
    }
}