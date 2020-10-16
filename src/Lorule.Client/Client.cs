#region

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DAClient.ClientFormats;
using Darkages.Network;
using Darkages.Security;

#endregion

namespace DAClient
{
    public class Client
    {
        public enum ServerState
        {
            Lobby,
            Login,
            World,
            Creating,
            Created
        }

        public static int Connections;

        private readonly string Pass;

        private readonly string User;

        private SecurityProvider _encryption = new SecurityProvider();

        private NetworkPacketReader _reader = new NetworkPacketReader();

        private readonly byte[] _recvBuffer = new byte[0x10000];

        private Socket _socket;

        private readonly NetworkPacketWriter _writer = new NetworkPacketWriter();

        public bool RequiresVersion = true;

        public ServerState State = ServerState.Lobby;

        public Client(string lpUser, string lpPassword)
        {
            User = lpUser;
            Pass = lpPassword;
        }

        public byte Ordinal { get; set; }

        public bool Connect(byte[] lpAddress, int port)
        {
            var ip = new IPAddress(lpAddress);

            return Connect(ip.ToString(), port);
        }

        public bool Connect(string lpAddress, int port)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(IPAddress.Parse(lpAddress), port);

                if (_socket.Connected)
                {
                    _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length,
                        SocketFlags.None, EndReceive, _socket);

                    return true;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to server: {0}:{1}", lpAddress, port);
            }

            return false;
        }

        public void Send(NetworkFormat format)
        {
            if (!_socket.Connected)
                return;

            _writer.Position = 0;
            _writer.Write(format.Command);

            if (format.Secured) _writer.Write(Ordinal++);

            format.Serialize(_writer);

            var packet = _writer.ToPacket();
            {
                if (format.Secured)
                    _encryption.Transform(packet);

                var buffer = packet.ToArray();
                _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;

            if (!socket.Connected)
                return;

            var bytes = socket.EndReceive(ar, out var error);

            if (bytes == 0 || error != SocketError.Success)
            {
                socket.Close();
                return;
            }

            if (bytes - 3 <= 0)
            {
                socket.Close();
                return;
            }

            var packetRef = new byte[bytes - 3];
            {
                Array.Copy(_recvBuffer, 3, packetRef, 0, packetRef.Length);
                var packet = new NetworkPacket(packetRef, packetRef.Length);
                {
                    ReceivePacketData(packet);
                }
            }

            if (!socket.Connected)
                return;

            socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length,
                SocketFlags.None, EndReceive, socket);
        }

        private void ReceivePacketData(NetworkPacket packet)
        {
            if (packet.Command == 0x7E)
            {
                if (RequiresVersion)
                {
                    var format = new ClientVersion();
                    Send(format);

                    RequiresVersion = false;
                }

                Send(new CreateAccount(User, Pass));
                State = ServerState.Creating;

                Thread.Sleep(5000);
            }
            else if (packet.Command == 0x02)
            {
                if (State == ServerState.Created)
                {
                    State = ServerState.World;
                    Thread.Sleep(5000);
                    Send(new Login(User, Pass));
                    return;
                }

                if (State == ServerState.Creating)
                {
                    Send(new CreateAisling());
                    State = ServerState.Created;
                }
            }
            else if (packet.Command == 0x00)
            {
                _reader = new NetworkPacketReader();
                _reader.Packet = packet;
                {
                    _reader.Position--;
                }

                var type = _reader.ReadByte();

                if (type == 0)
                {
                    var serverTableCrc = _reader.ReadUInt32();
                    var seed = _reader.ReadByte();
                    var salt = _reader.ReadBytes(_reader.ReadByte());

                    _encryption = new SecurityProvider(
                        new SecurityParameters(seed, salt));

                    Send(new EncryptionReceived(0));
                }
            }
            else if (packet.Command == 0x03)
            {
                _reader = new NetworkPacketReader();
                _reader.Packet = packet;
                {
                    _reader.Position--;
                }

                var address = _reader.ReadBytes(4);
                var port = _reader.ReadUInt16();

                _reader.Position++;

                var seed = _reader.ReadByte();
                var key = _reader.ReadStringA();

                var name = _reader.ReadStringA();
                var socketid = _reader.ReadUInt32();

                _encryption = new SecurityProvider(new SecurityParameters(seed, Encoding.ASCII.GetBytes(key)));
                {
                    Array.Reverse(address);
                }

                _socket.Close();
                Connect(address, port);

                State = ServerState.Login;

                Send(new RedirectRequest(seed, key, name, socketid));
            }
            else if (packet.Command == 0x05)
            {
                State = ServerState.World;
                Connections++;
            }
        }
    }
}