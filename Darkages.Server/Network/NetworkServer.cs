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
using Darkages.Common;
using Darkages.Network.ClientFormats;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Darkages.Network
{
    public abstract class NetworkServer<TClient> : ObjectManager
        where TClient : NetworkClient<TClient>, new()
    {
        private readonly MethodInfo[] _handlers;

        private Cache<byte, NetworkFormat> FormatCache = new Cache<byte, NetworkFormat>();

        private bool _listening;

        public IPAddress Address;

        public TClient[] Clients;

        protected NetworkServer(int capacity)
        {
            var type  = typeof(NetworkServer<TClient>);

            Address   = ServerContext.Ipaddress;
            Clients   = new TClient[capacity];

            _handlers = new MethodInfo[256];

            for (var i = 0; i < _handlers.Length; i++)
                _handlers[i] = type.GetMethod(
                    $"Format{i:X2}Handler",
                    BindingFlags.NonPublic | BindingFlags.Instance);

        }

        private void EndConnectClient(IAsyncResult result)
        {
            var _listener = (Socket)result.AsyncState;

            var _handler = _listener.EndAccept(result);

            if (_listener == null || !_listening)
                return;

            if (_listening)
            {

                var client = new TClient
                {
                    WorkSocket   = new NetworkSocket(_handler),
                };

                if (client.WorkSocket.Connected)
                {
                    if (AddClient(client))
                    {
                        ClientConnected(client);

                        lock (Generator.Random)
                        {
                            client.Serial = Generator.GenerateNumber();
                        }

                        client.WorkSocket.BeginReceiveHeader(new AsyncCallback(EndReceiveHeader), out var error, client);

                        if (error != SocketError.Success)
                            ClientDisconnected(client);
                    }
                    else
                    {
                        ClientDisconnected(client);
                    }
                }


                _listener.BeginAccept(new AsyncCallback(EndConnectClient), _listener);
            }
        }

        private void EndReceiveHeader(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState is TClient client)
                {
                    var bytes = client.WorkSocket.EndReceiveHeader(result, out var error);

                    if (bytes == 0 ||
                        error != SocketError.Success)
                    {
                        ClientDisconnected(client);
                        return;
                    }

                    if (client.WorkSocket.HeaderComplete)
                    {
                        client.WorkSocket.BeginReceivePacket(new AsyncCallback(EndReceivePacket), out error, client);
                    }
                    else
                    {
                        client.WorkSocket.BeginReceiveHeader(new AsyncCallback(EndReceiveHeader), out error, client);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void EndReceivePacket(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState is TClient client)
                {
                    var bytes = client.WorkSocket.EndReceivePacket(result, out var error);

                    if (bytes == 0 ||
                        error != SocketError.Success)
                    {
                        ClientDisconnected(client);
                        return;
                    }

                    if (client.WorkSocket.PacketComplete)
                    {
                        ClientDataReceived(client, client.WorkSocket.ToPacket());

                        client.WorkSocket.BeginReceiveHeader(new AsyncCallback(EndReceiveHeader), out error, client);
                    }
                    else
                    {
                        client.WorkSocket.BeginReceivePacket(new AsyncCallback(EndReceivePacket), out error, client);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public virtual bool AddClient(TClient client)
        {
            var index = -1;

            for (var i = Clients.Length - 1; i >= 0; i--)
                if (Clients[i] == null)
                {
                    index = i;
                    break;
                }

            if (index == -1)
                return false;

            Clients[index] = client;
            return true;
        }

        public void RemoveClient(int Serial)
        {
            lock (Clients)
            {
                for (var i = Clients.Length - 1; i >= 0; i--)
                    if (Clients[i] != null &&
                        Clients[i].Serial == Serial)
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
        }

        public virtual void Start(int port)
        {
            if (_listening)
                return;

            _listening = true;

            var _listener  = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            {
                _listener.Bind(new IPEndPoint(IPAddress.Any, port));
                _listener.Listen(Clients.Length);
                _listener.BeginAccept(new AsyncCallback(EndConnectClient), _listener);
            }
        }

        public virtual void ClientConnected(TClient client)
        {
            ServerContext.Info?.Warning("Connection From {0} Established.", client.WorkSocket.RemoteEndPoint.ToString());
        }

        public virtual void ClientDataReceived(TClient client, NetworkPacket packet)
        {
            if (client == null)
                return;

            if (!client.WorkSocket.Connected)
                return;

            if (packet == null)
                return;

            NetworkFormat format;

            if (FormatCache.Exists(packet.Command))
            {
                format = FormatCache.Get(packet.Command);
            }
            else
            {
                format = NetworkFormatManager.GetClientFormat(packet.Command);
                FormatCache.AddOrUpdate(packet.Command, format, Timeout.Infinite);
            }

            if (format != null)
            {
                try
                {
                    client.Read(packet, format);

                    _handlers[format.Command]?.Invoke(this,
                        new object[]
                        {
                                client,
                                format
                        });
                }
                catch (Exception)
                {
                    //ignore   
                }
            }
        }

        public virtual void ClientDisconnected(TClient client)
        {
            if (client == null)
                return;

            if (client.WorkSocket != null &&
                client.WorkSocket.Connected)
            {
                client.WorkSocket.Shutdown(SocketShutdown.Both);
                client.WorkSocket.Close();
            }

            RemoveClient(client.Serial);
        }

        private void RemoveAisling(TClient client)
        {
            if (ServerContext.Game == null)
                return;

            var nearby = GetObjects<Aisling>((client as GameClient).Aisling.Map, i => i.WithinRangeOf((client as GameClient).Aisling));
            foreach (var near in nearby)
            {
                if (near.Serial == (client as GameClient).Aisling.Serial)
                    continue;

                if (near.LoggedIn)
                {
                    if (near.Map != null && near.Map.Ready)
                        near.Map.Update(
                            (client as GameClient).Aisling.X,
                            (client as GameClient).Aisling.Y, (client as GameClient).Aisling, true);

                    near.Show(Scope.Self, new ServerFormat0E((client as GameClient).Aisling.Serial));
                }
            }
        }


        public virtual void ClientHandler(object obj)
        {

        }

        #region Format Handlers

        protected virtual void Format00Handler(TClient client, ClientFormat00 format)
        {
        }

        protected virtual void Format01Handler(TClient client, ClientFormat01 format)
        {
        }

        protected virtual void Format02Handler(TClient client, ClientFormat02 format)
        {
        }

        protected virtual void Format03Handler(TClient client, ClientFormat03 format)
        {
        }

        protected virtual void Format04Handler(TClient client, ClientFormat04 format)
        {
        }

        protected virtual void Format05Handler(TClient client, ClientFormat05 format)
        {
        }

        protected virtual void Format06Handler(TClient client, ClientFormat06 format)
        {
        }

        protected virtual void Format07Handler(TClient client, ClientFormat07 format)
        {
        }

        protected virtual void Format08Handler(TClient client, ClientFormat08 format)
        {
        }

        protected virtual void Format09Handler(TClient client, ClientFormat09 format)
        {
        }

        protected virtual void Format0AHandler(TClient client, ClientFormat0A format)
        {
        }

        protected virtual void Format0BHandler(TClient client, ClientFormat0B format)
        {
        }

        protected virtual void Format0CHandler(TClient client, ClientFormat0C format)
        {
        }

        protected virtual void Format0DHandler(TClient client, ClientFormat0D format)
        {
        }

        protected virtual void Format0EHandler(TClient client, ClientFormat0E format)
        {
        }

        protected virtual void Format0FHandler(TClient client, ClientFormat0F format)
        {
        }

        protected virtual void Format10Handler(TClient client, ClientFormat10 format)
        {
        }

        protected virtual void Format11Handler(TClient client, ClientFormat11 format)
        {
        }

        protected virtual void Format12Handler(TClient client, ClientFormat12 format)
        {
        }

        protected virtual void Format13Handler(TClient client, ClientFormat13 format)
        {
        }

        protected virtual void Format14Handler(TClient client, ClientFormat14 format)
        {
        }

        protected virtual void Format15Handler(TClient client, ClientFormat15 format)
        {
        }

        protected virtual void Format16Handler(TClient client, ClientFormat16 format)
        {
        }

        protected virtual void Format17Handler(TClient client, ClientFormat17 format)
        {
        }

        protected virtual void Format18Handler(TClient client, ClientFormat18 format)
        {
        }

        protected virtual void Format19Handler(TClient client, ClientFormat19 format)
        {
        }

        protected virtual void Format1AHandler(TClient client, ClientFormat1A format)
        {
        }

        protected virtual void Format1BHandler(TClient client, ClientFormat1B format)
        {
        }

        protected virtual void Format1CHandler(TClient client, ClientFormat1C format)
        {
        }

        protected virtual void Format1DHandler(TClient client, ClientFormat1D format)
        {
        }

        protected virtual void Format1EHandler(TClient client, ClientFormat1E format)
        {
        }

        protected virtual void Format1FHandler(TClient client, ClientFormat1F format)
        {
        }

        protected virtual void Format20Handler(TClient client, ClientFormat20 format)
        {
        }

        protected virtual void Format21Handler(TClient client, ClientFormat21 format)
        {
        }

        protected virtual void Format22Handler(TClient client, ClientFormat22 format)
        {
        }

        protected virtual void Format23Handler(TClient client, ClientFormat23 format)
        {
        }

        protected virtual void Format24Handler(TClient client, ClientFormat24 format)
        {
        }

        protected virtual void Format25Handler(TClient client, ClientFormat25 format)
        {
        }

        protected virtual void Format26Handler(TClient client, ClientFormat26 format)
        {
        }

        protected virtual void Format27Handler(TClient client, ClientFormat27 format)
        {
        }

        protected virtual void Format28Handler(TClient client, ClientFormat28 format)
        {
        }

        protected virtual void Format29Handler(TClient client, ClientFormat29 format)
        {
        }

        protected virtual void Format2AHandler(TClient client, ClientFormat2A format)
        {
        }

        protected virtual void Format2BHandler(TClient client, ClientFormat2B format)
        {
        }

        protected virtual void Format2CHandler(TClient client, ClientFormat2C format)
        {
        }

        protected virtual void Format2DHandler(TClient client, ClientFormat2D format)
        {
        }

        protected virtual void Format2EHandler(TClient client, ClientFormat2E format)
        {
        }

        protected virtual void Format2FHandler(TClient client, ClientFormat2F format)
        {
        }

        protected virtual void Format30Handler(TClient client, ClientFormat30 format)
        {
        }

        protected virtual void Format31Handler(TClient client, ClientFormat31 format)
        {
        }

        protected virtual void Format32Handler(TClient client, ClientFormat32 format)
        {
        }

        protected virtual void Format33Handler(TClient client, ClientFormat33 format)
        {
        }

        protected virtual void Format34Handler(TClient client, ClientFormat34 format)
        {
        }

        protected virtual void Format35Handler(TClient client, ClientFormat35 format)
        {
        }

        protected virtual void Format36Handler(TClient client, ClientFormat36 format)
        {
        }

        protected virtual void Format37Handler(TClient client, ClientFormat37 format)
        {
        }

        protected virtual void Format38Handler(TClient client, ClientFormat38 format)
        {
        }

        protected virtual void Format39Handler(TClient client, ClientFormat39 format)
        {
        }

        protected virtual void Format3AHandler(TClient client, ClientFormat3A format)
        {
        }

        protected virtual void Format3BHandler(TClient client, ClientFormat3B format)
        {
        }

        protected virtual void Format3CHandler(TClient client, ClientFormat3C format)
        {
        }

        protected virtual void Format3DHandler(TClient client, ClientFormat3D format)
        {
        }

        protected virtual void Format3EHandler(TClient client, ClientFormat3E format)
        {
        }

        protected virtual void Format3FHandler(TClient client, ClientFormat3F format)
        {
        }

        protected virtual void Format40Handler(TClient client, ClientFormat40 format)
        {
        }

        protected virtual void Format41Handler(TClient client, ClientFormat41 format)
        {
        }

        protected virtual void Format42Handler(TClient client, ClientFormat42 format)
        {
        }

        protected virtual void Format43Handler(TClient client, ClientFormat43 format)
        {
        }

        protected virtual void Format44Handler(TClient client, ClientFormat44 format)
        {
        }

        protected virtual void Format45Handler(TClient client, ClientFormat45 format)
        {
        }

        protected virtual void Format46Handler(TClient client, ClientFormat46 format)
        {
        }

        protected virtual void Format47Handler(TClient client, ClientFormat47 format)
        {
        }

        protected virtual void Format48Handler(TClient client, ClientFormat48 format)
        {
        }

        protected virtual void Format49Handler(TClient client, ClientFormat49 format)
        {
        }

        protected virtual void Format4AHandler(TClient client, ClientFormat4A format)
        {
        }

        protected virtual void Format4BHandler(TClient client, ClientFormat4B format)
        {
        }

        protected virtual void Format4CHandler(TClient client, ClientFormat4C format)
        {
        }

        protected virtual void Format4DHandler(TClient client, ClientFormat4D format)
        {
        }

        protected virtual void Format4EHandler(TClient client, ClientFormat4E format)
        {
        }

        protected virtual void Format4FHandler(TClient client, ClientFormat4F format)
        {
        }

        protected virtual void Format50Handler(TClient client, ClientFormat50 format)
        {
        }

        protected virtual void Format51Handler(TClient client, ClientFormat51 format)
        {
        }

        protected virtual void Format52Handler(TClient client, ClientFormat52 format)
        {
        }

        protected virtual void Format53Handler(TClient client, ClientFormat53 format)
        {
        }

        protected virtual void Format54Handler(TClient client, ClientFormat54 format)
        {
        }

        protected virtual void Format55Handler(TClient client, ClientFormat55 format)
        {
        }

        protected virtual void Format56Handler(TClient client, ClientFormat56 format)
        {
        }

        protected virtual void Format57Handler(TClient client, ClientFormat57 format)
        {
        }

        protected virtual void Format58Handler(TClient client, ClientFormat58 format)
        {
        }

        protected virtual void Format59Handler(TClient client, ClientFormat59 format)
        {
        }

        protected virtual void Format5AHandler(TClient client, ClientFormat5A format)
        {
        }

        protected virtual void Format5BHandler(TClient client, ClientFormat5B format)
        {
        }

        protected virtual void Format5CHandler(TClient client, ClientFormat5C format)
        {
        }

        protected virtual void Format5DHandler(TClient client, ClientFormat5D format)
        {
        }

        protected virtual void Format5EHandler(TClient client, ClientFormat5E format)
        {
        }

        protected virtual void Format5FHandler(TClient client, ClientFormat5F format)
        {
        }

        protected virtual void Format60Handler(TClient client, ClientFormat60 format)
        {
        }

        protected virtual void Format61Handler(TClient client, ClientFormat61 format)
        {
        }

        protected virtual void Format62Handler(TClient client, ClientFormat62 format)
        {
        }

        protected virtual void Format63Handler(TClient client, ClientFormat63 format)
        {
        }

        protected virtual void Format64Handler(TClient client, ClientFormat64 format)
        {
        }

        protected virtual void Format65Handler(TClient client, ClientFormat65 format)
        {
        }

        protected virtual void Format66Handler(TClient client, ClientFormat66 format)
        {
        }

        protected virtual void Format67Handler(TClient client, ClientFormat67 format)
        {
        }

        protected virtual void Format68Handler(TClient client, ClientFormat68 format)
        {
        }

        protected virtual void Format69Handler(TClient client, ClientFormat69 format)
        {
        }

        protected virtual void Format6AHandler(TClient client, ClientFormat6A format)
        {
        }

        protected virtual void Format6BHandler(TClient client, ClientFormat6B format)
        {
        }

        protected virtual void Format6CHandler(TClient client, ClientFormat6C format)
        {
        }

        protected virtual void Format6DHandler(TClient client, ClientFormat6D format)
        {
        }

        protected virtual void Format6EHandler(TClient client, ClientFormat6E format)
        {
        }

        protected virtual void Format6FHandler(TClient client, ClientFormat6F format)
        {
        }

        protected virtual void Format70Handler(TClient client, ClientFormat70 format)
        {
        }

        protected virtual void Format71Handler(TClient client, ClientFormat71 format)
        {
        }

        protected virtual void Format72Handler(TClient client, ClientFormat72 format)
        {
        }

        protected virtual void Format73Handler(TClient client, ClientFormat73 format)
        {
        }

        protected virtual void Format74Handler(TClient client, ClientFormat74 format)
        {
        }

        protected virtual void Format75Handler(TClient client, ClientFormat75 format)
        {
        }

        protected virtual void Format76Handler(TClient client, ClientFormat76 format)
        {
        }

        protected virtual void Format77Handler(TClient client, ClientFormat77 format)
        {
        }

        protected virtual void Format78Handler(TClient client, ClientFormat78 format)
        {
        }

        protected virtual void Format79Handler(TClient client, ClientFormat79 format)
        {
        }

        protected virtual void Format7AHandler(TClient client, ClientFormat7A format)
        {
        }

        protected virtual void Format7BHandler(TClient client, ClientFormat7B format)
        {
        }

        protected virtual void Format7CHandler(TClient client, ClientFormat7C format)
        {
        }

        protected virtual void Format7DHandler(TClient client, ClientFormat7D format)
        {
        }

        protected virtual void Format7EHandler(TClient client, ClientFormat7E format)
        {
        }

        protected virtual void Format7FHandler(TClient client, ClientFormat7F format)
        {
        }

        protected virtual void Format80Handler(TClient client, ClientFormat80 format)
        {
        }

        protected virtual void Format81Handler(TClient client, ClientFormat81 format)
        {
        }

        protected virtual void Format82Handler(TClient client, ClientFormat82 format)
        {
        }

        protected virtual void Format83Handler(TClient client, ClientFormat83 format)
        {
        }

        protected virtual void Format84Handler(TClient client, ClientFormat84 format)
        {
        }

        protected virtual void Format85Handler(TClient client, ClientFormat85 format)
        {
        }

        protected virtual void Format86Handler(TClient client, ClientFormat86 format)
        {
        }

        protected virtual void Format87Handler(TClient client, ClientFormat87 format)
        {
        }

        protected virtual void Format88Handler(TClient client, ClientFormat88 format)
        {
        }

        protected virtual void Format89Handler(TClient client, ClientFormat89 format)
        {
        }

        protected virtual void Format8AHandler(TClient client, ClientFormat8A format)
        {
        }

        protected virtual void Format8BHandler(TClient client, ClientFormat8B format)
        {
        }

        protected virtual void Format8CHandler(TClient client, ClientFormat8C format)
        {
        }

        protected virtual void Format8DHandler(TClient client, ClientFormat8D format)
        {
        }

        protected virtual void Format8EHandler(TClient client, ClientFormat8E format)
        {
        }

        protected virtual void Format8FHandler(TClient client, ClientFormat8F format)
        {
        }

        protected virtual void Format90Handler(TClient client, ClientFormat90 format)
        {
        }

        protected virtual void Format91Handler(TClient client, ClientFormat91 format)
        {
        }

        protected virtual void Format92Handler(TClient client, ClientFormat92 format)
        {
        }

        protected virtual void Format93Handler(TClient client, ClientFormat93 format)
        {
        }

        protected virtual void Format94Handler(TClient client, ClientFormat94 format)
        {
        }

        protected virtual void Format95Handler(TClient client, ClientFormat95 format)
        {
        }

        protected virtual void Format96Handler(TClient client, ClientFormat96 format)
        {
        }

        protected virtual void Format97Handler(TClient client, ClientFormat97 format)
        {
        }

        protected virtual void Format98Handler(TClient client, ClientFormat98 format)
        {
        }

        protected virtual void Format99Handler(TClient client, ClientFormat99 format)
        {
        }

        protected virtual void Format9AHandler(TClient client, ClientFormat9A format)
        {
        }

        protected virtual void Format9BHandler(TClient client, ClientFormat9B format)
        {
        }

        protected virtual void Format9CHandler(TClient client, ClientFormat9C format)
        {
        }

        protected virtual void Format9DHandler(TClient client, ClientFormat9D format)
        {
        }

        protected virtual void Format9EHandler(TClient client, ClientFormat9E format)
        {
        }

        protected virtual void Format9FHandler(TClient client, ClientFormat9F format)
        {
        }

        protected virtual void FormatA0Handler(TClient client, ClientFormatA0 format)
        {
        }

        protected virtual void FormatA1Handler(TClient client, ClientFormatA1 format)
        {
        }

        protected virtual void FormatA2Handler(TClient client, ClientFormatA2 format)
        {
        }

        protected virtual void FormatA3Handler(TClient client, ClientFormatA3 format)
        {
        }

        protected virtual void FormatA4Handler(TClient client, ClientFormatA4 format)
        {
        }

        protected virtual void FormatA5Handler(TClient client, ClientFormatA5 format)
        {
        }

        protected virtual void FormatA6Handler(TClient client, ClientFormatA6 format)
        {
        }

        protected virtual void FormatA7Handler(TClient client, ClientFormatA7 format)
        {
        }

        protected virtual void FormatA8Handler(TClient client, ClientFormatA8 format)
        {
        }

        protected virtual void FormatA9Handler(TClient client, ClientFormatA9 format)
        {
        }

        protected virtual void FormatAAHandler(TClient client, ClientFormatAA format)
        {
        }

        protected virtual void FormatABHandler(TClient client, ClientFormatAB format)
        {
        }

        protected virtual void FormatACHandler(TClient client, ClientFormatAC format)
        {
        }

        protected virtual void FormatADHandler(TClient client, ClientFormatAD format)
        {
        }

        protected virtual void FormatAEHandler(TClient client, ClientFormatAE format)
        {
        }

        protected virtual void FormatAFHandler(TClient client, ClientFormatAF format)
        {
        }

        protected virtual void FormatB0Handler(TClient client, ClientFormatB0 format)
        {
        }

        protected virtual void FormatB1Handler(TClient client, ClientFormatB1 format)
        {
        }

        protected virtual void FormatB2Handler(TClient client, ClientFormatB2 format)
        {
        }

        protected virtual void FormatB3Handler(TClient client, ClientFormatB3 format)
        {
        }

        protected virtual void FormatB4Handler(TClient client, ClientFormatB4 format)
        {
        }

        protected virtual void FormatB5Handler(TClient client, ClientFormatB5 format)
        {
        }

        protected virtual void FormatB6Handler(TClient client, ClientFormatB6 format)
        {
        }

        protected virtual void FormatB7Handler(TClient client, ClientFormatB7 format)
        {
        }

        protected virtual void FormatB8Handler(TClient client, ClientFormatB8 format)
        {
        }

        protected virtual void FormatB9Handler(TClient client, ClientFormatB9 format)
        {
        }

        protected virtual void FormatBAHandler(TClient client, ClientFormatBA format)
        {
        }

        protected virtual void FormatBBHandler(TClient client, ClientFormatBB format)
        {
        }

        protected virtual void FormatBCHandler(TClient client, ClientFormatBC format)
        {
        }

        protected virtual void FormatBDHandler(TClient client, ClientFormatBD format)
        {
        }

        protected virtual void FormatBEHandler(TClient client, ClientFormatBE format)
        {
        }

        protected virtual void FormatBFHandler(TClient client, ClientFormatBF format)
        {
        }

        protected virtual void FormatC0Handler(TClient client, ClientFormatC0 format)
        {
        }

        protected virtual void FormatC1Handler(TClient client, ClientFormatC1 format)
        {
        }

        protected virtual void FormatC2Handler(TClient client, ClientFormatC2 format)
        {
        }

        protected virtual void FormatC3Handler(TClient client, ClientFormatC3 format)
        {
        }

        protected virtual void FormatC4Handler(TClient client, ClientFormatC4 format)
        {
        }

        protected virtual void FormatC5Handler(TClient client, ClientFormatC5 format)
        {
        }

        protected virtual void FormatC6Handler(TClient client, ClientFormatC6 format)
        {
        }

        protected virtual void FormatC7Handler(TClient client, ClientFormatC7 format)
        {
        }

        protected virtual void FormatC8Handler(TClient client, ClientFormatC8 format)
        {
        }

        protected virtual void FormatC9Handler(TClient client, ClientFormatC9 format)
        {
        }

        protected virtual void FormatCAHandler(TClient client, ClientFormatCA format)
        {
        }

        protected virtual void FormatCBHandler(TClient client, ClientFormatCB format)
        {
        }

        protected virtual void FormatCCHandler(TClient client, ClientFormatCC format)
        {
        }

        protected virtual void FormatCDHandler(TClient client, ClientFormatCD format)
        {
        }

        protected virtual void FormatCEHandler(TClient client, ClientFormatCE format)
        {
        }

        protected virtual void FormatCFHandler(TClient client, ClientFormatCF format)
        {
        }

        protected virtual void FormatD0Handler(TClient client, ClientFormatD0 format)
        {
        }

        protected virtual void FormatD1Handler(TClient client, ClientFormatD1 format)
        {
        }

        protected virtual void FormatD2Handler(TClient client, ClientFormatD2 format)
        {
        }

        protected virtual void FormatD3Handler(TClient client, ClientFormatD3 format)
        {
        }

        protected virtual void FormatD4Handler(TClient client, ClientFormatD4 format)
        {
        }

        protected virtual void FormatD5Handler(TClient client, ClientFormatD5 format)
        {
        }

        protected virtual void FormatD6Handler(TClient client, ClientFormatD6 format)
        {
        }

        protected virtual void FormatD7Handler(TClient client, ClientFormatD7 format)
        {
        }

        protected virtual void FormatD8Handler(TClient client, ClientFormatD8 format)
        {
        }

        protected virtual void FormatD9Handler(TClient client, ClientFormatD9 format)
        {
        }

        protected virtual void FormatDAHandler(TClient client, ClientFormatDA format)
        {
        }

        protected virtual void FormatDBHandler(TClient client, ClientFormatDB format)
        {
        }

        protected virtual void FormatDCHandler(TClient client, ClientFormatDC format)
        {
        }

        protected virtual void FormatDDHandler(TClient client, ClientFormatDD format)
        {
        }

        protected virtual void FormatDEHandler(TClient client, ClientFormatDE format)
        {
        }

        protected virtual void FormatDFHandler(TClient client, ClientFormatDF format)
        {
        }

        protected virtual void FormatE0Handler(TClient client, ClientFormatE0 format)
        {
        }

        protected virtual void FormatE1Handler(TClient client, ClientFormatE1 format)
        {
        }

        protected virtual void FormatE2Handler(TClient client, ClientFormatE2 format)
        {
        }

        protected virtual void FormatE3Handler(TClient client, ClientFormatE3 format)
        {
        }

        protected virtual void FormatE4Handler(TClient client, ClientFormatE4 format)
        {
        }

        protected virtual void FormatE5Handler(TClient client, ClientFormatE5 format)
        {
        }

        protected virtual void FormatE6Handler(TClient client, ClientFormatE6 format)
        {
        }

        protected virtual void FormatE7Handler(TClient client, ClientFormatE7 format)
        {
        }

        protected virtual void FormatE8Handler(TClient client, ClientFormatE8 format)
        {
        }

        protected virtual void FormatE9Handler(TClient client, ClientFormatE9 format)
        {
        }

        protected virtual void FormatEAHandler(TClient client, ClientFormatEA format)
        {
        }

        protected virtual void FormatEBHandler(TClient client, ClientFormatEB format)
        {
        }

        protected virtual void FormatECHandler(TClient client, ClientFormatEC format)
        {
        }

        protected virtual void FormatEDHandler(TClient client, ClientFormatED format)
        {
        }

        protected virtual void FormatEEHandler(TClient client, ClientFormatEE format)
        {
        }

        protected virtual void FormatEFHandler(TClient client, ClientFormatEF format)
        {
        }

        protected virtual void FormatF0Handler(TClient client, ClientFormatF0 format)
        {
        }

        protected virtual void FormatF1Handler(TClient client, ClientFormatF1 format)
        {
        }

        protected virtual void FormatF2Handler(TClient client, ClientFormatF2 format)
        {
        }

        protected virtual void FormatF3Handler(TClient client, ClientFormatF3 format)
        {
        }

        protected virtual void FormatF4Handler(TClient client, ClientFormatF4 format)
        {
        }

        protected virtual void FormatF5Handler(TClient client, ClientFormatF5 format)
        {
        }

        protected virtual void FormatF6Handler(TClient client, ClientFormatF6 format)
        {
        }

        protected virtual void FormatF7Handler(TClient client, ClientFormatF7 format)
        {
        }

        protected virtual void FormatF8Handler(TClient client, ClientFormatF8 format)
        {
        }

        protected virtual void FormatF9Handler(TClient client, ClientFormatF9 format)
        {
        }

        protected virtual void FormatFAHandler(TClient client, ClientFormatFA format)
        {
        }

        protected virtual void FormatFBHandler(TClient client, ClientFormatFB format)
        {
        }

        protected virtual void FormatFCHandler(TClient client, ClientFormatFC format)
        {
        }

        protected virtual void FormatFDHandler(TClient client, ClientFormatFD format)
        {
        }

        protected virtual void FormatFEHandler(TClient client, ClientFormatFE format)
        {
        }

        protected virtual void FormatFFHandler(TClient client, ClientFormatFF format)
        {
        }

        #endregion
    }
}
