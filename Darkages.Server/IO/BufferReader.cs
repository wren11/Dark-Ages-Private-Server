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
using System.IO;
using System.Net;
using System.Text;

namespace Darkages.IO
{
    public class BufferReader : BinaryReader
    {
        private readonly Encoding encoding = Encoding.GetEncoding(949);

        public BufferReader(Stream stream)
            : base(stream, Encoding.GetEncoding(949))
        {
        }

        public IPAddress ReadIPAddress()
        {
            var ipBuffer = new byte[4];

            ipBuffer[3] = ReadByte();
            ipBuffer[2] = ReadByte();
            ipBuffer[1] = ReadByte();
            ipBuffer[0] = ReadByte();

            return new IPAddress(ipBuffer);
        }

        public string ReadStringA()
        {
            return encoding.GetString(
                ReadBytes(ReadByte()));
        }

        public string ReadStringB()
        {
            return encoding.GetString(
                ReadBytes(ReadUInt16()));
        }

        public override string ReadString()
        {
            var data = ' ';
            var text = string.Empty;

            do
            {
                text += data = ReadChar();
            } while (data != '\0');

            return text;
        }

        public override short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public override ushort ReadUInt16()
        {
            return (ushort)((
                                 ReadByte() << 8) |
                             ReadByte());
        }

        public override int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public override uint ReadUInt32()
        {
            return (uint)((
                               ReadUInt16() << 16) |
                           ReadUInt16());
        }
    }
}
