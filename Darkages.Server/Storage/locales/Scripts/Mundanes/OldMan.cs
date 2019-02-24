using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("tut/OldMan")]
    public class OldMan : MundaneScript
    {
        Reactor reactor, classes_reactor;

        public OldMan(GameServer server, Mundane mundane) : base(server, mundane)
        {
            reactor = new Reactor()
            {
                Name = "Tutorial : Magic",
                CallerType = ReactorQualifer.Object,
                CanActAgain = false,
                Steps = new List<DialogSequence>()
                        {
                            new DialogSequence()
                            {
                                 Id = 0,
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 DisplayText = "Hey.",
                                 Title = mundane.Template.Name,
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
                                 HasOptions = true,
                                 ContinueOn = 0x0003,
                                 ContinueAt = 3,
                                 RollBackTo = 2,
                                 Callback   = (a, b) => {

                                     a.ActiveSequence = b;
                                     a.Client.SendOptionsDialog(mundane, "what?", new OptionsDataItem[]
                                     {                                          
                                         new OptionsDataItem(0x0001, "Tutorial : Interface"),
                                         new OptionsDataItem(0x0002, "Tutorial : Interaction"),
                                         new OptionsDataItem(0x0003, "Tutorial : Bulletins"),
                                         new OptionsDataItem(0x0004, "Tutorial : Classes"),
                                         new OptionsDataItem(0x0005, "Tutorial : General Info"),
                                     });
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
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "Here is a stick if you don't already have one. it's not much but its better then nothing. Make sure to equip it.",
                                 Callback = ((cbAisling, cbSequence) =>
                                 {
                                     if (cbAisling.Inventory.Has(i => i.Template.Name == "Stick") == null)
                                     {
                                         if (Item.Create(cbAisling, "Stick")?.GiveTo(cbAisling) ?? false)
                                         {
                                             ServerContext.Info.Debug("{0} received Stick from {1}", cbAisling.Username, cbSequence.Title);
                                         }
                                     }
                                 }),
                            },
                            new DialogSequence()
                            {
                                 Id = 8,
                                 CanMoveBack = true,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "There are some small creatures here you can practice on. When you are ready walk up the path and you should see an Old Man. Double-Click him to learn more.",
                                 Callback = ((cbAisling, cbSequence) =>
                                 {
                                     if (cbAisling.ActiveReactor.NextReactor != null)
                                     {
                                        cbAisling.ActiveReactor = cbAisling.ActiveReactor.NextReactor;
                                        cbAisling.ActiveReactor.Next(cbAisling.Client, true);
                                     }
                                 }),
                            },
                        },
            };


            classes_reactor = new Reactor()
            {
                Name = "Tutorial : Magic - part 2",
                CallerType = ReactorQualifer.Reactor,
                CanActAgain = true,
                CallingReactor = reactor.Name,
                Steps = new List<DialogSequence>()
                {
                            new DialogSequence()
                            {
                                 Id = 0,
                                 CanMoveBack = false,
                                 CanMoveNext = true,
                                 DisplayImage = (ushort)mundane.Template.Image,
                                 Title = mundane.Template.Name,
                                 DisplayText = "I'm a new reactor.", 
                            },
                },
            };
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.Aisling.CanReact = true;

            if (client.Aisling.ActiveReactor == null)
            {
                client.Aisling.ActiveReactor = Clone<Reactor>(reactor);
                client.Aisling.ActiveReactor.Steps = new List<DialogSequence>(reactor.Steps);
                client.Aisling.ActiveReactor.Script = ScriptManager.Load<ReactorScript>("Default Response Handler", reactor);


                client.Aisling.ActiveReactor.NextReactor = Clone<Reactor>(classes_reactor);
                client.Aisling.ActiveReactor.NextReactor.Steps = new List<DialogSequence>(classes_reactor.Steps);
                client.Aisling.ActiveReactor.NextReactor.Script = ScriptManager.Load<ReactorScript>("Default Response Handler", classes_reactor);
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

                if (client.Aisling.ActiveSequence != null && responseID == client.Aisling.ActiveSequence.ContinueOn)
                {
                    reactor.Goto(client, client.Aisling.ActiveSequence.ContinueAt);
                }
                else
                {
                    reactor.Goto(client, client.Aisling.ActiveSequence.RollBackTo);
                }
            }
        }

        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
