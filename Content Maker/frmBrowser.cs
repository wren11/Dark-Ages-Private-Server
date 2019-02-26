using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class FrmBrowser : Form
    {
        public FrmBrowser()
        {
            InitializeComponent();

            GoTo("http://www.vorlof.com");
        }


        public void GoTo(string url)
        {
            webBrowser1.Navigate(url);
        }

        private void FrmBrowser_Load(object sender, EventArgs e)
        {

        }
    }
}
