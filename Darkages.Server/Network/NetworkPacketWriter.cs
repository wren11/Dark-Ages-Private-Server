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
using System;
using System.Net;
using System.Text;

namespace Darkages.Network
{
    public class NetworkPacketWriter
    {
        private readonly byte[] buffer = new byte[0x10000];
        private readonly Encoding encoding = Encoding.GetEncoding(949);

        public int Position { get; set; }
        public bool CanWrite => Position + 1 < buffer.Length;

        public void Write(bool value)
        {
            Write(
                (byte)(value ? 1 : 0));
        }

        public void Write(byte value)
        {
            buffer[Position++] = value;
        }

        public void Write(byte[] value)
        {
            Array.Copy(value, 0, buffer, Position, value.Length);
            Position += value.Length;
        }

        public void Write(sbyte value)
        {
            buffer[Position++] = (byte)value;
        }

        public void Write(short value)
        {
            Write(
                (ushort)value);
        }

        public void Write(ushort value)
        {
            Write(
                (byte)(value >> 8));
            Write(
                (byte)value);
        }

        public void Write(int value)
        {
            Write(
                (uint)value);
        }

        public void Write(uint value)
        {
            Write(
                (ushort)(value >> 16));
            Write(
                (ushort)value);
        }

        public void Write<T>(T value)
            where T : IFormattable
        {
            value.Serialize(this);
        }

        public void WriteString(string value)
        {
            encoding.GetBytes(value, 0, value.Length, buffer, Position);
            Position += encoding.GetByteCount(value);
        }

        public void WriteStringA(string value)
        {
            var count = encoding.GetByteCount(value);

            Write(
                (byte)count);

            encoding.GetBytes(value, 0, value.Length, buffer, Position);
            Position += count;
        }

        public void WriteStringB(string value)
        {
            var count = encoding.GetByteCount(value);

            Write(
                (ushort)count);

            encoding.GetBytes(value, 0, value.Length, buffer, Position);
            Position += count;
        }

        public void Write(IPEndPoint endPoint)
        {
            var ipBytes = endPoint.Address.GetAddressBytes();

            Write(ipBytes[3]);
            Write(ipBytes[2]);
            Write(ipBytes[1]);
            Write(ipBytes[0]);
            Write(
                (ushort)endPoint.Port);
        }

        public NetworkPacket ToPacket()
        {
            return new NetworkPacket(buffer, Position);
        }
    }
}
