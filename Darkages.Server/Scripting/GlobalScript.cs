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
using Darkages.Network.Object;

namespace Darkages.Scripting
{
    public abstract class GlobalScript : ObjectManager
    {
        public GameClient Client;

        public GlobalScript(GameClient client)
        {
            Client = client;
        }

        public GameServerTimer Timer { get; set; }

        public abstract void Update(TimeSpan elapsedTime);


        public abstract void Run(GameClient client);
    }
}