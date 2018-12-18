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
using Darkages.Network.ServerFormats;
using System;

namespace Darkages.Network.Game.Components
{
    public class DaytimeComponent : GameServerComponent
    {
        private readonly GameServerTimer timer;
        private byte shade;

        public DaytimeComponent(GameServer server)
            : base(server)
        {
            timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContext.Config.DayTimeInterval));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            timer.Update(elapsedTime);

            if (timer.Elapsed)
            {
                timer.Reset();

                var format20 = new ServerFormat20 { Shade = shade };

                foreach (var client in Server.Clients)
                {
                    if (client != null)
                        client.Send(format20);
                }

                shade += 1;
                shade %= 18;
            }
        }
    }
}
