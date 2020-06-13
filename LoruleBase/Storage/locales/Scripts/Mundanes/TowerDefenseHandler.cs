#region

using System.Collections.Generic;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("TowerDefenseHandler")]
    public class TowerDefenseHandler : MundaneScript
    {
        public TowerDefenseHandler(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>();
            options.Add(new OptionsDataItem(0x0001, "Enter"));
            options.Add(new OptionsDataItem(0x0002, "I will pass."));
            client.SendOptionsDialog(Mundane, "Want to give it a try?", options.ToArray());
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (responseID == 0x0001)
                client.TransitionToMap(510, new Position(5, 4));
            else
                client.CloseDialog();
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}