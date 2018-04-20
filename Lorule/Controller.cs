using Darkages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lorule
{
    public partial class Controller : Form
    {
        public Controller()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (ServerContext.Game)
            {
                ServerContext.Paused = true;
                try
                {
                    lock (ServerContext.Game.Clients)
                    {
                        ServerContext.LoadAndCacheStorage();
                    }
                }
                catch (Exception)
                {
                    //error
                }
                finally
                {
                    ServerContext.Paused = false;
                }
            }
        }
    }
}
