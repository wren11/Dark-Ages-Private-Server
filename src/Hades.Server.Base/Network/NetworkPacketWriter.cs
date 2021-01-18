#region

using Darkages.IO;
using System;
using System.Net;
using System.Text;

#endregion

namespace Darkages.Network
{
    public class NetworkPacketWriter
    {
        internal int Position;
        private readonly Encoding _encoding = Encoding.GetEncoding(949);
        private readonly byte[] _buffer;

        public NetworkPacketWriter()
        {
            _buffer = new byte[65534];
        }

        public NetworkPacket ToPacket()
        {
            return Position > 0 ? new NetworkPacket(_buffer, Position) : null;
        }

        public void Write(bool value)
        {
            Write(
                (byte) (value ? 1 : 0));
        }

        public void Write(byte value)
        {
            _buffer[Position++] = value;
        }

        public void Write(byte[] value)
        {
            Array.Copy(value, 0, _buffer, Position, value.Length);
            Position += value.Length;
        }

        public void Write(sbyte value)
        {
            _buffer[Position++] = (byte) value;
        }

        public void Write(short value)
        {
            Write((ushort) value);
        }

        public void Write(ushort value)
        {
            Write((byte) (value >> 8));
            Write((byte) value);
        }

        public void Write(int value)
        {
            Write((uint) value);
        }

        public void Write(uint value)
        {
            Write((ushort) (value >> 16));
            Write((ushort) value);
        }

        public void Write<T>(T value)
            where T : IFormattable
        {
            value.Serialize(this);
        }

        public void Write(IPEndPoint endPoint)
        {
            var ipBytes = endPoint.Address.GetAddressBytes();

            Write(ipBytes[3]);
            Write(ipBytes[2]);
            Write(ipBytes[1]);
            Write(ipBytes[0]);
            Write((ushort) endPoint.Port);
        }

        public void WriteString(string value)
        {
            _encoding.GetBytes(value, 0, value.Length, _buffer, Position);
            Position += _encoding.GetByteCount(value);
        }

        public void WriteStringA(string value)
        {
            var count = _encoding.GetByteCount(value);

            Write((byte) count);

            _encoding.GetBytes(value, 0, value.Length, _buffer, Position);

            Position += count;
        }

        public void WriteStringB(string value)
        {
            var count = _encoding.GetByteCount(value);

            Write((ushort) count);

            _encoding.GetBytes(value, 0, value.Length, _buffer, Position);
            Position += count;
        }
    }
}