using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Proxy.Networking
{
    public class ProxyClient
    {
        private readonly object _syncObject = new object();

        public bool IsLoaded = false;

        public List<Packet> ClientPBuff1 = new List<Packet>();
        public List<Packet> ClientPBuff2 = new List<Packet>();

        public List<Packet> ServerPBuff1 = new List<Packet>();
        public List<Packet> ServerPBuff2 = new List<Packet>();

        public ProxyBase Proxy { get; set; }

        public CryptoProvider Crypto { get; set; }
        public uint Serial { get; set; }

        private readonly uint _serial;
        private readonly Socket _clientSocket;
        private readonly byte[] _clientRecvBuffer;
        private readonly SerialDelegate _onClientDisconnect;
        private readonly SerialPacketDelegate _clientOnReceive;
        private readonly SerialPacketDelegate _serverOnReceive;

        private byte[] _clientReceivedBuffer;
        private byte[] _clientSendBuffer;
        private byte[] _serverReceivedBuffer;
        private byte[] _serverSendBuffer;
        private byte[] _serverRecvBuffer;

        private byte _clientOrdinal;
        private bool _disconnected;
        private Socket _serverSocket;

        public ProxyClient(
            uint serial,
            Socket socket,
            IPEndPoint serverEndPoint,
            SerialPacketDelegate clientReceiveDelegate,
            SerialDelegate clientDisconnectDelegate,
            SerialPacketDelegate serverReceiveDelegate)
        {
            Crypto = new CryptoProvider();
            _serial = serial;

            _disconnected = false;

            _clientReceivedBuffer = new byte[0x0];
            _clientSendBuffer = new byte[0x0];

            _clientRecvBuffer = new byte[ushort.MaxValue];
            _clientSocket = socket;
            _clientOnReceive = clientReceiveDelegate;
            _onClientDisconnect = clientDisconnectDelegate;
            _serverOnReceive = serverReceiveDelegate;

            InitializeServerSocket(serverEndPoint);
            _clientSocket.BeginReceive(_clientRecvBuffer, 0x0, _clientRecvBuffer.Length, SocketFlags.None, EndReceive,
                null);
        }


        private void EndReceive(IAsyncResult result)
        {
            try
            {
                var bytesReceived = _clientSocket.EndReceive(result);
                if (bytesReceived == 0x0)
                {
                    Disconnect();
                }
                else
                {
                    Array.Resize(ref _clientReceivedBuffer, _clientReceivedBuffer.Length + bytesReceived);
                    Array.Copy(_clientRecvBuffer, 0x0, _clientReceivedBuffer,
                        _clientReceivedBuffer.Length - bytesReceived,
                        bytesReceived);

                    while (_clientReceivedBuffer.Length >= 0x3)
                    {
                        var packetLength = (_clientReceivedBuffer[0x1] << 0x8) + _clientReceivedBuffer[0x2];

                        if (packetLength + 0x3 <= _clientReceivedBuffer.Length)
                        {
                            var rawData = new byte[packetLength];
                            Array.Copy(_clientReceivedBuffer, 0x3, rawData, 0x0, packetLength);

                            var numArray = new byte[_clientReceivedBuffer.Length - (packetLength + 0x3)];
                            Array.Copy(_clientReceivedBuffer, packetLength + 0x3, numArray, 0x0, numArray.Length);

                            _clientReceivedBuffer = numArray;
                            _clientOnReceive(_serial, new Packet(rawData));
                        }
                        else
                            break;
                    }

                    SendServerPBuff1();
                    SendServerPBuff2();

                    _clientSocket.BeginReceive(_clientRecvBuffer,
                        0x0,
                        _clientRecvBuffer.Length,
                        SocketFlags.None,
                        EndReceive,
                        null);
                }
            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (InvalidOperationException)
            {
                Disconnect();
            }
        }

        private void SendClientPBuff1()
        {
            lock (_syncObject)
            {
                var buffer = new byte[0x0];

                foreach (var packet in ClientPBuff1)
                {
                    var rawBytes = packet.ToArray();

                    Array.Resize(ref buffer, buffer.Length + rawBytes.Length);
                    Array.Copy(rawBytes, 0x0, buffer, buffer.Length - rawBytes.Length, rawBytes.Length);
                }

                for (var offset = 0x0; offset < buffer.Length; offset += 0x400)
                {
                    if (offset + 0x400 <= buffer.Length)
                        _clientSocket.Send(buffer, offset, 0x400, SocketFlags.None);
                    else
                        _clientSocket.Send(buffer, offset, buffer.Length - offset, SocketFlags.None);
                }

                ClientPBuff1.Clear();
            }
        }

        public void SendClientPBuff2()
        {
            lock (_syncObject)
            {
                var buffer = new byte[0x0];

                foreach (var packet in ClientPBuff2)
                {
                    var rawBytes = packet.ToArray();

                    Array.Resize(ref buffer, buffer.Length + rawBytes.Length);
                    Array.Copy(rawBytes, 0x0, buffer, buffer.Length - rawBytes.Length, rawBytes.Length);
                }

                for (var offset = 0x0; offset < buffer.Length; offset += 0x400)
                {
                    if (offset + 0x400 <= buffer.Length)
                        _clientSocket.Send(buffer, offset, 0x400, SocketFlags.None);
                    else
                        _clientSocket.Send(buffer, offset, buffer.Length - offset, SocketFlags.None);
                }

                ClientPBuff2.Clear();
            }
        }

        private void Disconnect()
        {
            if (_disconnected)
                return;

            lock (_syncObject)
            {
                _clientSocket.Disconnect(true);

                if (_serverSocket.Connected)
                    _serverSocket.Disconnect(false);

                if (_clientSocket.Connected)
                    _clientSocket.Disconnect(false);

                _onClientDisconnect(_serial);
                _disconnected = true;
            }
        }

        public void InitializeServerSocket(IPEndPoint endPoint)
        {
            _serverRecvBuffer = new byte[ushort.MaxValue];

            _serverReceivedBuffer = new byte[0x0];
            _serverSendBuffer = new byte[0x0];

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Connect(endPoint);
            _serverSocket.BeginReceive(_serverRecvBuffer, 0x0, _serverRecvBuffer.Length, SocketFlags.None,
                ServerEndReceive, null);
        }

        private void ServerEndReceive(IAsyncResult result)
        {
            try
            {
                int length1 = _serverSocket.EndReceive(result);
                if (length1 == 0x0)
                {
                    Disconnect();
                }
                else
                {
                    Array.Resize(ref _serverReceivedBuffer, _serverReceivedBuffer.Length + length1);
                    Array.Copy(_serverRecvBuffer, 0x0, _serverReceivedBuffer,
                        _serverReceivedBuffer.Length - length1, length1);
                    while (_serverReceivedBuffer.Length >= 0x3)
                    {
                        int length2 = (_serverReceivedBuffer[0x1] << 0x8) + _serverReceivedBuffer[0x2];
                        if (length2 + 0x3 <= _serverReceivedBuffer.Length)
                        {
                            byte[] rawData = new byte[length2];
                            Array.Copy(_serverReceivedBuffer, 0x3, rawData, 0x0, length2);
                            byte[] numArray = new byte[_serverReceivedBuffer.Length - (length2 + 0x3)];
                            Array.Copy(_serverReceivedBuffer, length2 + 0x3, numArray, 0x0,
                                numArray.Length);
                            _serverReceivedBuffer = numArray;
                            _serverOnReceive(_serial, new Packet(rawData));
                        }
                        else
                            break;
                    }

                    SendClientPBuff1();
                    SendClientPBuff2();
                    _serverSocket.BeginReceive(_serverRecvBuffer, 0x0, _serverRecvBuffer.Length, SocketFlags.None,
                        new AsyncCallback(ServerEndReceive), null);
                }
            }
            catch (SocketException ex)
            {
                Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                Disconnect();
            }
        }

        private void SendServerPBuff1()
        {
            if (ServerPBuff1 != null && ServerPBuff1.Count == 0x0)
                return;

            if (ServerPBuff1 == null)
                return;

            lock (ServerPBuff1)
            {
                byte[] array1 = new byte[0x0];
                for (var index = 0x0; index < ServerPBuff1.Count; ++index)
                {
                    if (ServerPBuff1[index].Action != 0x62 && ServerPBuff1[index].Action != 0x0 &&
                        (ServerPBuff1[index].Action != 0x10 && ServerPBuff1[index].Action != 0x7B) &&
                        (ServerPBuff1[index].Action != 0x68 && ServerPBuff1[index].Action != 0x2D &&
                         ServerPBuff1[index].Action != 0x4F) && ServerPBuff1[index].Action != 0x57)
                    {
                        Packet data = ServerPBuff1[index];
                        Crypto.Transform(data);
                        data.Ordinal = _clientOrdinal++;
                        Crypto.Transform(data);
                        ServerPBuff1[index] = data;
                    }
                    else
                        _clientOrdinal = (byte) (ServerPBuff1[index].Ordinal + 0x1U);

                    var array2 = ServerPBuff1[index].ToArray();
                    Array.Resize(ref array1, array1.Length + array2.Length);
                    Array.Copy(array2, 0x0, array1, array1.Length - array2.Length, array2.Length);
                }

                for (var offset = 0x0; offset < array1.Length; offset += 0x400)
                {
                    if (offset + 0x400 <= array1.Length)
                        _serverSocket.Send(array1, offset, 0x400, SocketFlags.None);
                    else
                        _serverSocket.Send(array1, offset, array1.Length - offset, SocketFlags.None);
                }

                ServerPBuff1.Clear();
                GC.Collect();
            }
        }

        public void SendServerPBuff2()
        {
            try
            {
                if (ServerPBuff2.Count == 0x0)
                    return;
                lock (ServerPBuff2)
                {
                    byte[] array1 = new byte[0x0];
                    for (int index = 0x0; index < ServerPBuff2.Count; ++index)
                    {
                        if (ServerPBuff2[index].Action != 0x62 && ServerPBuff2[index].Action != 0x0 &&
                            (ServerPBuff2[index].Action != 0x10 && ServerPBuff2[index].Action != 0x7B) &&
                            (ServerPBuff2[index].Action != 0x68 && ServerPBuff2[index].Action != 0x2D &&
                             ServerPBuff2[index].Action != 0x4F) && ServerPBuff2[index].Action != 0x57)
                        {
                            Packet data = ServerPBuff2[index];
                            Crypto.Transform(data);
                            data.Ordinal = (byte)(_clientOrdinal++ % 255);
                            Crypto.Transform(data);
                            ServerPBuff2[index] = data;
                        }

                        byte[] array2 = ServerPBuff2[index].ToArray();
                        Array.Resize(ref array1, array1.Length + array2.Length);
                        Array.Copy(array2, 0x0, array1, array1.Length - array2.Length, array2.Length);
                    }

                    for (int offset = 0x0; offset < array1.Length; offset += 0x400)
                    {
                        if (offset + 0x400 <= array1.Length)
                            _serverSocket.Send(array1, offset, 0x400, SocketFlags.None);
                        else
                            _serverSocket.Send(array1, offset, array1.Length - offset, SocketFlags.None);
                    }

                    ServerPBuff2.Clear();
                    GC.Collect();
                }
            }
            catch
            {
                Disconnect();
            }
        }
    }
}
