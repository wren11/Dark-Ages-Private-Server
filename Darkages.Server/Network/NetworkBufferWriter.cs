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
using Darkages.IO;
using System;
using System.Net;
using System.Text;

namespace Darkages.Network
{
    public class NetworkBufferWriter
    {
        public byte[] rawData;

        public int Position;

        public NetworkBufferWriter()
        {
            rawData = BufferPool.Take(0x35000);
        }

        public void Write(byte[] value)
        {
            Array.Copy(value, 0, rawData, Position, value.Length);
            Position += value.Length;
        }      

        public byte[] ToBuffer()
        {
            var nbuffer = BufferPool.Take(Position);
            {
                Array.Copy(rawData, 0, nbuffer, 0, Position);
            }
            return nbuffer;
        }

        public NetworkPacket ToPacket()
        {
            return new NetworkPacket(rawData, Position);
        }
    }
}
