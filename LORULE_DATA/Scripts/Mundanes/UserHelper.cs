using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("User Helper Menu")]
    public class UserHelper : MundaneScript
    {
        public UserHelper(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
                client.SendOptionsDialog(Mundane, "What do you need?",
                    new OptionsDataItem(0x0001, "Return Home."));
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0001:
                    {
                        if (client.Aisling.Path != Class.Peasant)
                        {
                            client.Aisling.GoHome();
                        }
                        else
                        {
                            client.TransitionToMap(ServerContext.GlobalMapCache[ServerContext.Config.StartingMap], ServerContext.Config.StartingPosition);
                        }
                    } break;
            }
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}