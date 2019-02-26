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
using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat42 : NetworkFormat
    {
        public ServerFormat42()
        {
            Secured = true;
            Command = 0x42;
        }

        public Aisling Player;

        public Item ExchangedItem;

        public byte Stage;

        public byte Type;

        public string Message;

        public ServerFormat42(Aisling user, byte type = 0x00, byte method = 0x00, string lpMsg = "", Item lpItem = null)
        {
            Stage = type;
            Player = user;
            ExchangedItem = lpItem;
            Message = lpMsg;
        }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
