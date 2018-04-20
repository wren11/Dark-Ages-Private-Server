///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Barren Lord")]
    public class BarrenLord : MundaneScript
    {
        public BarrenLord(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void TargetAcquired(Sprite Target)
        {
        }


        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>();
            options.Add(new OptionsDataItem(0x0001, "Yes, Lord Barren"));
            options.Add(new OptionsDataItem(0x0002, "No."));

            client.SendOptionsDialog(Mundane, "You seek redemption?", options.ToArray());
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
    }
}
