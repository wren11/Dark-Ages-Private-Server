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
using Darkages.Scripting;
using System;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Reactors")]
    public class Reactors : GlobalScript
    {
        GameClient Client;
        public Reactors(GameClient client) : base(client)
        {
            Client = client;
        }

        public override void OnDeath(GameClient client, TimeSpan elapsedTime)
        {

        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Client == null)
                return;
            if (Client.IsRefreshing)
                return;
                   
            if (Client.Aisling != null && Client.Aisling.LoggedIn)
            {
                if (!Client.Aisling.Map.Ready)
                    return;

                EastWoodlands();

            }
        }

        private void EastWoodlands()
        {
            if (Client.Aisling.CurrentMapId == 300 && Client.Aisling.Y > 1 && Client.Aisling.Y < 3)
            {
                Client.SendMessage(0x02, "* East Woodlands *\n\nThis zone is governed by law. A guard has let you pass, this time.");
                Client.TransitionToMap(Client.Aisling.CurrentMapId, new Types.Position(Client.Aisling.X, 5));
            }
        }
    }
}
