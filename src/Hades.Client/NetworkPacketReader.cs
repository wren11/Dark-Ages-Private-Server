#region

using System.Text;

#endregion

namespace Darkages.Network
{
    public class NetworkPacketReader
    {
        private readonly Encoding _encoding = Encoding.GetEncoding(0x3B5);

        public NetworkPacket Packet { get; set; }
        public int Position { get; set; }
        public bool CanRead => Position + 1 < Packet.Data.Length;

        public T ReadObject<T>()
            where T : IFormattable, new()
        {
            var result = new T();

            result.Serialize(this);

            return result;
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        public byte ReadByte()
        {
            byte b;

            if (Position == -1)
                b = Packet.Ordinal;
            else
                b = Packet.Data[Position];

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

        public short ReadInt16()
        {
            return (short) ReadUInt16();
        }

        public ushort ReadUInt16()
        {
            return (ushort) ((
                                 ReadByte() << 8) |
                             ReadByte());
        }

        public int ReadInt32()
        {
            return (int) ReadUInt32();
        }

        public uint ReadUInt32()
        {
            return (uint) ((
                               ReadUInt16() << 0x10) +
                           ReadUInt16());
        }
    }
}