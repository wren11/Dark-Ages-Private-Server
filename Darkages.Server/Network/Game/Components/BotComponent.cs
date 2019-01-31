using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darkages.Network.Game.Components
{
    public class BotComponent : GameServerComponent
    {
        public GameServerTimer Timer = new GameServerTimer(TimeSpan.FromMilliseconds(450));

        public BotComponent(GameServer server) : base(server)
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                UpdateBots();

                Timer.Reset();
            }
        }

        private void UpdateBots()
        {
            lock (Server.Clients)
            {

                foreach (var client in Server.Clients.Where(i => i != null && i.Aisling != null && i.Aisling.LoggedIn && i.Aisling.IsBot))
                {
                    var Aisling = client.Aisling;
                    if (Aisling != null)
                    {
                        if (Aisling.IsBot)
                        {

                            if (Aisling.CanWalk())
                            {
                                if (Aisling.AreaID != 1001)
                                {
                                    //client.Say("See you in map 101. :)");
                                    //client.TransitionToMap(1001, new Position(50, 50));
                                }

                                if ((DateTime.UtcNow - Aisling.LastBotUpdate).TotalMilliseconds > 560)
                                {
                                    Aisling.Wander();
                                    Aisling.Assail();

                                    Aisling.LastBotUpdate = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
