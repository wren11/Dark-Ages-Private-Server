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
    public class ServerFormat3F : NetworkFormat
    {
        public ServerFormat3F()
        {
            Secured = true;
            Command = 0x3F;
        }

        public ServerFormat3F(byte pane, byte slot, int time) : this()
        {
            Pane = pane;
            Slot = slot;
            Time = time;
        }

        public byte Pane;
        public byte Slot;
        public int Time;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)Pane);
            writer.Write((byte)Slot);
            writer.Write((int)Time);
        }
    }
}
