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
using Darkages.Network.Game;

namespace Darkages
{
    public class EphemeralReactor
    {
        private GameServerTimer _timer;

        public EphemeralReactor(string lpKey, int lpTimeout)
        {
            YamlKey = lpKey;
            _timer = new GameServerTimer(TimeSpan.FromSeconds(lpTimeout));
        }

        public string YamlKey { get; set; }
        public bool Expired { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                Expired = true;
                _timer = null;
            }
        }
    }
}