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
    public class ClientFormat3B : NetworkFormat
    {
        public string To, Title, Message;

        public ClientFormat3B()
        {
            Secured = true;
            Command = 0x3B;
        }

        public ushort TopicIndex { get; set; }
        public ushort BoardIndex { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            if (reader.GetCanRead())
            {
                Type = reader.ReadByte();

                if (reader.GetCanRead()) BoardIndex = reader.ReadUInt16();

                if (reader.GetCanRead()) TopicIndex = reader.ReadUInt16();

                if (Type == 0x06)
                {
                    reader.Position = 0;
                    reader.ReadByte();
                    BoardIndex = reader.ReadUInt16();

                    To = reader.ReadStringA();
                    Title = reader.ReadStringA();
                    Message = reader.ReadStringB();
                }
                else if (Type == 0x04)
                {
                    reader.Position = 0;
                    reader.ReadByte();
                    BoardIndex = reader.ReadUInt16();

                    Title = reader.ReadStringA();
                    Message = reader.ReadStringB();
                }
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}