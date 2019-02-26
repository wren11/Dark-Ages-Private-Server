using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Tut", author: "Dean")]
    public class Tut : GlobalScript
    {
        Reactor reactor;
    
        public Tut(GameClient client) : base(client)
        {
            Timer = new GameServerTimer(TimeSpan.FromMilliseconds(200));

            CreateReactors();
        }



        public override void Run(GameClient client)
        {
            if (client.Aisling.CurrentMapId == ServerContext.Config.StartingMap
                    && client.Aisling.X == 26 && client.Aisling.Y == 47)
            {
                if (client.Aisling.ActiveReactor == null)
                {
                    client.Aisling.ActiveReactor = Clone<Reactor>(reactor);
                    client.Aisling.ActiveReactor.Sequences = new List<DialogSequence>(reactor.Sequences);
                    client.Aisling.ActiveReactor.Decorator = ScriptManager.Load<ReactorScript>("Default Response Handler", reactor);
                    client.Aisling.ActiveReactor.Location = new Position(reactor.Location.X, reactor.Location.Y);
                }

                client.Aisling.ActiveReactor.Update(client);
            }
        }

        private void CreateReactors()
        {
            reactor = new Reactor()
            {
                Name = "Welcome",
                Location = new Position(26, 47),
                CanActAgain = false,
                Sequences = new List<DialogSequence>()
                        {
                            new DialogSequence()
                            {
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "Welcome to Dark Ages : Online Roleplaying. This tutorial will give yuou the facts and skills you need to begin.",
                                 Title = "",
                            },
                            new DialogSequence()
                            {
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "You will gain 5000 Experience, a dirk and 1000 coins.",
                                 Title = "",
                            },
                            new DialogSequence()
                            {
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "The first thing you should do in this tutorial is talk to all the Merchants, There are five of them. When you are ready to leave, You can leave the tutorial by continuing to the end of this path.",
                                 Title = "",
                            },
                            new DialogSequence()
                            {
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "to move, right click the mouse to the position of where you want to move. in tight spots, you can use the arrow keys on your keyboard. Talk to the knightnexttothe tree by double-clicking him, if you get lost, press <Tab>.",
                                 Title = "",
                            },
                            new DialogSequence()
                            {
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "You should have a piece of clothing in your inventory. Press the <a> key to access your inventory. Double click on the piece of clothing to wear it.",
                                 Title = "",
                            },
                            new DialogSequence()
                            {
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = 0x0000,
                                 DisplayText = "You may skip this tutorial by going up this path and then turn right at the next path.",
                                 Title = ""
                            },
                        },
                Quest = new Quest()
                {
                    Name = "Tut 1, Start",
                    QuestStages = new List<QuestStep<Template>>()
                    {
                        new QuestStep<Template>()
                        {
                             Type = QuestType.Gossip,
                        },
                    },
                    ExpRewards = new List<uint>()
                     {
                          500,500,1000,1000,1000
                     },
                    ItemRewards = new List<string>()
                     {
                         "Dirk",
                          Client.Aisling.Gender == Gender.Male ? "Shirt" : "Blouse",
                     },
                    GoldReward = 1000,
                }
            };
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Timer != null)
            {
                Timer.Update(elapsedTime);

                if (Timer.Elapsed)
                {
                    Run(Client);
                    Timer.Reset();
                }
            }
        }
    }
}
