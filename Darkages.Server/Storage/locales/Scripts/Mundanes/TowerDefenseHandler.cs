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
using System;
using System.Collections.Generic;
using System.Linq;

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
            {
                client.TransitionToMap(510, new Position(5, 4));

                return;
            }
            else
            {
                client.CloseDialog();
            }
        }

        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
