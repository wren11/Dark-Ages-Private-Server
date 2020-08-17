using System;
using System.Linq;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Network.Game.Components
{
    public class AfkComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public AfkComponent(GameServer server) : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromSeconds(6));
        }

        public void Pulse(GameClient client)
        {
            client.Aisling?.Show(Scope.NearbyAislings,
                new ServerFormat1A(client.Aisling.Serial, 16, 20));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                if (ServerContextBase.Game != null)
                    if (ServerContextBase.Game.Clients != null)
                        foreach (var client in from client in ServerContextBase.Game.Clients
                                               where client != null

                                               let afk = (DateTime.UtcNow - client.LastMovement).TotalMinutes > 3
                                                         && (DateTime.UtcNow - client.LastClientRefresh).TotalMinutes > 3
                                               where afk
                                               select client)
                        {
                            Pulse(client);
                        }

                _timer.Reset();
            }
        }
    }
}