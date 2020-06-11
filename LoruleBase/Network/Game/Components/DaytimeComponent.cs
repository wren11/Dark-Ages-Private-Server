///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using System;
using Darkages.Network.ServerFormats;

namespace Darkages.Network.Game.Components
{
    public class DaytimeComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;
        private byte _shade;

        public DaytimeComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContextBase.Config.DayTimeInterval));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                _timer.Reset();

                var format20 = new ServerFormat20 {Shade = _shade};

                foreach (var client in Server.Clients)
                    if (client != null)
                        client.Send(format20);

                _shade += 1;
                _shade %= 18;
            }
        }
    }
}