using DAClient.ClientFormats;
using Darkages.Network;
using Darkages.Security;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DAClient
{
    public class Client
    {
        public static int Connections = 0;

        public enum ServerState
        {
            Lobby,
            Login,
            World
        }

        NetworkPacketReader _reader = new NetworkPacketReader();

        NetworkPacketWriter _writer = new NetworkPacketWriter();

        SecurityProvider _encryption = new SecurityProvider();

        public ServerState State = ServerState.Lobby;

        public byte Ordinal { get; set; } = 0;

        private Socket _socket;

        private byte[] _recvBuffer = new byte[0x10000];

        private readonly string User;

        private readonly string Pass;


        public Client(string lpUser, string lpPassword)
        {
            User = lpUser;
            Pass = lpPassword;
        }


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

        private void EndReceive(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;

            if (!socket.Connected)
                return;

            var bytes = socket.EndReceive(ar, out var error);

            if (bytes == 0 || error != SocketError.Success)
            {
                socket.Close();
                return;
            }

            if ((bytes - 3) <= 0)
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

        private void Send(NetworkFormat format)
        {
            _writer.Position = 0;
            _writer.Write(format.Command);

            if (format.Secured)
            {
                _writer.Write(Ordinal++);
            }

            format.Serialize(_writer);

            var packet = _writer.ToPacket();
            {

                if (format.Secured)
                    _encryption.Transform(packet);

                var buffer = packet.ToArray();
                _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
        }

        public bool RequiresVersion = true;

        private void ReceivePacketData(NetworkPacket packet)
        {
            switch (packet.Command)
            {
                #region Connected, Send Client Version.
                case 0x7E:
                    {
                        if (RequiresVersion)
                        {
                            var format = new ClientVersion();
                            Send(format);

                            RequiresVersion = false;
                        }
                    }
                    break;
                #endregion
                #region Receive Encryption Information.
                case 0x00:
                    {
                        _reader = new NetworkPacketReader();
                        _reader.Packet = packet;
                        {
                            _reader.Position--;
                        }

                        byte type = _reader.ReadByte();

                        if (type == 0)
                        {
                            var serverTableCrc = _reader.ReadUInt32();
                            var seed = _reader.ReadByte();
                            var salt = _reader.ReadBytes(_reader.ReadByte());

                            _encryption = new SecurityProvider(
                                new SecurityParameters(seed, salt));

                            Send(new EncryptionReceived(0));

                        }

                    } break;
                #endregion
                #region Received Server Table Data.
                case 0x56:
                    {
                        _encryption.Transform(packet);
                        _reader = new NetworkPacketReader();
                        _reader.Packet = packet;
                    }
                    break;
                #endregion
                #region Received Redirect Information.
                case 0x03:
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
                    } break;
                #endregion
                #region Request Login
                case 0x60:
                    {
                        if (State == ServerState.Login)
                        {
                            Send(new Login(User, Pass));
                            State = ServerState.World;
                        }
                    } break;
                #endregion
                #region Login Response
                case 0x02:
                    {
                        _encryption.Transform(packet);
                        _reader = new NetworkPacketReader();
                        _reader.Packet = packet;

                    } break;
                #endregion
                case 0x05:
                    Connections++;
                    break;
                default: break;
            }
        }
    }
}
