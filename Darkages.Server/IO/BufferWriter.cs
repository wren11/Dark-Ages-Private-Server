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
    public class BufferWriter : BinaryWriter
    {
        private static readonly Encoding encoding = Encoding.GetEncoding(949);

        public BufferWriter(Stream stream)
            : base(stream, Encoding.GetEncoding(949))
        {
        }

        public void Write(IPAddress ipAddress)
        {
            var ipBuffer = ipAddress.GetAddressBytes();

            base.Write(ipBuffer[3]);
            base.Write(ipBuffer[2]);
            base.Write(ipBuffer[1]);
            base.Write(ipBuffer[0]);
        }

        public void WriteStringA(string value)
        {
            var length = (byte)encoding.GetByteCount(value);

            base.Write(length);
            base.Write(encoding.GetBytes(value));
        }

        public void WriteStringB(string value)
        {
            var length = (ushort)encoding.GetByteCount(value);

            Write(length);
            base.Write(encoding.GetBytes(value));
        }

        public override void Write(string value)
        {
            base.Write(encoding.GetBytes(value + '\0'));
        }

        public override void Write(short value)
        {
            base.Write(new[]
            {
                (byte) (value >> 8),
                (byte) value
            });
        }

        public override void Write(ushort value)
        {
            base.Write(new[]
            {
                (byte) (value >> 8),
                (byte) value
            });
        }

        public override void Write(int value)
        {
            base.Write(new[]
            {
                (byte) (value >> 24),
                (byte) (value >> 16),
                (byte) (value >> 8),
                (byte) value
            });
        }

        public override void Write(uint value)
        {
            base.Write(new[]
            {
                (byte) (value >> 24),
                (byte) (value >> 16),
                (byte) (value >> 8),
                (byte) value
            });
        }
    }
}
