using Darkages;
using System.IO;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class frmMain : Form
    {
        public static ServerContext context = new ServerContext();

        public frmMain()
        {
            InitDataContext();
            InitializeComponent();
        }

        private static void InitDataContext()
        {
            ServerContext.LoadConstants();
            {
                ServerContext.StoragePath = @"..\..\..\LORULE_DATA";

                if (!Directory.Exists(ServerContext.StoragePath))
                {
                    MessageBox.Show("Error, LORULE_DATA directory could not be found.");
                    Application.Exit();
                }
                else
                {
                    ServerContext.LoadAndCacheStorage();
                }
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            new AreaCreateWizard().ShowDialog();
        }
    }
}
