#region

using Darkages.Network.ServerFormats;
using System;
using System.Linq;

#endregion

namespace Darkages.Network.Game.Components
{
    public class DaytimeComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;
        private byte _shade;

        public DaytimeComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContextBase.Config.DayTimeInterval));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (!_timer.Elapsed)
                return;

            _timer.Reset();

            var format20 = new ServerFormat20 { Shade = _shade };

            if (Server.Clients != null)
                foreach (var client in Server.Clients.Where(client => client != null))
                    client.Send(format20);

            _shade += 1;
            _shade %= 18;
        }
    }
}