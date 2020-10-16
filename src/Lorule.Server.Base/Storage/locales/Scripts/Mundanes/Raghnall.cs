#region

using System.Collections.Generic;
using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("tut/Raghnall")]
    public class Raghnall : MundaneScript
    {
        private readonly Reactor _reactor;

        public Raghnall(GameServer server, Mundane mundane) : base(server, mundane)
        {
            _reactor = new Reactor
            {
                Name = "Tutorial : Combat",
                CallerType = ReactorQualifer.Object,
                CanActAgain = false,
                Sequences = new List<DialogSequence>
                {
                    new DialogSequence
                    {
                        CanMoveBack = false,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        DisplayText = "Hello, I'm going to teach you about combat.",
                        Title = mundane.Template.Name
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "First be aware that combat is dangerous. Make friends about your same level and adventure in a group."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "If you die, most of your magic items will be destroyed, you will lose a percentage of your experience points, and you will gain a legendary scar of Sgrios, a legend mark from the god of destruction."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "The current skill you have is called the Assail skill. It is a basic attack skill. Throughout the game you will learn more variety of skills that can perform different actions. To use skills just double-click their icon."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "To attack a creature, face the creature and press the <Space Bar>. It is recommended that you use the keyboard arrow keys during combat. Be careful what you attack. Never attack mundanes, like me. You cannot harm other Aislings and they cannot harm you."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "To help you see creatures, press <Tab>. The creatures will appear as red on the overhead map."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "Initially, several creatures will be too great for you. Ask a friend which areas are safe to venture into at first."
                    },
                    new DialogSequence
                    {
                        CanMoveBack = false,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "Here is a stick if you don't already have one. it's not much but its better then nothing. Make sure to equip it.",
                        OnSequenceStep = (cbAisling, cbSequence) =>
                        {
                            if (cbAisling.Inventory.Has(i => i.Template.Name == "Stick") == null)
                                if (Item.Create(cbAisling, "Stick")?.GiveTo(cbAisling) ?? false)
                                {
                                }
                        }
                    },
                    new DialogSequence
                    {
                        CanMoveBack = true,
                        CanMoveNext = true,
                        DisplayImage = (ushort) mundane.Template.Image,
                        Title = mundane.Template.Name,
                        DisplayText =
                            "There are some small creatures here you can practice on. When you are ready walk up the path and you should see an Old Man. Double-Click him to learn more."
                    }
                }
            };
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.Aisling.CanReact = true;

            client.Aisling.ActiveReactor = Clone<Reactor>(_reactor);
            client.Aisling.ActiveReactor.Sequences = new List<DialogSequence>(_reactor.Sequences);
            client.Aisling.ActiveReactor.Decorators =
                ScriptManager.Load<ReactorScript>("Default Response Handler", _reactor);

            client.Aisling.ActiveReactor.Update(client);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseId, string args)
        {
        }

        public override void TargetAcquired(Sprite target)
        {
        }
    }
}