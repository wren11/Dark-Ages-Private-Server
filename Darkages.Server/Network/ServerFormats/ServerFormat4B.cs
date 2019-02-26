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
namespace Darkages.Network.ServerFormats
{
    public class ServerFormat4B : NetworkFormat
    {
        public ServerFormat4B()
        {
            Secured = true;
            Command = 0x4B;
        }

        public byte Type { get; set; }
        public uint Serial { get; set; }
        public byte ItemSlot { get; set; }

        public ServerFormat4B(uint serial, byte type, byte itemSlot = 0) : this()
        {
            Type = type;
            Serial = serial;
            ItemSlot = itemSlot;
        }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (Type == 0)
            {
                writer.Write((ushort)0x06);
                writer.Write((byte)0x4A);
                writer.Write((byte)0x00);
                writer.Write(Serial);
                writer.Write((byte)0x00);
            }

            if (Type == 1)
            {
                writer.Write((ushort)0x07);
                writer.Write((byte)0x4A);
                writer.Write((byte)0x01);
                writer.Write(Serial);
                writer.Write(ItemSlot);
                writer.Write((byte)0x00);
            }
        }
    }
}
