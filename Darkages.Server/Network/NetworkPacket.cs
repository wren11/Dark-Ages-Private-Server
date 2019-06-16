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
        public byte Command;
        public byte Ordinal;
        public byte[] Data;

        public NetworkPacket(byte[] array, int count)
        {
           
                this.Command = array[0];
                this.Ordinal = array[1];
                this.Data = (count - 2 > 0) ? new byte[count - 2] : new byte[count];

                if (this.Data.Length != 0)
                {
                    Buffer.BlockCopy(array, 2, this.Data, 0, this.Data.Length);
                }
            
        }

        public byte[] ToArray()
        {
            var buffer = new byte[this.Data.Length + 5];

            buffer[0] = 0xAA;
            buffer[1] = (byte)((this.Data.Length + 2) >> 8);
            buffer[2] = (byte)((this.Data.Length + 2) >> 0);
            buffer[3] = this.Command;
            buffer[4] = this.Ordinal;

            for (int i = 0; i < this.Data.Length; i++)
            {
                buffer[i + 5] = this.Data[i];
            }

            return buffer;
        }

        public override string ToString()
        {
            return string.Format("{0:X2} {1:X2} {2}",
                this.Command,
                this.Ordinal,
                BitConverter.ToString(this.Data).Replace('-', ' '));
        }
    }
}
