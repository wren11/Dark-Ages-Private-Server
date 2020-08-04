#region

using Darkages.Network.ServerFormats;
using System;
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
                Timer.Reset();

                foreach (var client in Server.Clients)
                    if (client != null &&
                        client.Aisling != null)
                    {
                        client.Send(new ServerFormat3B());
                        client.LastPing = DateTime.UtcNow;
                    }
            }

            return Task.CompletedTask;
        }
    }
}