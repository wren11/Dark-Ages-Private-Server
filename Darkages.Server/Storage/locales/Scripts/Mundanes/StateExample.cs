using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("some example")]
    public class StateExample : MundaneScript
    {
        Quest quest;

        public StateExample(GameServer server, Mundane mundane) 
            : base(server, mundane)
        {
            //this is called on script compile, build a quest.

            quest = new Quest();
            quest.Name = "some unique name";
            quest.LegendRewards = new List<Legend.LegendItem>()
            {
                new Legend.LegendItem()
                {
                     Category = "Class",
                     Color = (byte)LegendColor.Blue,
                      Icon = (byte)LegendIcon.Priest,
                       Value = "some legend mark"
                },
            };
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            //display some options to the user
            client.SendOptionsDialog(Mundane, "some options",
                new OptionsDataItem(0x0003, "option 3"));
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0003:
                    {
                        //this would return false if they were already on the quest.
                        //or failed to accept it for any reason
                        if (client.Aisling.AcceptQuest(quest))
                        {
                            //completed it.
                            quest.OnCompleted(client.Aisling, false);
                        }
                        else
                        {
                            //they already did it.
                        }
                    }
                    break;
                default: break;
            }
        }


        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }


        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
