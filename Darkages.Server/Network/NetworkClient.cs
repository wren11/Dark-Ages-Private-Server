using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Security;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Darkages.Network
{
    public abstract class NetworkClient<TClient> : ObjectManager, INotifyPropertyChanged, INetworkClient<TClient>
        where TClient : NetworkClient<TClient>
    {
        private readonly object _syncLock = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        protected NetworkClient()
        {
            Reader = new NetworkPacketReader();
            Writer = new NetworkPacketWriter();

            Encryption = new SecurityProvider();
        }

        public NetworkPacketReader Reader { get; set; }

        public NetworkPacketWriter Writer { get; set; }

        public NetworkSocket ServerSocket { get; set; }

        public SecurityProvider Encryption { get; set; }

        public byte Ordinal { get; set; }

        public int Serial { get; set; }

        public DateTime DateMapOpened { get; set; }

        public bool MapOpen { get; set; } = false;

        public int LastSelectedNodeIndex { get; set; } = 0;

        private int _selectedNodeIndex;

        public int SelectedNodeIndex
        {
            get => _selectedNodeIndex;
            set
            {
                if (value != _selectedNodeIndex)
                {
                    _selectedNodeIndex = value;
                    NotifyPropertyChanged("SelectedNodeIndex");
                }
            }
        }

        private bool _inMapTransition;

        public bool InMapTransition
        {
            get => _inMapTransition;
            set
            {
                if (value != _inMapTransition)
                {
                    _inMapTransition = value;
                    NotifyPropertyChanged("InMapTransition");
                }
            }
        }

        public Session<TClient> Session { get; internal set; }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Read(NetworkPacket packet, NetworkFormat format)
        {
            if (packet == null)
                return;

            if (format.Secured)
            {
                Encryption.Transform(packet);

                if (format.Command == 0x39 || format.Command == 0x3A)
                {
                    TransFormDialog(packet);
                    Reader.Position = 6;
                }
                else
                {
                    Reader.Position = 0;
                }
            }
            else
            {
                Reader.Position = -1;
            }

            ResetReader(packet, format);
        }

        private void ResetReader(NetworkPacket packet, NetworkFormat format)
        {
            Reader.Packet = packet;
            format.Serialize(Reader);
            Reader.Position = -1;
        }

        public void FlushAndSend(NetworkFormat format)
        {
            lock (_syncLock)
            {
                Writer.Position = 0;
                Writer.Write(format.Command);

                if (format.Secured)
                    Writer.Write(Ordinal++);

                format.Serialize(Writer);

                var packet = Writer.ToPacket();
                if (packet == null)
                    return;

                if (format.Secured)
                    Encryption.Transform(packet);

                var array = packet.ToArray();
                Session.SendAsync(array);
            }
        }

        public NetworkClient<TClient> Send(NetworkFormat format)
        {
            FlushAndSend(format);
            return this;
        }

        public void Send(NetworkPacketWriter lpData)
        {
            var packet = lpData.ToPacket();
            Encryption.Transform(packet);

            lock (_syncLock)
            {
                var array = packet.ToArray();
                Session.SendAsync(array);
            }
        }

        public void Send(byte[] data)
        {
            lock (_syncLock)
            {
                Writer.Position = 0;
                Writer.Write(data);

                var packet = Writer.ToPacket();
                if (packet == null)
                    return;

                Encryption.Transform(packet);

                var array = packet.ToArray();
                Session.SendAsync(array);
            }
        }

        private protected static byte P(NetworkPacket value) => (byte) (value.Data[1] ^ (byte) (value.Data[0] - 0x2D));

        private void TransFormDialog(NetworkPacket value)
        {
            lock (_syncLock)
            {
                value.Data[2] ^= (byte) (P(value) + 0x73);
                value.Data[3] ^= (byte) (P(value) + 0x73);
                value.Data[4] ^= (byte) (P(value) + 0x28);
                value.Data[5] ^= (byte) (P(value) + 0x29);

                for (var i = 0; i < value.Data.Length - 6; i++)
                    value.Data[6 + i] ^= (byte) (((byte) (P(value) + 0x28) + i + 2) % 256);
            }
        }

        public void SendMessageBox(byte code, string text)
        {
            Send(new ServerFormat02(code, text));
        }
    }
}