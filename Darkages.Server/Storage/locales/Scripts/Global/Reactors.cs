using Darkages.Network.Game;
using Darkages.Scripting;
using System;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Reactors")]
    public class Reactors : GlobalScript
    {
        GameClient Client;
        public Reactors(GameClient client) : base(client)
        {
            Client = client;
        }

        public override void OnDeath(GameClient client, TimeSpan elapsedTime)
        {

        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Client == null)
                return;
            if (Client.IsRefreshing)
                return;
                   
            if (Client.Aisling != null && Client.Aisling.LoggedIn)
            {
                if (!Client.Aisling.Map.Ready)
                    return;

                EastWoodlands();

            }
        }

        private void EastWoodlands()
        {
            if (Client.Aisling.CurrentMapId == 300 && Client.Aisling.Y > 1 && Client.Aisling.Y < 3)
            {
                Client.SendMessage(0x02, "* East Woodlands *\n\nThis zone is governed by law. A guard has let you pass, this time.");
                Client.TransitionToMap(Client.Aisling.CurrentMapId, new Types.Position(Client.Aisling.X, 5));
            }
        }
    }
}
