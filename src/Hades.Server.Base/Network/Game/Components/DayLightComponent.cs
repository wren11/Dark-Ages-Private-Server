using System;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;

namespace Darkages.Server.Network.Game.Components
{
    public class DayLightComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;
        private byte _shade = 0;

        public DayLightComponent(GameServer server)
            : base(server)
        { 
            _timer = new GameServerTimer(TimeSpan.FromSeconds(20.0f));
        }

        protected internal override void Update(TimeSpan elapsedTime)
        {
            if (_timer.Update(elapsedTime))
            {
                var format20 = new ServerFormat20 {Shade = _shade};

                lock (Server.Clients)
                {
                    foreach (var client in Server.Clients)
                    {
                        client?.Send(format20);
                    }
                }

                _shade += 1;
                _shade %= 18;
            }
        }
    }
}