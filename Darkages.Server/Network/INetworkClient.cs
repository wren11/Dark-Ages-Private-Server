using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Darkages.Security;

namespace Darkages.Network
{
    public interface INetworkClient<TClient> where TClient : NetworkClient<TClient>
    {
        NetworkPacketReader Reader { get; set; }
        NetworkPacketWriter Writer { get; set; }
        int Serial { get; set; }
        void Read(NetworkPacket packet, NetworkFormat format);
        NetworkClient<TClient> Send(NetworkFormat format);
        void Send(NetworkPacketWriter lpData);
        void Send(byte[] data);
        NetworkSocket ServerSocket { get; set; }
    }
}