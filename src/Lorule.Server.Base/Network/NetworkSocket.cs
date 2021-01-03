#region

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using Darkages.IO;
using Darkages.Network.Object;
using ServiceStack.Validation;

#endregion

namespace Darkages.Network
{
    public class NetworkSocket : ObjectManager
    {
        internal Socket Socket;
        private const int HeaderLength = 3;

        private readonly byte[] _header = new byte[HeaderLength];
        private readonly byte[] _packet = new byte[65534];

        private int _headerOffset;
        private int _packetLength;
        private int _packetOffset;

        private readonly NetworkStream _networkStreamReader;

        public NetworkSocket(Socket socket)
        {
            ConfigureTcpSocket(socket);
            Socket = socket;

            _networkStreamReader = new NetworkStream(socket);
        }

        public bool HeaderComplete => _headerOffset == HeaderLength;

        public bool PacketComplete => _packetOffset == _packetLength;

        public virtual IAsyncResult BeginReceiveHeader(AsyncCallback callback, object state)
        {
            return _networkStreamReader.BeginRead(_header, _headerOffset, HeaderLength - _headerOffset, callback, state);
        }

        public virtual IAsyncResult BeginReceivePacket(AsyncCallback callback, object state)
        {
            return _networkStreamReader.BeginRead(_packet, _packetOffset, _packetLength - _packetOffset, callback, state);
        }

        public virtual int EndReceiveHeader(IAsyncResult result)
        {
            var bytes = _networkStreamReader.EndRead(result);

            if (bytes == 0)
                return 0;

            _headerOffset += bytes;

            if (!HeaderComplete)
                return bytes;

            _packetLength = (_header[1] << 8) | _header[2];
            _packetOffset = 0;

            return bytes;
        }

        public virtual int EndReceivePacket(IAsyncResult result)
        {
            var bytes = _networkStreamReader.EndRead(result);

            if (bytes == 0)
                return 0;

            _packetOffset += bytes;

            if (PacketComplete) _headerOffset = 0;

            return bytes;
        }


        public NetworkPacket ToPacket()
        {
            return PacketComplete ? new NetworkPacket(_packet, _packetLength) : null;
        }

        public void Send(byte[] data)
        {
            try
            {
                if (!Socket.Connected)
                    return;

                if (_networkStreamReader.CanWrite && Socket.Connected)
                {
                    _networkStreamReader.Write(data, 0, data.Length);
                    _networkStreamReader.Flush();
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private static void ConfigureTcpSocket(Socket tcpSocket)
        {
            tcpSocket.NoDelay = true;
            tcpSocket.ReceiveBufferSize = 65534;
            tcpSocket.SendBufferSize = 65534;
        }
    }
}