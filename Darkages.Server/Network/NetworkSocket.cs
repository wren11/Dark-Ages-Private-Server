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

        private readonly byte[] header = new byte[0x0003];
        private int headerLength = 3;
        private int headerOffset;
        private readonly byte[] packet = new byte[0x4000];
        private int packetLength;
        private int packetOffset;

        public NetworkSocket(Socket socket)
            : base(socket.DuplicateAndClose(processId))
        {
        }

        public bool HeaderComplete => headerOffset == headerLength;

        public bool PacketComplete => packetOffset == packetLength;

        public IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state)
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

        public IAsyncResult BeginReceivePacket(AsyncCallback callback, out SocketError error, object state)
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

        public int EndReceiveHeader(IAsyncResult result, out SocketError error)
        {
            try
            {
                var bytes = EndReceive(result, out error);

                if (bytes == 0 ||
                    error != SocketError.Success)
                    return 0;

                headerOffset += bytes;

                if (!HeaderComplete)
                    return bytes;

                packetLength = (header[1] << 8) | header[2];
                packetOffset = 0;

                return bytes;
            }
            catch (Exception e)
            {
                error = SocketError.SocketError;
                ServerContext.Report(e);
                ServerContext.Report(error);
            }


            return 0;
        }

        public int EndReceivePacket(IAsyncResult result, out SocketError error)
        {
            var bytes = EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            packetOffset += bytes;

            if (!PacketComplete)
                return bytes;

            headerLength = 3;
            headerOffset = 0;

            return bytes;
        }

        public NetworkPacket ToPacket()
        {
            return PacketComplete ? new NetworkPacket(packet, packetLength) : null;
        }
    }
}