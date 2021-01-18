#region

using Darkages.Network.ClientFormats;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Security;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace Darkages.Network
{
    public abstract partial class NetworkClient : ObjectManager
    {
        private readonly object _sendLock = new object();

        private bool _sending;

        protected NetworkClient()
        {
            Reader = new NetworkPacketReader();
            Writer = new NetworkPacketWriter();
            Encryption = new SecurityProvider();
        }

        public SecurityProvider Encryption { get; set; }
        public bool InMapTransition { get; set; }
        public byte Ordinal { get; set; }
        public NetworkPacketReader Reader { get; set; }
        public int Serial { get; set; }
        public NetworkPacketWriter Writer { get; set; }
        public Socket Socket => State.Socket;

        internal NetworkSocket State { get; set; }
        public DateTime LastMessageFromClient { get; set; }

        public void FlushAndSend(NetworkFormat format)
        {
            if (!Socket.Connected)
                return;

            lock (_sendLock)
            {
                Writer.Position = 0x0;
                Writer.Write(format.Command);

                if (format.Secured)
                    Writer.Write(Ordinal++);

                format.Serialize(Writer);

                var packet = Writer.ToPacket();
                if (packet == null)
                    return;

                if (format.Secured)
                    Encryption.Transform(packet);

                var buffer = packet.ToArray();

                if (buffer.Length <= 0x0) return;

                if (_sending)
                    return;

                _sending = true;

                try
                {
                    IAsyncResult ar = Socket.BeginSend(
                        buffer,
                        0x0,
                        buffer.Length,
                        SocketFlags.None,
                        SendCompleted,
                        Socket
                    );

                    ar.AsyncWaitHandle.WaitOne();
                }
                catch (SocketException)
                {
                    //ignore
                }
            }
        }

        public void Read(NetworkPacket packet, NetworkFormat format)
        {
            if (packet == null)
                return;

            if (InMapTransition && !(format is ClientFormat3F))
                return;

            if (format is ClientFormat3F clientFormat3F && InMapTransition)
                if (this is GameClient client)
                {
                    client.LastNodeClicked = DateTime.UtcNow;

                    InMapTransition = false;

                    Thread.Sleep(0xC8);
                    GameServer.TraverseWorldMap(client, clientFormat3F);
                }

            if (format.Secured)
            {
                Encryption.Transform(packet);

                if (format.Command == 0x39 || format.Command == 0x3A)
                {
                    TransFormDialog(packet);
                    Reader.Position = 0x6;
                }
                else
                {
                    Reader.Position = 0x0;
                }
            }
            else
            {
                Reader.Position = -0x1;
            }

            Reader.Packet = packet;
            format.Serialize(Reader);
            Reader.Position = -0x1;
        }

        public void Send(NetworkFormat format)
        {
            FlushAndSend(format);
        }

        public void Send(NetworkPacketWriter data)
        {
            if (!Socket.Connected)
                return;

            if (InMapTransition) return;

            lock (ServerContext.SyncLock)
            {
                var packet = data.ToPacket();
                Encryption.Transform(packet);

                var array = packet.ToArray();

                if (Socket.Connected)
                    Socket.Send(array, SocketFlags.None);
            }
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        public void Send(string rawData)
        {
            Send(ConvertHexStringToByteArray(rawData));
        }

        public void Send(byte[] data)
        {
            if (!Socket.Connected)
                return;

            lock (ServerContext.SyncLock)
            {
                Writer.Position = 0x0;
                Writer.Write(data);

                var packet = Writer.ToPacket();
                if (packet == null)
                    return;

                Encryption.Transform(packet);

                var array = packet.ToArray();

                try
                {
                    if (Socket.Connected)
                        Socket.Send(array, SocketFlags.None);
                }
                catch (SocketException)
                {
                    // Ignore
                }
            }
        }

        public void SendMessageBox(byte code, string text)
        {
            Send(new ServerFormat02(code, text));
        }

        private static byte P(NetworkPacket value)
        {
            return (byte)(value.Data[0x1] ^ (byte)(value.Data[0x0] - 0x2D));
        }

        private static void TransFormDialog(NetworkPacket value)
        {
            if (value.Data.Length > 0x2) value.Data[0x2] ^= (byte)(P(value) + 0x73);
            if (value.Data.Length > 0x3) value.Data[0x3] ^= (byte)(P(value) + 0x73);
            if (value.Data.Length > 0x4) value.Data[0x4] ^= (byte)(P(value) + 0x28);
            if (value.Data.Length > 0x5) value.Data[0x5] ^= (byte)(P(value) + 0x29);

            for (var i = value.Data.Length - 0x6 - 0x1; i >= 0x0; i--)
            {
                var index = i + 0x6;

                if (index >= 0x0 && value.Data.Length > index)
                    value.Data[index] ^= (byte)(((byte)(P(value) + 0x28) + i + 0x2) % 0x100);
            }
        }

        private void SendCompleted(IAsyncResult ar)
        {
            var signal = ar.AsyncState as ManualResetEvent;

            if (ar.IsCompleted && ar.CompletedSynchronously)
            {
                signal?.Set();
            }

            lock (_sendLock)
            {
                _sending = false;
            }
        }
    }
}