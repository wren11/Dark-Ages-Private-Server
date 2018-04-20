using System;
using System.Windows.Forms;
using static ClientLauncher.frmMain;

namespace ClientLauncher
{
    public partial class ConfigOps : Form
    {
        Configuration config = null;
        public ConfigOps(Configuration config)
        {
            InitializeComponent();
            this.config = config;
        }

        private void ConfigOps_Load(object sender, EventArgs e)
        {
            this.propertyGrid1.SelectedObject = null;
            this.propertyGrid1.SelectedObject = config;
        }
    }
}
