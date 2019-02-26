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
    public class ClientFormat4F : NetworkFormat
    {
        public ClientFormat4F()
        {
            Secured = true;
            Command = 0x4F;
        }

        public ushort Count { get; set; }
        public byte[] Image { get; set; }
        public string Words { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Count = reader.ReadUInt16();
            Image = reader.ReadBytes(reader.ReadUInt16());
            Words = reader.ReadStringB();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}
