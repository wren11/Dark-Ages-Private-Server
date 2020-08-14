#region

using System;
using System.Globalization;
using System.Net.Sockets;

#endregion

namespace Darkages.Network
{
    public class NetworkPacket
    {
        public byte Ordinal { get; }
        public byte Command { get; }
        public byte[] Data { get; }

        public NetworkPacket(byte[] array, long count)
        {
            Command = array[0];

            if (Command == byte.MaxValue)
                return;

            Ordinal = array[1];
            Data = count - 2 > 0 ? new byte[count - 0x2] : new byte[count];

            if (Data.Length >= 2)
            {
                Buffer.BlockCopy(array, 2, Data, 0, Data.Length);
            }
        }

        public byte[] ToArray()
        {
            var buffer = new byte[Data.Length + 5];

            buffer[0] = 0xAA;
            buffer[1] = (byte)((Data.Length + 2) >> 8);
            buffer[2] = (byte)(Data.Length + 2);
            buffer[3] = Command;
            buffer[4] = Ordinal;

            for (var i = 0; i < Data.Length; i++)
                buffer[i + 5] = Data[i];

            return buffer;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:X2} {1:X2} {2}",
                Command,
                Ordinal,
                BitConverter.ToString(Data).Replace('-', ' '));
        }
    }
}