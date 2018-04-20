using Darkages.Network.Game;
using Darkages.Scripting;
using System;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Tower Defender Player Reaper")]
    public class TowerDefenders : GlobalScript
    {
        GameClient Client;

        public TowerDefenders(GameClient client) : base(client)
        {
            Client = client;
        }

        public override void OnDeath(GameClient client, TimeSpan elapsedTime)
        {
            client.SendMessage(0x02, "Looks like you failed bud.");
            {
                client.Revive();
                client.Aisling.GoHome();
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Client != null && Client.Aisling != null && Client.Aisling.LoggedIn)
            {
                if (Client.Aisling.AreaID == 510 && Client.Aisling.Dead)
                    OnDeath(Client, elapsedTime);
            }
        }
    }
}
