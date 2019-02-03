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
using System.Diagnostics;
using System.Net.Sockets;

namespace Darkages.Network
{
    public class NetworkSocket : Socket
    {
        private static readonly int processId = Process.GetCurrentProcess().Id;
        private static readonly int headerLength = 3;

        public readonly byte[] header = new byte[0x0003];
        public readonly byte[] packet = new byte[0x10000];

        private int headerOffset;
        private int packetLength;
        private int packetOffset;

        public NetworkSocket(Socket socket)
            : base(socket.DuplicateAndClose(processId))
        {

        }

        public bool HeaderComplete => headerOffset == headerLength;

        public bool PacketComplete => packetOffset == packetLength;

        public virtual IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state)
        {
            return BeginReceive(
                header,
                headerOffset,
                headerLength - headerOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public virtual IAsyncResult BeginReceivePacket(AsyncCallback callback, out SocketError error, object state)
        {
            return BeginReceive(
                packet,
                packetOffset,
                packetLength - packetOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public virtual int EndReceiveHeader(IAsyncResult result, out SocketError error)
        {
            var bytes = EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            headerOffset += bytes;

            if (HeaderComplete)
            {
                packetLength = (header[1] << 8) | header[2];
                packetOffset = 0;
            }

            return bytes;
        }

        public virtual int EndReceivePacket(IAsyncResult result, out SocketError error)
        {
            var bytes = EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            packetOffset += bytes;

            if (PacketComplete)
                headerOffset = 0;

            return bytes;
        }

        public NetworkPacket ToPacket()
        {
            return PacketComplete ? new NetworkPacket(packet, packetLength) : null;
        }
    }
}
