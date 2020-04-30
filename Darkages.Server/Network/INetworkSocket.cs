using System;
using System.Net.Sockets;

namespace Darkages.Network
{
    public interface INetworkSocket
    {
        bool HeaderComplete { get; }
        bool PacketComplete { get; }
        IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state);
        IAsyncResult BeginReceivePacket(AsyncCallback callback, out SocketError error, object state);
        int EndReceiveHeader(IAsyncResult result, out SocketError error);
        int EndReceivePacket(IAsyncResult result, out SocketError error);
        NetworkPacket ToPacket();
    }
}