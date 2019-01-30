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

                        var objects = ServerContext.Game.GetObjects(i => true, Darkages.Network.Object.ObjectManager.Get.All);

                        foreach (var obj in objects)
                        {
                            obj.Remove();
                        }

                        ServerContext.LoadAndCacheStorage();
                    }
                }
                catch
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
