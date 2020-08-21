#region

using System.Collections.Generic;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("some example")]
    public class StateExample : MundaneScript
    {
        private readonly Quest quest;

        public StateExample(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
            quest = new Quest();
            quest.Name = "some unique name";
            quest.LegendRewards = new List<Legend.LegendItem>
            {
                new Legend.LegendItem
                {
                    Category = "Class",
                    Color = (byte) LegendColor.Blue,
                    Icon = (byte) LegendIcon.Priest,
                    Value = "some legend mark"
                }
            };
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.SendOptionsDialog(Mundane, "some options",
                new OptionsDataItem(0x0003, "option 3"));
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0003:
                {
                    if (client.Aisling.AcceptQuest(quest))
                        quest.OnCompleted(client.Aisling);
                }
                    break;
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}