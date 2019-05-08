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
using Darkages.Storage;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class AreaCreateWizard : Form
    {
        private string SelectedMap { get; set; }

        public AreaCreateWizard()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            new WarpManager(textBox3.Text).ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Map Files|*.map";
                ofd.Multiselect = false;
                ofd.InitialDirectory = ServerContext.StoragePath + "\\maps";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var match = Regex.Match(ofd.FileName, @"lod\d*[^.map]");
                    var mapid = match.Success ? match.Value.Replace("lod", string.Empty) : "error";

                    if (mapid != "error")
                    {
                        textBox3.Text = mapid;
                    }

                    SelectedMap = ofd.FileName;
                }
            }
        }


        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button4.Enabled = true;
        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button4.Enabled = false;
        }

        private void AreaCreateWizard_Load(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button4.Enabled = false;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (ServerContext.GlobalMapCache.Count(i => i.Value.Name.Equals(textBox4.Text, StringComparison.OrdinalIgnoreCase)
                || i.Value.Number == Convert.ToInt32(textBox3.Text)) > 0)
            {
                MessageBox.Show("This map appears to exist already.");
                return;
            }


            try
            {
                var map = new Area
                {
                    Name = textBox4.Text,
                    Rows = Convert.ToUInt16(textBox2.Text),
                    Cols = Convert.ToUInt16(textBox1.Text),
                    ID = Convert.ToInt32(textBox3.Text),
                    Music = Convert.ToInt32(textBox5.Text)
                };
                map.Number = map.ID;
                map.Ready = false;

                map.Flags = radioButton2.Checked ? Darkages.Types.MapFlags.Default : Darkages.Types.MapFlags.PlayerKill;
                {
                    var path = Path.GetFullPath(ServerContext.StoragePath + string.Format(@"\maps\lod{0}.map", map.Number));

                    if (!File.Exists(path))
                    {
                        File.Move(SelectedMap, path);
                    }

                    StorageManager.AreaBucket.Save(map);
                    ServerContext.LoadAndCacheStorage();
                }
            }
            catch (Exception error)
            {
                ServerContext.Info?.Error("Error creating area", error);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            new WorldManager().ShowDialog();
        }
    }
}
