using Proxy.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProxyApp
{
    public enum PacketFlow
    {
        SendingToClient,
        SendingToServer
    }

    public partial class FrmProxyWindow : Form
    {
        public FrmProxyWindow()
        {
            InitializeComponent();
        }

        private ProxyState _state;

        private void FrmProxyWindow_Load(object sender, EventArgs e)
        {

        }

        public ConcurrentStack<(PacketFlow,Packet)> Packets = new ConcurrentStack<(PacketFlow, Packet)>();

        public void AddPacket(Packet packet, PacketFlow flow) => Packets.Push((flow, packet));

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
                return;

            foreach (var line in richTextBox1.Lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {

                    var packet = new Packet(line);

                    if (_state.Client == null) return;

                    if (packet.Action == 0x39 || packet.Action == 0x3A)
                    {
                        _state.Client.Crypto.DialogTransform(packet);
                    }

                    _state.Client.Crypto.Transform(packet);
                    _state.Client.ServerPBuff2.Add(packet);
                    _state.Client.SendServerPBuff2();

                    Thread.Sleep(50);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
                return;

            var packet = new Packet(richTextBox1.Text);

            _state.Client?.ClientPBuff2.Add(packet);
            _state.Client?.SendClientPBuff2();
        }

        int _packets;

        public void timer1_Tick(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                if (checkBox1.Checked)
                    return;

                foreach (var (flow, packet) in Packets)
                {
                    if (flow == PacketFlow.SendingToClient)
                    {
                        richTextBox3.AppendText(packet + Environment.NewLine);
                    }
                    else
                    {
                        richTextBox2.AppendText(packet + Environment.NewLine);
                    }

                    ++_packets;

                    if (_packets <= 1000)
                        continue;

                    richTextBox3.Clear();
                    richTextBox2.Clear();
                    _packets = 0;
                }

                richTextBox3.ResumeLayout();
                richTextBox2.ResumeLayout();

                Packets.Clear();
            });

        }

        public void SetNetworkState(ProxyState state)
        {
            _state = state;
        }
        public static Collection<int> GeneratedNumbers = new Collection<int>();
        public static Random Random = new Random();

        public static int GenerateNumber()
        {
            int id;

            do
            {
                lock (Random)
                {
                    id = Random.Next();
                }
            } while (GeneratedNumbers.Contains(id));

            return id;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {

                var number = 0;

                while (number++ <= 5000)
                {
                    var packet = new Packet("47 FC 04 00");
                    _state.Client?.ClientPBuff2.Add(packet);
                    _state.Client?.SendClientPBuff2();

                    Thread.Sleep(1);
                }
            });
        }
    }
}
