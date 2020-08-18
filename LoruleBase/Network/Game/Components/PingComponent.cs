#region

using Darkages.Network.ServerFormats;
using System;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Darkages.Network.Game.Components
{
    public class PingComponent : GameServerComponent
    {
        public PingComponent(GameServer server)
            : base(server)
        {
            Timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContextBase.Config.PingInterval));
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