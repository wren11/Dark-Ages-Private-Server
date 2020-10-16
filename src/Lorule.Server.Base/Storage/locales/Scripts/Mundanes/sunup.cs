#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("sunup")]
    public class Sunup : MundaneScript
    {
        public Dialog SequenceMenu = new Dialog();

        public Sunup(GameServer server, Mundane mundane) : base(server, mundane)
        {
            Mundane.Template.QuestKey = "sunup_quest";

            SequenceMenu.DisplayImage = (ushort) Mundane.Template.Image;
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText =
                    "Wow, You made it this far! that's impressive. I guess you need to get over this gate huh?"
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Well if you wanna get past this, you need to prove yourself first."
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "?",
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane, "So, what's it gonna be mate?",
                            new OptionsDataItem(0x0010, "Get me over gate you."),
                            new OptionsDataItem(0x0011, "you seriously gonna make me do more shit?"),
                            new OptionsDataItem(0x0012, "fuck off dick head.")
                        );
                }
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = string.Empty,
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane,
                            $"Nice work {sender.Username.ToString()}. Now give me that shit.",
                            new OptionsDataItem(0x0017, "Hand the items to Sunup")
                        );
                }
            });
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            if (client.DlgSession == null)
                client.DlgSession = new DialogSession(client.Aisling, SequenceMenu.Serial)
                {
                    Callback = OnResponse,
                    StateObject = SequenceMenu
                };

            if (client.DlgSession.Serial != SequenceMenu.Serial)
                client.DlgSession = new DialogSession(client.Aisling, SequenceMenu.Serial)
                {
                    Callback = OnResponse,
                    StateObject = SequenceMenu
                };

            if (!client.Aisling.Position.IsNearby(client.DlgSession.SessionPosition))
                return;

            if (!SequenceMenu.CanMoveNext)
                SequenceMenu.SequenceIndex = 0;

            QuestComposite(client);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            var quest = client.Aisling.Quests.FirstOrDefault(i =>
                i.Name == Mundane.Template.QuestKey);

            if (client.DlgSession != null && client.DlgSession.Serial == SequenceMenu.Serial)
                switch (responseID)
                {
                    case 0:
                        SequenceMenu.SequenceIndex = 0;
                        client.DlgSession = null;

                        break;

                    case 1:
                        if (SequenceMenu.CanMoveNext)
                        {
                            SequenceMenu.MoveNext(client);
                            SequenceMenu.Invoke(client);
                        }

                        ;
                        break;

                    case 0x0010:
                        client.SendOptionsDialog(Mundane, "Please go back and find my Sachel that i lost back there.");

                        if (quest != null)
                        {
                            quest.Started = true;
                            quest.TimeStarted = DateTime.UtcNow;
                        }

                        break;

                    case 0x0011:
                        client.SendOptionsDialog(Mundane, "If you help me, I'll get you over this gate.",
                            new OptionsDataItem(0x0010, "Ok sounds like a plan mate."),
                            new OptionsDataItem(0x0012, "fuck you and fuck the gate.")
                        );
                        break;

                    case 0x0012:
                        client.SendOptionsDialog(Mundane,
                            "well stand here with your dick in your hand all day. because you can't leave other wise.");
                        break;

                    case ushort.MaxValue:
                        if (SequenceMenu.CanMoveBack)
                        {
                            var idx = (ushort) (SequenceMenu.SequenceIndex - 1);

                            SequenceMenu.SequenceIndex = idx;
                            client.DlgSession.Sequence = idx;

                            client.Send(new ServerFormat30(client, SequenceMenu));
                        }

                        break;

                    case 0x0015:
                        if (quest != null && !quest.Rewarded && !quest.Completed)
                            quest.OnCompleted(client.Aisling);
                        break;

                    case 0x0016:
                        if (quest != null && !quest.Rewarded && !quest.Completed)
                            quest.OnCompleted(client.Aisling);
                        break;

                    case 0x0017:
                        if (quest != null && !quest.Rewarded && !quest.Completed)
                        {
                            quest.OnCompleted(client.Aisling);
                            client.TransitionToMap(client.Aisling.Map, new Position(17, 20));
                        }

                        break;
                }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }

        private void QuestComposite(GameClient client)
        {
            var quest = client.Aisling.Quests.FirstOrDefault(i => i.Name == Mundane.Template.QuestKey);

            if (quest == null)
            {
                quest = new Quest {Name = Mundane.Template.QuestKey};
                quest.LegendRewards.Add(new Legend.LegendItem
                {
                    Category = "Quest",
                    Color = (byte) LegendColor.Blue,
                    Icon = (byte) LegendIcon.Victory,
                    Value = "Aided Sunup at the Refugee Camp"
                });

                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(10000);
                quest.ExpRewards.Add(15000);
                quest.ExpRewards.Add(15000);
                quest.ExpRewards.Add(15000);
                quest.GoldReward = 10000;

                client.Aisling.Quests.Add(quest);
            }

            quest.QuestStages = new List<QuestStep<Template>>();

            var q1 = new QuestStep<Template> {Type = QuestType.Accept};
            var q2 = new QuestStep<Template> {Type = QuestType.SingleItemHandIn};

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.SingleItemHandIn,
                Amount = 1,
                TemplateContext = ServerContext.GlobalItemTemplateCache["Sunup's Lost Sachel"]
            });

            quest.QuestStages.Add(q1);
            quest.QuestStages.Add(q2);

            if (!quest.Started)
            {
                SequenceMenu.Invoke(client);
            }
            else if (quest.Started && !quest.Completed && !quest.Rewarded)
            {
                client.SendOptionsDialog(Mundane, "Have you found my {=uLost Sachel?");
                quest.HandleQuest(client, SequenceMenu);
            }
            else if (quest.Completed)
            {
                client.TransitionToMap(client.Aisling.Map, new Position(17, 20));
            }
            else
            {
                SequenceMenu.Invoke(client);
            }
        }
    }
}