#region

using Darkages.Network.ServerFormats;
using System;
using System.Linq;
using Darkages.Types;

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

            if (Server.Clients == null) return;

            foreach (var client in Server.Clients.Where(client => client != null))
            {
                if (client.Aisling != null && !client.Aisling.LoggedIn)
                {
                    continue;
                }

                try
                {
                    client.Send(
                        client.Aisling?.Map != null && client.Aisling.Map.Flags.HasFlag(MapFlags.HasDayNight)
                            ? format20
                            : new ServerFormat20());
                }
                catch
                {
                    // ignored
                }
            }

            _shade += 1;
            _shade %= 18;
        }
    }
}