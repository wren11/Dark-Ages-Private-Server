#region

using System.Collections.Generic;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Barren Lord")]
    public class BarrenLord : MundaneScript
    {
        public BarrenLord(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>
            {
                new OptionsDataItem(0x0001, "Yes, Lord Barren"), new OptionsDataItem(0x0002, "No.")
            };

            client.SendOptionsDialog(Mundane, "You seek redemption?", options.ToArray());
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (responseID == 0x0001)
                client.SendOptionsDialog(Mundane, "You dare pay the costs?",
                    new OptionsDataItem(0x0005, "Yes"),
                    new OptionsDataItem(0x0001, "No"));

            if (responseID == 0x0005)
            {
                client.Aisling._MaximumHp -= ServerContext.Config.DeathHPPenalty;

                if (client.Aisling.MaximumHp <= 0)
                    client.Aisling._MaximumHp = ServerContext.Config.MinimumHp;

                client.Revive();
                client.SendMessage(0x02, "You have lost some health.");
                client.SendStats(StatusFlags.All);
                client.Aisling.GoHome();
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}