using Darkages.Network.ServerFormats;
using System;

namespace Darkages.Network.Game.Components
{
    public class DaytimeComponent : GameServerComponent
    {
        private readonly GameServerTimer timer;
        private byte shade;

        public DaytimeComponent(GameServer server)
            : base(server)
        {
            timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContext.Config.DayTimeInterval));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            timer.Update(elapsedTime);

            if (timer.Elapsed)
            {
                timer.Reset();

                var format20 = new ServerFormat20 { Shade = shade };

                foreach (var client in Server.Clients)
                {
                    if (client != null)
                        client.Send(format20);
                }

                shade += 1;
                shade %= 18;
            }
        }
    }
}