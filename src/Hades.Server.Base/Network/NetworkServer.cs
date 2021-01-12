#region

using Darkages.Common;
using Darkages.Network.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Darkages.Network.ClientFormats;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Network
{
    public abstract partial class NetworkServer<TClient> : ObjectManager
        where TClient : NetworkClient, new()
    {
        public Dictionary<int, TClient> ConnectedClients;
        private readonly MethodInfo[] _handlers;
        private Socket _listener;
        private bool _listening;

        protected NetworkServer(int capacity = 2048)
        {
            var type = typeof(NetworkServer<TClient>);

            Address = ServerContext.IpAddress;
            ConnectedClients = new Dictionary<int, TClient>(capacity);

            _handlers = new MethodInfo[256];

            for (var i = 0; i < _handlers.Length; i++)
                _handlers[i] = type.GetMethod($"Format{i:X2}Handler", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public IPAddress Address { get; }

        public List<TClient> Clients => ConnectedClients.Values.ToList();

        public virtual void Abort()
        {
            _listening = false;

            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }

            lock (Clients)
            {
                foreach (var client in Clients.Where(client => client != null))
                    ClientDisconnected(client);
            }
        }

        public virtual bool AddClient(TClient client)
        {
            if (!ConnectedClients.ContainsKey(client.Serial))
                ConnectedClients.Add(client.Serial, client);

            return true;
        }

        public virtual void ClientConnected(TClient client)
        {
            if (ServerContext.Game == null)
                return;

            ServerContext.Logger($"Connection From {0} Established. {client.Socket.RemoteEndPoint}");
        }

        public virtual void ClientDataReceived(TClient client, NetworkPacket packet)
        {
            var format = NetworkFormatManager.GetClientFormat(packet.Command);

            if (format == null)
                return;

            try
            {
                if (!Clients.Exists(i => i.Serial == client.Serial))
                    return;

                client.Read(packet, format);
                client.LastMessageFromClient = DateTime.UtcNow;

                if (_handlers[format.Command] != null)
                    _handlers[format.Command].Invoke(this,
                        new object[]
                        {
                            client,
                            format
                        });
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
        }

        public virtual void ClientDisconnected(TClient client)
        {
            if (client == null)
                return;

            if (client.Socket.Connected)
                client.Socket.Disconnect(false);

            RemoveClient(client);
        }

        public void RemoveClient(TClient client)
        {
            if (client != null && ConnectedClients != null && ConnectedClients.ContainsKey(client.Serial))
                ConnectedClients.Remove(client.Serial);
        }

        public virtual void Start(int port)
        {
            if (_listening)
                return;

            _listening = true;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(ServerContext.Config?.ConnectionCapacity ?? 1000);
            _listener.BeginAccept(EndConnectClient, _listener);
        }

        private void EndConnectClient(IAsyncResult result)
        {
            try
            {
                if (_listener == null || !_listening) return;

                var client = new TClient
                {
                    State = new NetworkSocket(_listener.EndAccept(result)),
                };

                if (client.Socket.Connected)
                {
                    lock (Generator.Random)
                    {
                        client.Serial = Generator.GenerateNumber();
                    }

                    if (AddClient(client))
                    {
                        ClientConnected(client);
                        client.State.BeginReceiveHeader(EndReceiveHeader, client);
                    }
                    else
                    {
                        ServerContext.Logger("Client could not be added.");
                        ClientDisconnected(client);
                    }
                }

                if (_listening)
                    _listener.BeginAccept(EndConnectClient, null);
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
        }

        private void EndReceiveHeader(IAsyncResult result)
        {
            try
            {
                if (!(result.AsyncState is TClient client))
                    return;

                var bytes = client.State.EndReceiveHeader(result);

                if (bytes == 0)
                {
                    ClientDisconnected(client);
                    return;
                }

                if (client.State.HeaderComplete)
                    client.State.BeginReceivePacket(EndReceivePacket, client);
                else
                    client.State.BeginReceiveHeader(EndReceiveHeader, client);
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
        }

        private void EndReceivePacket(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState is TClient client)
                {
                    var bytes = client.State.EndReceivePacket(result);

                    if (bytes == 0)
                    {
                        ClientDisconnected(client);
                        return;
                    }

                    if (client.State.PacketComplete)
                    {
                        ClientDataReceived(client, client.State.ToPacket());
                        client.State.BeginReceiveHeader(EndReceiveHeader, client);
                    }
                    else
                    {
                        client.State.BeginReceivePacket(EndReceivePacket, client);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
        }

        #region Format Handlers

        #endregion
    }
}