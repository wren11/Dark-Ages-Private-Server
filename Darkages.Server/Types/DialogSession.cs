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
using System;

namespace Darkages.Types
{
    public class DialogSession
    {
        public DialogSession(Aisling user, int serial)
        {
            Serial = serial;
            SessionPosition = user.Position;
            CurrentMapID = user.CurrentMapId;
            Sequence = 0;
        }

        public int CurrentMapID { get; set; }
        public ushort Sequence { get; set; }
        public Position SessionPosition { get; set; }
        public int Serial { get; set; }

        public Action<GameServer, GameClient, ushort, string> Callback { get; set; }
        public Dialog StateObject { get; set; }
    }
}
