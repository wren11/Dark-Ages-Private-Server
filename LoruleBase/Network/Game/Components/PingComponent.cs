#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;

#endregion

namespace Darkages.Network.Game.Components
{
    public class PingComponent : GameServerComponent
    {
        public PingComponent(GameServer server)
            : base(server)
        {
            Timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContext.Config.PingInterval));
        }

        public GameServerTimer Timer { get; set; }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Timer.Update(elapsedTime))
                foreach (var client in Server.Clients.Where(client => client != null && client.Aisling != null))
                {
                    client.Send(new ServerFormat3B());
                    client.LastPing = DateTime.UtcNow;
                }
        }
    }
}