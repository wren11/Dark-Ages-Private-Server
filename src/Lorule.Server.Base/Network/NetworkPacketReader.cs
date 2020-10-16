#region

using System.Text;
using Darkages.Types;

#endregion

namespace Darkages.Network
{
    public class NetworkPacketReader
    {
        public NetworkPacket Packet;
        public int Position;
        private readonly Encoding _encoding = Encoding.GetEncoding(0x3B5);

        public bool GetCanRead()
        {
            return Position + 1 < Packet.Data.Length;
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        public byte ReadByte()
        {
            byte b = 0;

            if (Position == -1)
            {
                b = Packet.Ordinal;
            }
            else
            {
                if (Position < Packet.Data.Length)
                    b = Packet.Data[Position];
            }

            Position++;

            return b;
        }

        public byte[] ReadBytes(int count)
        {
            var array = new byte[count];

            for (var i = 0; i < count; i++)
                array[i] = ReadByte();

            return array;
        }

        public short ReadInt16()
        {
            return (short) ReadUInt16();
        }

        public int ReadInt32()
        {
            return (int) ReadUInt32();
        }

        public T ReadObject<T>()
            where T : IFormattable, new()
        {
            var result = new T();

            result.Serialize(this);

            return result;
        }

        public Position ReadPosition()
        {
            var pos = new Position
            {
                X = ReadUInt16(),
                Y = ReadUInt16()
            };
            return pos;
        }

        public string ReadStringA()
        {
            var length = ReadByte();
            var result = _encoding.GetString(Packet.Data, Position, length);

            Position += length;

            return result;
        }

        public string ReadStringB()
        {
            var length = ReadUInt16();
            var result = _encoding.GetString(Packet.Data, Position, length);

            Position += length;

            return result;
        }

        public ushort ReadUInt16()
        {
            return (ushort) ((
                                 ReadByte() << 8) |
                             ReadByte());
        }

        public uint ReadUInt32()
        {
            return (uint) ((
                               ReadUInt16() << 0x10) +
                           ReadUInt16());
        }
    }
}