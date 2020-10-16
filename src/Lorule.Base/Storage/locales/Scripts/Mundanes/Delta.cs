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
    [Script("Delta")]
    public class Delta : MundaneScript
    {
        public Dialog SequenceMenu = new Dialog();

        public Delta(GameServer server, Mundane mundane) : base(server, mundane)
        {
            Mundane.Template.QuestKey = "delta_quest";

            SequenceMenu.DisplayImage = (ushort) Mundane.Template.Image;
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "This place is infested with walkers. if you help me, In return, I'll take care of you.",
                CanMoveBack = false,
                CanMoveNext = true
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "So i'll do you, if you scratch mine. wait that does not sound right does it?",
                CanMoveBack = true,
                CanMoveNext = true
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Interested?",
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane, "Keen?",
                            new OptionsDataItem(0x0010, "Yes"),
                            new OptionsDataItem(0x0011, "what's do you mean by scratch?"),
                            new OptionsDataItem(0x0012, "fuck off, this is not the time.")
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
                            "You killed them?! i actually was not expecting you to come back....... um.... (shrugs)",
                            new OptionsDataItem(0x0017, "Now about my reward...")
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

                        break;

                    case 0x0010:
                        client.SendOptionsDialog(Mundane, "I need you to kill some zombies for me. {=u(5)");

                        if (quest != null)
                        {
                            quest.Started = true;
                            quest.TimeStarted = DateTime.UtcNow;
                        }

                        break;

                    case 0x0011:
                        client.SendOptionsDialog(Mundane, "I will suck, I mean scratch your back, if you scratch mine.",
                            new OptionsDataItem(0x0010, "Sounds good. it better be good but."),
                            new OptionsDataItem(0x0012, "fuck off mate.")
                        );
                        break;

                    case 0x0012:
                        quest = null;
                        client.SendOptionsDialog(Mundane, "Well then, I will leave you alone.");
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

                    case 0x0017:
                        if (quest != null && !quest.Rewarded && !quest.Completed)
                        {
                            quest.OnCompleted(client.Aisling);

                            client.TransitionToMap(client.Aisling.Map, new Position(56, 42));

                            if (SequenceMenu.CanMoveNext)
                            {
                                SequenceMenu.MoveNext(client);
                                SequenceMenu.Invoke(client);
                            }
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
                    Value = "Scratched Delta's Back."
                });
                quest.ExpRewards.Add(1000);
                quest.ExpRewards.Add(2000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);

                quest.SpellRewards.Add("beag ioc fein");

                client.Aisling.Quests.Add(quest);
            }

            quest.QuestStages = new List<QuestStep<Template>>();

            var q1 = new QuestStep<Template> {Type = QuestType.Accept};
            var q2 = new QuestStep<Template> {Type = QuestType.HasItem};

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.KillCount,
                Amount = 5,
                Value = "Undead"
            });

            quest.QuestStages.Add(q1);
            quest.QuestStages.Add(q2);

            if (!quest.Started)
            {
                SequenceMenu.Invoke(client);
            }
            else if (quest.Started && !quest.Completed && !quest.Rewarded)
            {
                client.SendOptionsDialog(Mundane, "So your still alive?.. Look more zombies!!");
                quest.HandleQuest(client, SequenceMenu);
            }
            else if (quest.Completed)
            {
                client.SendOptionsDialog(Mundane, "Please, Don't ever come back here.....");
                client.TransitionToMap(client.Aisling.Map, new Position(56, 42));
            }
            else
            {
                SequenceMenu.Invoke(client);
            }
        }
    }
}