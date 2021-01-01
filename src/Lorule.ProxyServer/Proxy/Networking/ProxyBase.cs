using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Proxy.Networking.ServerStructs;

namespace Proxy.Networking
{
    public delegate void PacketDisposerDelegate(ProxyClient client, byte[] data);

    public class ProxyBase
    {
        private readonly string _remoteEndpoint;

        public uint RedirectSerial;
        public IPEndPoint Redirect;
        public SerialDelegate OnGameServerConnect;
        public SerialDelegate OnDisconnect;

        public ProxyBase(string remoteEndpoint, int port)
        {
            _remoteEndpoint = remoteEndpoint;

            RandomGen = new Random();
            Clients = new Dictionary<uint, ProxyClient>();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind((EndPoint) new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(10);
            Socket.BeginAccept(EndAccept, null);
        }

        public event PacketDisposerDelegate OnClientPacket, OnServerPacket;

        public Random RandomGen { get; set; }

        public Socket Socket { get; set; }

        public Dictionary<uint, ProxyClient> Clients { get; set; }

        private void EndAccept(IAsyncResult result)
        {
            var endAccept = Socket.EndAccept(result);

            uint index;

            if (RedirectSerial == 0x0U)
            {
                index = (uint) RandomGen.Next();

                while (Clients.ContainsKey(index))
                {
                    index = (uint) RandomGen.Next();
                }

            }
            else
            {
                index = RedirectSerial;
                RedirectSerial = 0;
            }

            if (Redirect == null)
            {
                Clients.Add(index,
                    new ProxyClient(index,
                        endAccept,
                        new IPEndPoint(IPAddress.Parse(_remoteEndpoint), 2610),
                        OnClientReceive,
                        OnClientDisconnect,
                        OnServerReceive));

                Clients[index].Proxy = this;
            }
            else
            {
                if (Clients.ContainsKey(index))
                    Clients.Remove(index);

                Clients[index] = new ProxyClient(index,
                    endAccept,
                    Redirect,
                    OnClientReceive,
                    OnClientDisconnect,
                    OnServerReceive)
                {
                    Proxy = this
                };

                Redirect = null;
            }

            Socket.BeginAccept(EndAccept, null);
        }


        public void OnClientReceive(uint serial, Packet packet)
        { 
            Clients[serial].Crypto.Transform( packet);

            if (packet.Action == 0x39 || packet.Action == 0x3A)
            {
                Clients[serial].Crypto.DialogTransform( packet);

                OnClientPacket?.Invoke(Clients[serial], (byte[])packet.Data.Clone());

                Clients[serial].Crypto.DialogTransform(packet);
            }
            else
            {
                OnClientPacket?.Invoke(Clients[serial], (byte[]) packet.Data.Clone());
            }

            Clients[serial].Crypto.Transform(packet);
            Clients[serial].ServerPBuff1.Add(packet);
        }

        public void OnServerReceive(uint serial, Packet packet)
        {
            if (!Clients.ContainsKey(serial))
                return;

            switch ((ServerAction) packet.Action)
            {
                case ServerAction.Redirect:
                {
                    var redirect = packet.Read<Redirect>(0x0);
                    Redirect = redirect.EndPoint;
                    RedirectSerial = redirect.Id;

                    packet = new Packet();
                    redirect.EndPoint = new IPEndPoint(IPAddress.Loopback, 2610);
                    packet.Write(redirect);
                    break;
                }
                case ServerAction.Serial:
                {
                    Clients[serial].Crypto.Transform(packet);

                    var serialInfo = packet.Read<Serial>(0x0);
                    Clients[serial].Serial = serialInfo.Id;

                    if (!Clients[serial].IsLoaded)
                    {
                        OnGameServerConnect?.Invoke(serial);
                        Clients[serial].IsLoaded = true;
                    }

                    Clients[serial].Crypto.Transform(packet);
                    break;
                }
                default:
                    Clients[serial].Crypto.Transform(packet);
                    OnServerPacket?.Invoke(Clients[serial], (byte[])packet.Data.Clone());
                    Clients[serial].Crypto.Transform(packet);
                    break;
            }



            Clients[serial].ClientPBuff1.Add(packet);
        }

        public void OnClientDisconnect(uint serial)
        {
            if (Clients.ContainsKey(serial))
            {
                Clients[serial].IsLoaded = false;
            }

            OnDisconnect?.Invoke(serial);
            Clients.Remove(serial);
        }
    }
}
