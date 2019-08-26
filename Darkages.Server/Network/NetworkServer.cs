// ************************************************************************
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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Darkages.Common;

namespace Darkages.Network
{
    public abstract class NetworkServer<TClient> : ServerFormatStubs<TClient>, IDisposable
        where TClient : NetworkClient<TClient>, new()
    {
        private readonly MethodInfo[] _handlers;

        private bool _listening;

        public IPAddress Address;

        public TClient[] Clients;

        private readonly Cache<byte, NetworkFormat> _formatCache = new Cache<byte, NetworkFormat>();

        private AutoResetEvent _receivingStarted = new AutoResetEvent(false);

        protected NetworkServer(int capacity)
        {
            var type = typeof(NetworkServer<TClient>);

            Address = ServerContext.IPADDR;
            Clients = new TClient[capacity];

            _handlers = new MethodInfo[256];

            for (var i = 0; i < _handlers.Length; i++)
                _handlers[i] = type.GetMethod(
                    $"Format{i:X2}Handler",
                    BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public Socket Listener { get; set; }

        private void EndConnectClient(IAsyncResult result)
        {
            var handler = Listener.EndAccept(result);

            if (Listener == null || !_listening)
                return;

            if (!_listening)
                return;

            handler.UseOnlyOverlappedIO = true;

            var client = new TClient
            {
                ServerSocket = new NetworkSocket(handler)
                {
                    SendBufferSize      = 4096,
                    ReceiveBufferSize   = 4096,
                    UseOnlyOverlappedIO = true,
                }
            };

            if (client.ServerSocket.Connected)
            {
                if (AddClient(client))
                {
                    ClientConnected(client);

                    lock (Generator.Random)
                    {
                        client.Serial = Generator.GenerateNumber();
                    }

                    client.ServerSocket.BeginReceiveHeader(EndReceiveHeader, out var error, client);

                    if (error != SocketError.Success)
                        ClientDisconnected(client);
                }
                else
                {
                    ClientDisconnected(client);
                }
            }


            Listener.BeginAccept(EndConnectClient, Listener);
        }

        private void EndReceiveHeader(IAsyncResult result)
        {
            _receivingStarted.Set();

            try
            {
                if (!(result.AsyncState is TClient client))
                    return;

                var bytes = client.ServerSocket.EndReceiveHeader(result, out var error);

                if (bytes == 0 ||
                    error != SocketError.Success)
                {
                    ClientDisconnected(client);
                    return;
                }

                if (client.ServerSocket.HeaderComplete)
                {
                    client.ServerSocket.BeginReceivePacket(EndReceivePacket, out error, client);
                }
                else
                {
                    client.ServerSocket.BeginReceiveHeader(EndReceiveHeader, out error, client);
                }
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        private void EndReceivePacket(IAsyncResult result)
        {
            try
            {
                if (!(result.AsyncState is TClient client))
                    return;

                var bytes = client.ServerSocket.EndReceivePacket(result, out var error);

                if (bytes == 0 ||
                    error != SocketError.Success)
                {
                    ClientDisconnected(client);
                    return;
                }

                if (client.ServerSocket.PacketComplete)
                {
                    ClientDataReceived(client, client.ServerSocket.ToPacket());
                    client.ServerSocket.BeginReceiveHeader(EndReceiveHeader, out error, client);
                }
                else
                {
                    client.ServerSocket.BeginReceivePacket(EndReceivePacket, out error, client);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public virtual bool AddClient(TClient client)
        {
            var index = -1;

            lock (Clients)
            {
                for (var i = Clients.Length - 1; i >= 0; i--)
                    if (Clients[i] == null)
                    {
                        index = i;
                        break;
                    }


                if (index == -1)
                    return false;

                Clients[index] = client;
            }

            return true;
        }

        public void RemoveClient(int lpSerial)
        {
            lock (Clients)
            {
                for (var i = Clients.Length - 1; i >= 0; i--)
                    if (Clients[i] != null &&
                        Clients[i].Serial == lpSerial)
                    {
                        Clients[i] = null;
                        break;
                    }
            }
        }

        public virtual void Abort()
        {
            _listening = false;

            lock (Clients)
            {
                foreach (var client in Clients)
                    if (client != null)
                        ClientDisconnected(client);
            }

            if (Listener != null && Listener.Connected)
            {
                Listener.Shutdown(SocketShutdown.Both);
                Listener.Close();
                Listener = null;
            }
        }

        public virtual void Start(int port)
        {
            if (_listening)
                return;

            _listening = true;

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                UseOnlyOverlappedIO = true
            };
            {
                Listener.Bind(new IPEndPoint(IPAddress.Any, port));

                Listener.Listen(Clients.Length);

                Listener.BeginAccept(EndConnectClient, null);
            }
        }

        public virtual void ClientConnected(TClient client)
        {
            ServerContext.Logger?.Trace("Connection From {0} Established.",
                client.ServerSocket.RemoteEndPoint.ToString());
        }

        public virtual void ClientDataReceived(TClient client, NetworkPacket packet)
        {
            if (client == null)
                return;

            if (!client.ServerSocket.Connected)
                return;

            if (packet == null)
                return;

            NetworkFormat format;

            if (_formatCache.Exists(packet.Command))
            {
                format = _formatCache.Get(packet.Command);
            }
            else
            {
                format = NetworkFormatManager.GetClientFormat(packet.Command);
                _formatCache.AddOrUpdate(packet.Command, format, Timeout.Infinite);
            }

            if (format == null)
                return;

            try
            {
                _receivingStarted.WaitOne();
                client.Read(packet, format);

                if (_handlers[format.Command]?.Invoke(this,
                    new object[]
                    {
                        client,
                        format
                    }) != null)
                {
                }
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        public virtual void ClientDisconnected(TClient client)
        {
            if (client == null)
                return;

            if (client.ServerSocket != null &&
                client.ServerSocket.Connected)
            {
                client.ServerSocket.Shutdown(SocketShutdown.Both);
                client.ServerSocket.Close();
                client.ServerSocket = null;
            }

            RemoveClient(client.Serial);
        }

        #region IDisposable Implementation

        protected bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _formatCache?.Dispose();
                Listener?.Dispose();
            }

            disposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}