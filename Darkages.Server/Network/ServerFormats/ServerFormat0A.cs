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
    public class ServerFormat0A : NetworkFormat
    {
        public enum MsgType
        {
            Action = 2,
            Board = 10,
            Dialog = 9,
            Global = 3,
            Guild = 12,
            Message = 1,
            Party = 11,
            Whisper = 0
        }

        public ServerFormat0A(byte type, string text) : this()
        {
            Type = type;
            Text = text;
        }

        public ServerFormat0A()
        {
            Secured = true;
            Command = 0x0A;
        }

        public byte Type { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (!string.IsNullOrEmpty(Text))
                writer.WriteStringB(Text);
        }
    }
}