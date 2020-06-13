#region

using System;
using System.Net;
using System.Text;
using Darkages.IO;

#endregion

namespace Darkages.Network
{
    public class NetworkPacketWriter
    {
        private readonly Encoding encoding = Encoding.GetEncoding(949);

        public NetworkPacketWriter()
        {
            Buffer = BufferPool.Take(0x8192);
        }

        public byte[] Buffer { get; }

        public int Position { get; set; }

        public bool IsEmpty => Buffer == null || Buffer.Length == 0;

        public void Write(bool value)
        {
            Write(
                (byte) (value ? 1 : 0));
        }

        public void Write(byte value)
        {
            Buffer[Position++] = value;
        }

        public void Write(byte[] value)
        {
            Array.Copy(value, 0, Buffer, Position, value.Length);
            Position += value.Length;
        }

        public void Write(sbyte value)
        {
            Buffer[Position++] = (byte) value;
        }

        public void Write(short value)
        {
            Write(
                (ushort) value);
        }

        public void Write(ushort value)
        {
            Write(
                (byte) (value >> 8));
            Write(
                (byte) value);
        }

        public void Write(int value)
        {
            Write(
                (uint) value);
        }

        public void Write(uint value)
        {
            Write(
                (ushort) (value >> 16));
            Write(
                (ushort) value);
        }

        public void Write<T>(T value)
            where T : IFormattable
        {
            value.Serialize(this);
        }

        public void WriteString(string value)
        {
            encoding.GetBytes(value, 0, value.Length, Buffer, Position);
            Position += encoding.GetByteCount(value);
        }

        public void WriteStringA(string value)
        {
            var count = encoding.GetByteCount(value);

            Write(
                (byte) count);

            encoding.GetBytes(value, 0, value.Length, Buffer, Position);
            Position += count;
        }

        public void WriteStringB(string value)
        {
            var count = encoding.GetByteCount(value);

            Write(
                (ushort) count);

            encoding.GetBytes(value, 0, value.Length, Buffer, Position);
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
                (ushort) endPoint.Port);
        }

        public NetworkPacket ToPacket()
        {
            if (Position > 0) return new NetworkPacket(Buffer, Position);

            return null;
        }
    }
}