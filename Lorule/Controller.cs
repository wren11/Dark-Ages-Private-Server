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
