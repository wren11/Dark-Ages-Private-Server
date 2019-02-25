///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
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

        private void button4_Click(object sender, System.EventArgs e)
        {
            new NewItemWizard().ShowDialog();
        }

        private void button5_Click(object sender, System.EventArgs e)
        {
            new frmReactorWizard().ShowDialog();
        }

        private void frmMain_Load(object sender, System.EventArgs e)
        {
            button4_Click(sender, e);
        }
    }
}
