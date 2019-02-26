using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("tut/OldMan")]
    public class OldMan : MundaneScript
    {
        public void Serialize()
        {
            var json = JsonConvert.SerializeObject(reactor, new JsonSerializerSettings() { Formatting = Formatting.Indented });
            var obj = JsonConvert.DeserializeObject<Reactor>(json, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }


        Reactor reactor;




        public OldMan(GameServer server, Mundane mundane) : base(server, mundane)
        {
            reactor = new Reactor()
            {
                Name = "Tutorial : Magic",
                CanActAgain = false,
                CallingNpc = mundane.Template.Name,
                WhenCanActAgain = new GameServerTimer(TimeSpan.FromMinutes(10)),
                Sequences = new List<DialogSequence>()
                        {
                            new DialogSequence()
                            {
                                 Id = 0,
                                 CanMoveBack  = false,
                                 CanMoveNext  = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 DisplayText  = "Hey.",
                                 Title        = mundane.Template.Name,
                            },
                            new DialogSequence()
                            {
                                 Id = 1,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "What's Up?",
                            },
                            new DialogSequence()
                            {
                                 Id = 2,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "Let me tell you what?",
                                 ContinueOn = 0x0003,
                                 ContinueAt = 3,
                                 RollBackTo = 1,
                                 HasOptions = true,
                                 Options = new OptionsDataItem[]
                                     {
                                         new OptionsDataItem(0x0001, "Tutorial : Interface"),
                                         new OptionsDataItem(0x0002, "Tutorial : Interaction"),
                                         new OptionsDataItem(0x0003, "Tutorial : Bulletins"),
                                         new OptionsDataItem(0x0004, "Tutorial : Classes"),
                                         new OptionsDataItem(0x0005, "Tutorial : General Info"),
                                     },
                            },
                            new DialogSequence()
                            {
                                 Id = 3,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "engaged",
                            },
                            new DialogSequence()
                            {
                                 Id = 4,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "To attack a creature, face the creature and press the <Space Bar>. It is recommended that you use the keyboard arrow keys during combat. Be careful what you attack. Never attack mundanes, like me. You cannot harm other Aislings and they cannot harm you.",
                            },
                            new DialogSequence()
                            {
                                 Id = 5,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "To help you see creatures, press <Tab>. The creatures will appear as red on the overhead map.",
                            },
                            new DialogSequence()
                            {
                                 Id = 6,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "Initially, several creatures will be too great for you. Ask a friend which areas are safe to venture into at first.",
                            },
                            new DialogSequence()
                            {
                                 Id = 7,
                                 CanMoveBack  = false,
                                 CanMoveNext  = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "There are some small creatures here you can practice on. go and kill 5 for me using what I've taught you, Come back here once you're done for your next challenge.",
                                 IsCheckPoint = true,
                                 Conditions  = new List<QuestRequirement>()
                                 {
                                     new QuestRequirement()
                                     {
                                        Type   = QuestType.KillCount,
                                        Value  = "Kardi",
                                        Amount = 5,
                                     }
                                 },
                                 ContinueAt = 8,
                                 ContinueOn = 8,
                                 ConditionFailMessage = "You have not done what i have asked of you yet. Please return when you have."
                            },
                            new DialogSequence()
                            {
                                 Id = 8,
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 ContinueOn = 0x0006,
                                 ContinueAt = 9,
                                 RollBackTo = 7,
                                 HasOptions = true,
                                 DisplayText = "Great, Here you go. Now, Ready for the next challenge?",
                                 Options = new OptionsDataItem[]
                                     {
                                         new OptionsDataItem(0x0006, "Bring it On."),
                                         new OptionsDataItem(0x0007, "No I've had enough."),
                                     },
                            },
                            new DialogSequence()
                            {
                                 Id = 9,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 IsCheckPoint = true,
                                 DisplayText = "I'm a new reactor. I'm the next dialog.",
                            },
                            new DialogSequence()
                            {
                                 Id = 10,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "bye bye bye",
                            },
                            new DialogSequence()
                            {
                                 Id = 11,
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "a",
                            },
                            new DialogSequence()
                            {
                                 Id = 12,
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "b",
                            },
                },
            };

            Serialize();

        }


        public override void OnClick(GameServer server, GameClient client)
        {
            if (client.Aisling.ActiveSequence == null)
            {
                client.Aisling.ActiveReactor = null;
                client.Aisling.ActiveSequence = reactor.Sequences[0];
            }

            if (client.Aisling.ActiveSequence.DisplayImage != Mundane.Template.Image)
            {
                client.Aisling.ActiveReactor  = null;
                client.Aisling.ActiveSequence = null;
                return;
            }

            client.Aisling.CanReact = true;

            if (client.Aisling.ActiveReactor == null)
            {
                client.Aisling.ActiveReactor = Clone<Reactor>(reactor);
                client.Aisling.ActiveReactor.Sequences = new List<DialogSequence>(reactor.Sequences);
                client.Aisling.ActiveReactor.Decorator = ScriptManager.Load<ReactorScript>("Default Response Handler", reactor);

            }

            client.Aisling.ActiveReactor.Update(client);

        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (client.Aisling != null)
            {
                if (!client.Aisling.WithinRangeOf(Mundane))
                    return;

                if (client.Aisling.ActiveSequence != null)
                {
                    if (responseID == client.Aisling.ActiveSequence.ContinueOn)
                    {
                        reactor.Goto(client, client.Aisling.ActiveSequence.ContinueAt);
                    }
                    else
                    {
                        reactor.Goto(client, client.Aisling.ActiveSequence.RollBackTo);
                    }
                }
            }
        }

        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
