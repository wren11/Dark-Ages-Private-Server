using System;

namespace Darkages.Network
{
    public class NetworkPacket
    {
        public NetworkPacket(byte[] array, int start, int count)
        {
            Command = array[start + 0];
            Ordinal = array[start + 1];
            Data = new byte[count - 2];

            if (Data.Length != 0)
                Array.Copy(array, start + 2, Data, 0, Data.Length);
        }

        public byte Command { get; set; }
        public byte Ordinal { get; set; }
        public byte[] Data { get; set; }

        public byte[] ToArray()
        {
            var buffer = new byte[Data.Length + 5];

            buffer[0] = 0xAA;
            buffer[1] = (byte)((Data.Length + 2) >> 8);
            buffer[2] = (byte)((Data.Length + 2) >> 0);
            buffer[3] = Command;
            buffer[4] = Ordinal;

            for (var i = 0; i < Data.Length; i++)
                buffer[i + 5] = Data[i];

            return buffer;
        }

        public override string ToString()
        {
            return string.Format("{0}",
                BitConverter.ToString(this.ToArray()).Replace('-', ' '));
        }
    }
}