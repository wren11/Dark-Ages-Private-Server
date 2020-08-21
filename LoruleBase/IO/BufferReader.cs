#region

using System.IO;
using System.Net;
using System.Text;

#endregion

namespace Darkages.IO
{
    public class BufferReader : BinaryReader
    {
        private readonly Encoding encoding = Encoding.GetEncoding(949);

        public BufferReader(Stream stream)
            : base(stream, Encoding.GetEncoding(949))
        {
        }

        public override short ReadInt16()
        {
            return (short) ReadUInt16();
        }

        public override int ReadInt32()
        {
            return (int) ReadUInt32();
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

        public override ushort ReadUInt16()
        {
            return (ushort) ((
                                 ReadByte() << 8) |
                             ReadByte());
        }

        public override uint ReadUInt32()
        {
            return (uint) ((
                               ReadUInt16() << 16) |
                           ReadUInt16());
        }
    }
}