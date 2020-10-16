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
    [Script("gos")]
    public class Gos : MundaneScript
    {
        public Dialog SequenceMenu = new Dialog();

        public Gos(GameServer server, Mundane mundane) : base(server, mundane)
        {
            Mundane.Template.QuestKey = "gos_quest";

            SequenceMenu.DisplayImage = (ushort) Mundane.Template.Image;
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Man, Look around, can you believe this shit?! fuckin rats everywhere!"
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Looks like you need to level, so why not go kill some."
            });
            SequenceMenu.Sequences.Add(new DialogSequence
            {
                Title = Mundane.Template.Name,
                DisplayText = "Interested?",
                HasOptions = true,
                OnSequenceStep = (sender, args) =>
                {
                    if (args.HasOptions)
                        sender.Client.SendOptionsDialog(Mundane, "Want to help?",
                            new OptionsDataItem(0x0010, "Yeah, I'll help."),
                            new OptionsDataItem(0x0011, "what's in it for me?"),
                            new OptionsDataItem(0x0012, "nah, I'm not killing rats for you. fuck off.")
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
                            $"oye {sender.Path.ToString()}, Nice job.",
                            new OptionsDataItem(0x0017, "Hand over the rat shit")
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
            if (message == "benson called you a pussy")
            {
                var benson = GetObject<Mundane>(client.Aisling.Map, i => i.Template.Name == "Benson");

                Mundane.Target = benson ?? client.Aisling as Sprite;
                Mundane.Template.EnableAttacking = true;
                Mundane.Template.EnableTurning = true;
                Mundane.Template.EnableWalking = true;

                if (Mundane.Template.EnableAttacking)
                    Mundane.Template.AttackTimer = new GameServerTimer(TimeSpan.FromMilliseconds(750));

                if (Mundane.Template.EnableWalking)
                {
                    Mundane.Template.EnableTurning = false;
                    Mundane.Template.WalkTimer = new GameServerTimer(TimeSpan.FromSeconds(500));
                }

                if (Mundane.Template.EnableTurning)
                    Mundane.Template.TurnTimer = new GameServerTimer(TimeSpan.FromSeconds(1));

                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(3000);
                    Mundane.Show(Scope.NearbyAislings,
                        new ServerFormat0D {Text = "what u say cunt!!", Type = 0x00, Serial = Mundane.Serial});
                    Thread.Sleep(2000);
                    Mundane.Show(Scope.NearbyAislings,
                        new ServerFormat0D
                        {
                            Text = "you got a problem? there a problem!?",
                            Type = 0x00,
                            Serial = Mundane.Serial
                        });
                });

                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(12000);

                    if (benson != null)
                    {
                        var quest = client.Aisling.Quests.Find(i => i.Name == "Benson_quest" && !i.Completed);
                        quest.OnCompleted(client.Aisling);
                    }

                    Mundane.Show(Scope.NearbyAislings,
                        new ServerFormat0D {Text = "fuckn weak as piss.", Type = 0x00, Serial = Mundane.Serial});

                    Mundane.CurrentHp = 0;
                    Mundane.Template.TurnTimer = null;
                    Mundane.Template.AttackTimer = null;
                    Mundane.Template.WalkTimer = null;
                    Mundane.Template.EnableAttacking = false;
                    Mundane.Template.EnableWalking = false;
                    Mundane.Template.EnableTurning = false;
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
                        client.SendOptionsDialog(Mundane, "sweet, Bring me some rat shit. (10)");

                        if (quest != null)
                        {
                            quest.Started = true;
                            quest.TimeStarted = DateTime.UtcNow;
                        }

                        break;

                    case 0x0011:
                        client.SendOptionsDialog(Mundane, "I'll hook you up with something good!",
                            new OptionsDataItem(0x0010, "Sure, Ok then!"),
                            new OptionsDataItem(0x0012, "Don't need anything mate.")
                        );
                        break;

                    case 0x0012:
                        client.SendOptionsDialog(Mundane, "well fuck you then.");
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
                            client.SendOptionsDialog(Mundane, "Thank you.");
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
                    Value = "Helped Gos kill some rats."
                });

                client.Aisling.Quests.Add(quest);
            }

            quest.QuestStages = new List<QuestStep<Template>>();

            var q1 = new QuestStep<Template> {Type = QuestType.Accept};
            var q2 = new QuestStep<Template> {Type = QuestType.ItemHandIn};

            q2.Prerequisites.Add(new QuestRequirement
            {
                Type = QuestType.ItemHandIn,
                Amount = 10,
                TemplateContext = ServerContext.GlobalItemTemplateCache["rat shit"]
            });

            quest.QuestStages.Add(q1);
            quest.QuestStages.Add(q2);

            if (!quest.Started)
            {
                SequenceMenu.Invoke(client);
            }
            else if (quest.Started && !quest.Completed && !quest.Rewarded)
            {
                client.SendOptionsDialog(Mundane, "Where is the rat shit i need?");
                quest.HandleQuest(client, SequenceMenu);
            }
            else if (quest.Completed)
            {
                client.SendOptionsDialog(Mundane, "What a legend!");
            }
            else
            {
                SequenceMenu.Invoke(client);
            }
        }
    }
}