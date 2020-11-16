#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Benson")]
    public class Benson : MundaneScript
    {
        public override void OnDropped(GameClient client, byte itemSlot)
        { 
            //your logic here
        }

        public Dialog SequenceMenu = new Dialog();

        public Benson(GameServer server, Mundane mundane) : base(server, mundane)
        {
            Mundane.Template.QuestKey = "Benson_quest";

            SequenceMenu.DisplayImage = (ushort) Mundane.Template.Image;
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "See that cunt -  Gos, over there?"
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "go and tell Gos that he's a pussy."
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Interested?",
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane, "Or are you the pussy?",
                            new OptionsDataItem(0x0010, "I'll go tell him"),
                            new OptionsDataItem(0x0011, "no, you are.")
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
                        sender.Client.SendOptionsDialog(Mundane, "haha, what a wanker, here you go.");
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
            if (message.Contains("pussy"))
            {
                Mundane.Show(Scope.NearbyAislings,
                    new ServerFormat0D {Text = "Oh shit!", Type = 0x00, Serial = Mundane.Serial});

                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(400);

                    Mundane.Direction = 2;
                    Mundane.Turn();
                });

                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(1000);
                    Mundane.Show(Scope.NearbyAislings,
                        new ServerFormat0D {Text = "I was just joking!!", Type = 0x00, Serial = Mundane.Serial});
                });
            }
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
                        client.SendOptionsDialog(Mundane, "go on then. what you waiting for?!");

                        if (quest != null)
                        {
                            quest.Started = true;
                            quest.TimeStarted = DateTime.UtcNow;
                        }

                        break;

                    case 0x0011:
                        client.SendOptionsDialog(Mundane, "pussy.");
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
                    Value = "Helped Benson start a fight."
                });

                client.Aisling.Quests.Add(quest);
            }

            quest.QuestStages = new List<QuestStep<Template>>();

            var q1 = new QuestStep<Template> {Type = QuestType.Accept};
            var q2 = new QuestStep<Template> {Type = QuestType.Gossip};

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.Gossip,
                Amount = 1,
                Value = "pussy"
            });

            quest.QuestStages.Add(q1);
            quest.QuestStages.Add(q2);

            if (!quest.Started)
            {
                SequenceMenu.Invoke(client);
            }
            else if (quest.Started && !quest.Completed && !quest.Rewarded)
            {
                client.SendOptionsDialog(Mundane, "Don't think he heard you say. (benson called you a pussy.)");
                quest.HandleQuest(client, SequenceMenu);
            }
            else if (quest.Completed)
            {
                client.SendOptionsDialog(Mundane, "He's lucky I'm feeling sick ay. or i woulda smashed the cunt!");
            }
            else
            {
                SequenceMenu.Invoke(client);
            }
        }
    }
}