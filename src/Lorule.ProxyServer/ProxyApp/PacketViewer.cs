using Be.Windows.Forms;
using System.Windows.Forms;

namespace ProxyApp
{
    public partial class PacketViewer : UserControl
    { 
        public PacketViewer()
        {
            InitializeComponent();
        }


        public void LogPacket(string packetStr)
        {
            listBox1.Items.Add(packetStr);
        }
    }
}
