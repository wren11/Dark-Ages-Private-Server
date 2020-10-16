#region

using System;
// ReSharper disable ShiftExpressionRealShiftCountIsZero

#endregion

namespace Darkages.Network
{
    public sealed class NetworkPacket
    {
        public NetworkPacket(byte[] array, int count)
        {
            Command = array[0];
            Ordinal = array[1];
            Data = new byte[count - 2];

            if (Data.Length != 0)
                Array.Copy(array, 2, Data, 0, Data.Length);
        }

        public byte Command { get; set; }
        public byte Ordinal { get; set; }
        public byte[] Data { get; set; }

        public unsafe byte[] ToArray()
        {
            var buffer = new byte[Data.Length + 5];

            buffer[0] = 0xAA;
            buffer[1] = (byte) ((Data.Length + 2) >> 8);
            buffer[2] = (byte) ((Data.Length + 2) >> 0);
            buffer[3] = Command;
            buffer[4] = Ordinal;

            fixed (byte* pA = Data, pB = buffer)
            {
                for (var i = 0; i < Data.Length; i++)
                    pB[i + 5] = pA[i];
            }

            return buffer;
        }

        public override string ToString()
        {
            return string.Format("{0}",
                BitConverter.ToString(ToArray()).Replace('-', ' '));
        }
    }
}