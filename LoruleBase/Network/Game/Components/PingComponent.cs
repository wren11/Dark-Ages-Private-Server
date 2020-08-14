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
        public override UpdateType UpdateMethodType => UpdateType.Sync;

        public override Task Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                foreach (var client in Server.Clients.Where(client => client != null &&
                                                                      client.Aisling != null))
                {
                    client.Send(new ServerFormat3B());
                    client.LastPing = DateTime.UtcNow;
                }

                Timer.Reset();
            }

            return Task.CompletedTask;
        }
    }
}