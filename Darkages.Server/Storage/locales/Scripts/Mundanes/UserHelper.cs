using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System.Collections.Generic;

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
            var options = new List<OptionsDataItem>();
            options.Add(new OptionsDataItem(0x0001, "Return Home."));
            if (client.Aisling.Path == Class.Peasant)
            {
                options.Add(new OptionsDataItem(0x0002, "Skip Tutorial."));
            }
            client.SendOptionsDialog(Mundane, "What do you need?", options.ToArray());
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
                    }
                    break;
                case 0x0002:
                    {
                        client.Aisling.TutorialCompleted = true;
                        client.Aisling.ExpLevel = 11;
                        client.Aisling._Str = ServerContext.Config.BaseStatAttribute;
                        client.Aisling._Int = ServerContext.Config.BaseStatAttribute;
                        client.Aisling._Wis = ServerContext.Config.BaseStatAttribute;
                        client.Aisling._Con = ServerContext.Config.BaseStatAttribute;
                        client.Aisling._Dex = ServerContext.Config.BaseStatAttribute;
                        client.Aisling._MaximumHp = (ServerContext.Config.MinimumHp + 33) * 11;
                        client.Aisling._MaximumMp = (ServerContext.Config.MinimumHp + 21) * 11;

                        client.Aisling.StatPoints = 11 * ServerContext.Config.StatsPerLevel;
                        client.SendStats(StatusFlags.All);

                        client.SendMessage(0x02, "You have lost all memory...");
                        client.TransitionToMap(ServerContext.GlobalMapCache[85], new Position(35, 23));
                    }
                    break;
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