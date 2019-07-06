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
using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class MundaneScript : ObjectManager
    {
        public MundaneScript(GameServer server, Mundane mundane)
        {
            Server = server;
            Mundane = mundane;
        }

        public GameServer Server { get; set; }

        public Mundane Mundane { get; set; }

        public abstract void OnClick(GameServer server, GameClient client);

        public abstract void OnResponse(GameServer server, GameClient client, ushort responseID, string args);

        public abstract void OnGossip(GameServer server, GameClient client, string message);

        public abstract void TargetAcquired(Sprite Target);
    }
}