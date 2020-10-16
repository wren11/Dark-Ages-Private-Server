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
    [Script("macronator")]
    public class macronator : MundaneScript
    {
        public Dialog SequenceMenu = new Dialog();

        public macronator(GameServer server, Mundane mundane) : base(server, mundane)
        {
            Mundane.Template.QuestKey = "macronator_quest";

            SequenceMenu.DisplayImage = (ushort) Mundane.Template.Image;
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText =
                    "We trapped these monsters here to harvest there bones, but it seems we are under staffed."
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText =
                    "The guy who was supposed to knock them on the head shot through, Said he had the shits or something."
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Interested?",
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane, "Can you help me knock these on the head?",
                            new OptionsDataItem(0x0010, "My Pleasure."),
                            new OptionsDataItem(0x0011, "But they are in a cage.. how?"),
                            new OptionsDataItem(0x0012, "I'm not ya slave cunt.")
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
                            "See you look good at this. Not so bad - was it?",
                            new OptionsDataItem(0x0017, "a piece of piss mate.")
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

                        if (quest != null)
                            if (!quest.Started)
                            {
                                client.SendOptionsDialog(Mundane,
                                    "Here are some spells and a staff. Go and execute some 3 cows, I'm Hungry.");

                                if (Spell.GiveTo(client.Aisling, "Macronator's Magic Spell", 1) ||
                                    client.Aisling.SpellBook.Has("Macronator's Magic Spell"))
                                {
                                    if (Item.Create(client.Aisling,
                                            ServerContext.GlobalItemTemplateCache["Training Staff"])
                                        .GiveTo(client.Aisling))
                                    {
                                        client.SendMessage(0x02,
                                            "You received a new spell from macronator and a new item.");
                                        quest.Started = true;
                                        quest.TimeStarted = DateTime.UtcNow;
                                    }
                                    else
                                    {
                                        client.SendOptionsDialog(Mundane, "You are to weak after all.");
                                        quest.Started = false;
                                    }
                                }
                                else
                                {
                                    client.SendOptionsDialog(Mundane, "You are to smart to be here.");
                                    quest.Started = false;
                                }
                            }

                        break;

                    case 0x0011:
                        client.SendOptionsDialog(Mundane,
                            "You need to use magic. If you help, I will give you some magic and a staff.",
                            new OptionsDataItem(0x0010, "Ok, I will do it."),
                            new OptionsDataItem(0x0012, "I refuse to slaughter a helpless animal.")
                        );
                        break;

                    case 0x0012:
                        quest = null;
                        client.SendOptionsDialog(Mundane, "Go milk them then. by the cock.");
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
                            client.SendOptionsDialog(Mundane,
                                "I will let you keep those things i gave you. But don't tell anyone.");

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
                    Color = (byte) LegendColor.Brown,
                    Icon = (byte) LegendIcon.Victory,
                    Value = "Slaughtered some helpless cows."
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
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);
                quest.ExpRewards.Add(5000);

                client.Aisling.Quests.Add(quest);
            }

            quest.QuestStages = new List<QuestStep<Template>>();

            var q1 = new QuestStep<Template> {Type = QuestType.Accept};
            var q2 = new QuestStep<Template> {Type = QuestType.HasItem};

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.KillCount,
                Amount = 3,
                Value = "Helpless Animal"
            });

            quest.QuestStages.Add(q1);
            quest.QuestStages.Add(q2);

            if (!quest.Started)
            {
                SequenceMenu.Invoke(client);
            }
            else if (quest.Started && !quest.Completed && !quest.Rewarded)
            {
                client.SendOptionsDialog(Mundane, "Murder them fuck'n cows.");
                quest.HandleQuest(client, SequenceMenu);
            }
            else if (quest.Completed)
            {
                client.SendOptionsDialog(Mundane,
                    "Remember don't tell anyone i gave you that stuff. or it will be my ass that gets sodomized next.");
            }
            else
            {
                SequenceMenu.Invoke(client);
            }
        }
    }
}