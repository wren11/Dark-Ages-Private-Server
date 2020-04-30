using System;

namespace Darkages.Network
{
    public interface INetworkServer<TClient> where TClient : NetworkClient<TClient>, new()
    {
        void EndConnectClient(IAsyncResult result);
        void EndReceiveHeader(IAsyncResult result);
        void EndReceivePacket(IAsyncResult result);
        bool AddClient(TClient client);
        void RemoveClient(int lpSerial);
        void Abort();
        void Start(int port);
        void ClientConnected(TClient client);
        void ClientDataReceived(TClient client, NetworkPacket packet);
        void ClientDisconnected(TClient client);
        void OnConnectedInternal(Session<TClient> session);
        void OnDisconnectedInternal(Session<TClient> session);
    }
}