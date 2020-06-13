#region

using System;
using System.Net.Sockets;

#endregion

namespace Darkages.Network
{
    public class NetworkSocket
    {
        private static readonly int headerLength = 3;

        private readonly byte[] header = new byte[0x0003];
        private readonly byte[] packet = new byte[0xFFFF];

        private int headerOffset;
        private int packetLength;
        private int packetOffset;

        public NetworkSocket(Socket socket)
        {
            ConnectedSocket = socket;
        }

        public Socket ConnectedSocket { get; }

        public bool HeaderComplete => headerOffset == headerLength;

        public bool PacketComplete => packetOffset == packetLength;

        public virtual IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state)
        {
            return ConnectedSocket.BeginReceive(
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
            return ConnectedSocket.BeginReceive(
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
            var bytes = ConnectedSocket.EndReceive(result, out error);

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
            var bytes = ConnectedSocket.EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            packetOffset += bytes;

            if (PacketComplete) headerOffset = 0;

            return bytes;
        }

        public NetworkPacket ToPacket()
        {
            return PacketComplete ? new NetworkPacket(packet, packetLength) : null;
        }
    }
}