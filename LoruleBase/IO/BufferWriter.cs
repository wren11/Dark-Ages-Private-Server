#region

using System.IO;
using System.Net;
using System.Text;

#endregion

namespace Darkages.IO
{
    public class BufferWriter : BinaryWriter
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(949);

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

        public override void Write(string value)
        {
            base.Write(Encoding.GetBytes(value + '\0'));
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

        public void WriteStringA(string value)
        {
            var length = (byte) Encoding.GetByteCount(value);

            base.Write(length);
            base.Write(Encoding.GetBytes(value));
        }

        public void WriteStringB(string value)
        {
            var length = (ushort) Encoding.GetByteCount(value);

            Write(length);
            base.Write(Encoding.GetBytes(value));
        }
    }
}