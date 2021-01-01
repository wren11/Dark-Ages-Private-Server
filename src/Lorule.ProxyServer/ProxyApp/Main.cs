using Proxy.Networking;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace ProxyApp
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        public ObservableCollection<ProxyState> ProxyWindows = new ObservableCollection<ProxyState>();

        private readonly ProxyBase _proxyServer = new ProxyBase("127.0.0.1", 2610);

        public ProxyState this[uint serial]
        {
            get { return ProxyWindows.FirstOrDefault(i => i.Client.Serial == serial); }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            _proxyServer.OnGameServerConnect += OnGameEnter;

            _proxyServer.OnServerPacket += _proxyServer_OnServerPacket;
            _proxyServer.OnClientPacket += _proxyServer_OnClientPacket;
        }

        private void _proxyServer_OnServerPacket(ProxyClient client, byte[] data)
        {
            if (this[client.Serial] != null)
            {
                this[client.Serial].ProxyServerPacket(data);
            }
        }

        private void _proxyServer_OnClientPacket(ProxyClient client, byte[] data)
        {
            if (this[client.Serial] != null)
            {
                this[client.Serial].ProxyClientPacket(data);
            }
        }

        private void OnGameEnter(uint serial)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker) delegate()
                {
                    var state = new ProxyState()
                    {
                        ProxyWindow = new FrmProxyWindow() {MdiParent = this},
                        Serial = serial,
                        Server =  _proxyServer,
                        Client = _proxyServer.Clients[serial]
                    };
                    state.ProxyWindow.SetNetworkState(state);
                    state.ProxyWindow.Show();
                    ProxyWindows.Add(state);
                });

            }
        }
    }

    public class ProxyState
    { 
        public uint Serial { get; set; }

        public FrmProxyWindow ProxyWindow { get; set; }

        public ProxyClient Client { get; set; }

        public ProxyBase Server { get; set; }

        public void ProxyServerPacket(byte[] data)
        {
            ProxyWindow.AddPacket(new Packet(data), PacketFlow.SendingToClient);
        }
        public void ProxyClientPacket(byte[] data)
        {
            ProxyWindow.AddPacket(new Packet(data), PacketFlow.SendingToServer);
        }
    }
}
