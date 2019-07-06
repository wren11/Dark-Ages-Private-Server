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

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat04 : NetworkFormat
    {
        public ClientFormat04()
        {
            Secured = true;
            Command = 0x04;
        }

        public byte Gender { get; set; }
        public byte HairStyle { get; set; }
        public byte HairColor { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            HairStyle = reader.ReadByte();
            Gender = reader.ReadByte();
            HairColor = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}