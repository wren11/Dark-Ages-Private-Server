#region

using System.Collections.Generic;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Erin's Script")]
    public class Erin : MundaneScript
    {
        public Reactor Actor;

        public Erin(GameServer server, Mundane mundane) : base(server, mundane)
        {
            if (string.IsNullOrEmpty(mundane.Template.QuestKey))
                mundane.Template.QuestKey = "Monk: Beginner Equipment";
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var proceed = client != null && client.Aisling != null && client.Aisling.WithinRangeOf(Mundane);

            if (!proceed) return;

            if (client.Aisling.Path != Class.Monk)
            {
                client.SendOptionsDialog(Mundane, "We have no business together.");
                return;
            }

            Actor = new Reactor
            {
                CallBackScriptKey = Mundane.Template.ScriptKey,
                CallerType = ReactorQualifer.Reactor,
                ScriptKey = "Default Response Handler",
                CallingReactor = "Default Response Handler",
                Name = "Erin Part 1",

                Quest = new Quest
                {
                    Name = Mundane.Template.QuestKey,
                    Completed = false,
                    ItemRewards = new List<string>
                    {
                        "Wolf Earrings"
                    },
                    LegendRewards = new List<Legend.LegendItem>
                    {
                        new Legend.LegendItem
                        {
                            Category = "Class",
                            Icon = (byte) LegendIcon.Monk,
                            Color = (byte) LegendColor.Blue,
                            Value = "Met Erin and Became Friends."
                        }
                    },
                    ExpRewards = new List<uint>
                    {
                        1000, 2000, 3000, 4000, 6000
                    },
                    GoldReward = 5000,
                    QuestStages = new List<QuestStep<Template>>
                    {
                        new QuestStep<Template>
                        {
                            Prerequisites = new List<QuestRequirement>
                            {
                                new QuestRequirement
                                {
                                    Type = QuestType.ItemHandIn,
                                    Amount = 2,
                                    TemplateContext = ServerContext.GlobalItemTemplateCache["Wolf's Teeth"],
                                    Value = "Wolf's Teeth"
                                },
                                new QuestRequirement
                                {
                                    Type = QuestType.ItemHandIn,
                                    Amount = 1,
                                    TemplateContext = ServerContext.GlobalItemTemplateCache["Silver Earrings"],
                                    Value = "Silver Earrings"
                                }
                            }
                        }
                    }
                }
            };

            Actor.Sequences = new List<DialogSequence>
            {
                new DialogSequence
                {
                    HasOptions = false,
                    CanMoveBack = false,
                    CanMoveNext = true,
                    DisplayText =
                        "Ah... a new Monk. You have a difficult path ahead of you. You think you're going to get far with that equipment?",
                    DisplayImage = (ushort) Mundane.Template.Image,
                    Title = "Erin"
                },
                new DialogSequence
                {
                    HasOptions = true,
                    CanMoveBack = true,
                    CanMoveNext = true,
                    DisplayImage = (ushort) Mundane.Template.Image,
                    Title = "Erin",
                    DisplayText = "I can help you make something a little more violent, but i will need a few things.",
                    OnSequenceStep = (lpAisling, lpSequence) =>
                    {
                        lpSequence.OnSequenceStep = QuestCompleted;

                        if (lpSequence.HasOptions)
                        {
                            var subject = lpAisling.Quests.Find(i => i.Name == Mundane.Template.QuestKey);

                            if (subject == null)
                                lpAisling.Client.SendOptionsDialog(Mundane, "Are you Interested?",
                                    new OptionsDataItem(0x002A, "Yes i need your help"),
                                    new OptionsDataItem(0x001B, "No thanks, i'll be fine")
                                );
                            else if (subject != null && !subject.Completed)
                                Actor.Quest.HandleQuest(lpAisling.Client, null, quest_completed_ok =>
                                {
                                    if (quest_completed_ok)
                                        lpAisling.Client.SendOptionsDialog(Mundane,
                                            "I see you have everything. Let me get to work on this.",
                                            new OptionsDataItem(0x004A,
                                                "Put a hole here... put the silk through like so...")
                                        );
                                    else
                                        lpAisling.Client.SendOptionsDialog(Mundane,
                                            "Return to me when you have the items. I need 2 pieces of Wolf's Teeth, 1 Silver Earrings, and 2 Pieces of Spider's Silk.");
                                });
                            else if (subject != null && subject.Completed)
                                lpAisling.Client.SendOptionsDialog(Mundane,
                                    "Thanks again. I hope you enjoy those Wolf Earrings.");
                        }
                    }
                }
            };

            var lpsubject = client.Aisling.Quests.Find(i => i.Name == Mundane.Template.QuestKey);

            if (lpsubject != null && lpsubject.Completed)
            {
                client.SendOptionsDialog(Mundane, "Thanks again. I hope you enjoy those Wolf Earrings.");
                return;
            }

            Actor.Decorators = ScriptManager.Load<ReactorScript>(Actor.ScriptKey, Actor);
            if (Actor != null && Actor.Decorators != null)
                foreach (var script in Actor.Decorators.Values)
                    script.OnTriggered(client.Aisling);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x002A:
                {
                    if (!client.Aisling.AcceptQuest(Actor.Quest)) return;

                    client.SendOptionsDialog(Mundane,
                        "Alright now we're talking. I am going to need some materials. Bring me 2 pieces of Wolf's Teeth, 1 Silver Earrings, and 2 Pieces of Spider's Silk. I will do the rest.",
                        new OptionsDataItem(0x006A, "Continue"));
                }
                    break;

                case 0x006A:
                {
                    client.SendOptionsDialog(Mundane,
                        "You can find Wolf's Teeth from the Wolves in the Woodlands,\nsome Spider's Silk from the Spiders here in Mileth Crypt,\nand the Silver Earrings from the Armory in Mileth. Come back when you have the items.");
                }
                    break;

                case 0x001B:
                {
                    client.Aisling.DestroyReactor(Actor);
                    client.SendOptionsDialog(Mundane, "So be it. Goodluck Aisling.");
                }
                    break;

                case 0x004A:
                {
                    foreach (var script in Actor.Decorators.Values)
                        script.OnTriggered(client.Aisling);
                }
                    break;
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }

        private void QuestCompleted(Aisling aisling, DialogSequence b)
        {
            aisling.Client.SendOptionsDialog(Mundane,
                "Ah there we go, splendid! this should be useful to you for a while. Come back again another time, i might be able to make you something even better.");

            var subject = aisling.Quests.Find(i => i.Name == Mundane.Template.QuestKey);

            if (subject != null)
            {
                Actor.Quest.OnCompleted(aisling);
                aisling.DestroyReactor(Actor);
                aisling.Client.CloseDialog();

                subject.Completed = true;
            }
        }
    }
}