using System;
using System.Net.Sockets;
using System.Reflection;

namespace Darkages.Network
{
    public class Client<TClient> : NetworkClient<TClient>
    {
        private readonly MethodInfo[] _handlers;
        public byte[] Buffer = new byte[8192];

        public Client()
        {
            var type = typeof(NetworkClient<TClient>);
            _handlers = new MethodInfo[256];

            for (var i = 0; i < _handlers.Length; i++)
                _handlers[i] = type.GetMethod(
                    $"Format{i:X2}Handler", BindingFlags.Public | BindingFlags.Instance);
        }

        private void EndReceiveHeader(IAsyncResult ar)
        {
            var bytes = Socket.EndReceiveHeader(ar, out var error);

            if (bytes == 0 ||
                error != SocketError.Success)
            {
                Socket.Disconnect(false);
                return;
            }

            if (Socket.HeaderComplete)
                Socket.BeginReceivePacket(EndReceivePacket, out error, this);
            else
                Socket.BeginReceiveHeader(EndReceiveHeader, out error, this);
        }

        private void EndReceivePacket(IAsyncResult ar)
        {
            var bytes = Socket.EndReceivePacket(ar, out var error);

            if (bytes == 0 ||
                error != SocketError.Success)
            {
                Socket.Disconnect(false);
                return;
            }

            if (Socket.PacketComplete)
            {
                ServerDataReceived(Socket.ToPacket());
                Socket.BeginReceiveHeader(EndReceiveHeader, out error, this);
            }
            else
            {
                Socket.BeginReceivePacket(EndReceivePacket, out error, this);
            }
        }

        private void ServerDataReceived(NetworkPacket packet)
        {
            var format = NetworkFormatManager.GetServerFormat(packet.Command);

            if (format != null)
                try
                {
                    Read(packet, format);
                    if (_handlers[format.Command] != null)
                        _handlers[format.Command].Invoke(this,
                            new object[]
                            {
                                format
                            });
                }
                catch (Exception)
                {
                    //ignore   
                }
        }

        public void Connect(string ip, int port, out SocketError error)
        {
            try
            {
                Socket = new NetworkSocket(
                    new Socket(
                        SocketType.Stream,
                        ProtocolType.Tcp));

                Socket.Connect(ip, port);
                Socket.BeginReceiveHeader(EndReceiveHeader, out error, this);
            }
            catch (SocketException)
            {
                error = SocketError.NotConnected;
            }
        }

        public override void Read(NetworkPacket packet, NetworkFormat format)
        {
            if (format.Secured)
                Encryption.Transform(packet);

            Reader.Packet = packet;
            format.Serialize(Reader);

            if (format.Command == 0x7E)
            {
                SendMessageBox(0x00, "baram");
            }

            logger.Info("Client Proxy: {0}", format.Command);
        }
    }
}