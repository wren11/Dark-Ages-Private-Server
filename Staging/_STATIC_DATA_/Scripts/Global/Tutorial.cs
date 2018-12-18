///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Tutorial", "Dean")]
    public class Tutorial : GlobalScript
    {
        private readonly GameClient client;

        public Dictionary<int, Dictionary<string, bool>>
            Flags = new Dictionary<int, Dictionary<string, bool>>();

        public Tutorial(GameClient client) : base(client)
        {
            this.client = client;
        }

        public override void OnDeath(GameClient client, TimeSpan elapsedTime)
        {

        }

        public override void Update(TimeSpan elapsedTime)
        {

            if (!Flags.ContainsKey(client.Aisling.Serial))
            {
                Flags[client.Aisling.Serial] = new Dictionary<string, bool>();
                Flags[client.Aisling.Serial]["t0"] = false;
                Flags[client.Aisling.Serial]["t1"] = false;
                Flags[client.Aisling.Serial]["t2"] = false;
                Flags[client.Aisling.Serial]["t3"] = false;
                Flags[client.Aisling.Serial]["t4"] = false;
                Flags[client.Aisling.Serial]["t5"] = false;
                Flags[client.Aisling.Serial]["t6"] = false;
                Flags[client.Aisling.Serial]["t7"] = false;
                Flags[client.Aisling.Serial]["t8"] = false;
                Flags[client.Aisling.Serial]["t9"] = false;
            }

            if (client != null && client.Aisling != null && client.Aisling.LoggedIn)
            {
                if (client.Aisling.CurrentMapId == 100 && !Flags[client.Aisling.Serial]["t0"])
                {
                    if (client.Aisling.WithinRangeOf(5, 5))
                    {
                        client.SendMessage(0x02, "Huh.. What was that screaming?!, I better see what's going on.");
                        client.SendAnimation(94, client.Aisling, client.Aisling);
                        Flags[client.Aisling.Serial]["t0"] = true;
                    }
                }
                if (client.Aisling.CurrentMapId == 84 && !Flags[client.Aisling.Serial]["t1"])
                {
                    if (client.Aisling.WithinRangeOf(12, 22))
                    {
                        client.SendMessage(0x02, "Where is this place?...");
                        client.SendAnimation(94, client.Aisling, client.Aisling);
                        Flags[client.Aisling.Serial]["t1"] = true;
                    }
                }
                else if (client.Aisling.CurrentMapId == 85 && !Flags[client.Aisling.Serial]["t2"])
                {
                    if (client.Aisling.WithinRangeOf(34, 24))
                    {
                        client.SendMessage(0x02, "These guys all look serious. I Wonder what they will say...");
                        client.SendAnimation(94, client.Aisling, client.Aisling);

                        Flags[client.Aisling.Serial]["t2"] = true;
                    }
                }
                else if (client.Aisling.CurrentMapId == 99 && !Flags[client.Aisling.Serial]["t8"])
                {
                    if (client.Aisling.WithinRangeOf(42, 93))
                    {
                        client.SendMessage(0x02, "A Whore here, Really?");
                        client.SendAnimation(94, client.Aisling, client.Aisling);

                        Flags[client.Aisling.Serial]["t8"] = true;
                    }
                }
                else if (client.Aisling.CurrentMapId == 101 && !Flags[client.Aisling.Serial]["t4"])
                {
                    if (client.Aisling.WithinRangeOf(40, 23))
                    {
                        client.SendMessage(0x02, "Where did all these barron creatures come from?!...");
                        client.SendAnimation(94, client.Aisling, client.Aisling);

                        Flags[client.Aisling.Serial]["t4"] = true;
                    }
                }
                else if (client.Aisling.CurrentMapId == 83)
                {
                    var quest = client.Aisling.Quests.FirstOrDefault(i => i.Name == "macronator_quest");

                    if (client.Aisling.X == 8)
                    {
                        if (quest != null && !quest.Completed)
                        {
                            client.Aisling.X = 11;
                            client.SendMessage(0x02, "You are not ready to enter yet.");
                            client.SendAnimation(94, client.Aisling, client.Aisling);

                            client.Refresh();
                        }
                    }
                }
                else if (client.Aisling.CurrentMapId == ServerContext.Config.StartingMap)
                {
                    var quest = client.Aisling.Quests.FirstOrDefault(i => i.Name == "awakening");

                    if (quest == null)
                    {
                        quest = CreateQuest(quest);
                    }

                    if (!quest.Completed && quest.Started)
                        quest.HandleQuest(client);

                    if (!quest.Completed && client.Aisling.Y >= 11)
                    {
                        quest.Started = true;
                        client.Aisling.Y = 10;
                        client.SendMessage(0x02, "You hear walkers outside. you better find some equipment first.");
                        client.SendAnimation(94, client.Aisling, client.Aisling);
                        client.Refresh();
                    }
                    else
                    {
                        if (client.Aisling.Position.DistanceFrom(1, 2) == 1 && quest.Started)
                        {
                            if (!quest.Completed)
                            {
                                quest.OnCompleted(client.Aisling, true);

                                client.Aisling.Recover();

                                client.SendMessage(0x02, "You pick up your gear from the chest, and begin putting it on.");
                                client.SendAnimation(94, client.Aisling, client.Aisling);
                            }
                        }
                        else if (quest.Completed)
                        {
                            var quest2 = client.Aisling.Quests.FirstOrDefault(i => i.Name == "practice makes perfect");
                            if (quest2 == null)
                            {
                                quest2 = new Quest() { Name = "practice makes perfect" };
                                quest2.ExpRewards = new List<uint>();
                                quest2.ExpRewards.Add(500);
                                quest2.QuestStages = new List<QuestStep<Template>>();
                                quest2.QuestStages.Add(new QuestStep<Template>()
                                {
                                    AcceptedMessage = "Prepare.",
                                    Type = QuestType.Accept,
                                    Prerequisites = new List<QuestRequirement>()
                                       {
                                           new QuestRequirement()
                                           {
                                                Amount = 3,
                                                Type = QuestType.KillCount,
                                                Value = "Rat",
                                           }
                                       },
                                });

                                client.Aisling.Quests.Add(quest2);
                            }
                            else
                            {
                                if (quest2.Completed)
                                {
                                    return;
                                }
                            }

                                quest2.HandleQuest(client);

                            if (!quest2.Completed && client.Aisling.Y >= 11)
                            {
                                quest2.Started = true;
                                client.Aisling.Y = 10;
                                client.SendMessage(0x02, "You need more practice first.");
                                client.SendAnimation(94, client.Aisling, client.Aisling);
                                client.Refresh();
                            }
                            

                            if (!Flags[client.Aisling.Serial]["t9"])
                            {

                                client.SendAnimation(94, client.Aisling, client.Aisling);
                                client.Aisling.Show(Scope.Self, new ServerFormat29(200, 6, 9));
                                Flags[client.Aisling.Serial]["t9"] = true;

                                if (!quest2.Completed)
                                {
                                    if (!Skill.GiveTo(client, "Assail"))
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Quest CreateQuest(Quest quest)
        {
            quest = new Quest { Name = "awakening" };
            quest.LegendRewards.Add(new Legend.LegendItem
            {
                Category = "Event",
                Color = (byte)LegendColor.White,
                Icon = (byte)LegendIcon.Community,
                Value = "A Spiritual Awakening"
            });
            quest.GoldReward = 1000;
            quest.ItemRewards.Add(client.Aisling.Gender == Gender.Male ? "Shirt" : "Blouse");
            quest.ItemRewards.Add("Small Emerald Ring");
            quest.ItemRewards.Add("Small Spinal Ring");
            quest.ItemRewards.Add("Snow Secret");

            client.Aisling.Quests.Add(quest);
            quest.QuestStages = new List<QuestStep<Template>>();

            var q2 = new QuestStep<Template> { Type = QuestType.ItemHandIn };

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.HasItem,
                Amount = 1,
                TemplateContext = ServerContext.GlobalItemTemplateCache["Small Spinal Ring"]
            });

            quest.QuestStages.Add(q2);

            return quest;
        }
    }
}
