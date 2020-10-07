#region

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

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
            var options = new List<OptionsDataItem>
            {
                new OptionsDataItem(0x0001, "Return Home."),
                new OptionsDataItem(0x0005, "World Shout!")
            };
            if (!client.Aisling.TutorialCompleted) options.Add(new OptionsDataItem(0x0003, "Skip Tutorial."));
            client.SendOptionsDialog(Mundane, "What do you need?", options.ToArray());

            if (client.DlgSession == null)
                client.DlgSession = new DialogSession(client.Aisling, Mundane.Serial)
                {
                    Callback = OnResponse
                };
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (!string.IsNullOrEmpty(args))
            {
                var shoutArgs = args;

                foreach (var m in GetObjects<Aisling>(null, n => n.LoggedIn && n.Serial != client.Serial))
                    m.Client.SystemMessage("{=a" + $"{client.Aisling}: {shoutArgs}");

                client.CloseDialog();
            }
            else
            {
                switch (responseID)
                {
                    case 0x0001:
                    {
                        if (client.Aisling.TutorialCompleted)
                            client.Aisling.GoHome();
                        else
                            client.TransitionToMap(
                                ServerContext.GlobalMapCache[ServerContext.Config.StartingMap],
                                ServerContext.Config.StartingPosition);
                    }
                        break;

                    case 0x0002:
                    {
                        if (client.Aisling.Stage == ClassStage.Master)
                        {
                            client.SendOptionsDialog(Mundane, "You are a master already.");
                            return;
                        }

                        client.Aisling.Path = Class.Warrior;
                        client.Aisling.Stage = ClassStage.Master;
                        client.Aisling.ExpLevel = 99;
                        client.Aisling._Str = 215;
                        client.Aisling._Wis = 100;
                        client.Aisling._Int = 100;
                        client.Aisling._Con = 180;
                        client.Aisling._Dex = 150;
                        client.Aisling._MaximumHp = 20000;
                        client.Aisling._MaximumMp = 10000;

                        Item.Create(client.Aisling, "War Mantle").GiveTo(client.Aisling);
                        Item.Create(client.Aisling, "War Helmet").GiveTo(client.Aisling);

                        client.SendStats(StatusFlags.All);
                        client.CloseDialog();
                    }
                        break;

                    case 0x0003:
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
                        client.TransitionToMap(1006, new Position(2, 4));
                        client.Aisling.TutorialCompleted = true;
                    }
                        break;

                    case 0x0005:
                    {
                        client.Send(new ReactorInputSequence(Mundane, "What do you want to shout?", 40));
                    }
                        break;
                }
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}