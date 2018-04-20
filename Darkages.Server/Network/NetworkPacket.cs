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
