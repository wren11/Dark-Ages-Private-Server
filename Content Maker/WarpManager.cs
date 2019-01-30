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
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class WarpManager : Form
    {
        List<Position> Activations = new List<Position>();
        List<Position> PreviousActivations = new List<Position>();

        Area SelectedArea = null;


        public string NewMapID { get; set; }

        public WarpManager(string mapid)
        {
            NewMapID = mapid;
            InitializeComponent();
        }

        private void WarpManager_Load(object sender, EventArgs e)
        {
            var connected_maps = ServerContext.GlobalMapCache.Select(i => i.Value.ID).Intersect(
                ServerContext.GlobalWarpTemplateCache.Select(i => i.ActivationMapId));

            var validContent = new List<string>();

            foreach (var id in connected_maps)
            {
                if (ServerContext.GlobalMapCache.ContainsKey(id))
                {
                    var obj = id + " |  " + ServerContext.GlobalMapCache[id].Name;
                    validContent.Add(obj);
                }
            }

            comboBox1.DataSource = validContent.ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var id = comboBox1.Text.Split('|').FirstOrDefault().Trim();

            if (id == null)
                return;

            var area = ServerContext.GlobalMapCache[Convert.ToInt32(id)];

            if (area != null)
            {
                richTextBox1.Text = "Selected Map : " + area.Name;
                richTextBox1.AppendText("\nMap Dimensions : " + area.Cols + "," + area.Rows);

                SelectedArea = area;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var x = textBox1.Text;
            var y = textBox2.Text;

            var X = -1;
            var Y = -1;

            int.TryParse(x, out X);
            int.TryParse(y, out Y);

            if (X < 0 || Y < 0)
                return;

            Activations.Add(new Position(X, Y));
            listView1.Items.Add(new ListViewItem(new[] { x, y }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var idx = listView1.SelectedIndices;

            foreach (int id in idx)
            {
                if (id >= 0)
                {
                    Activations.RemoveAt(id);
                    listView1.Items.RemoveAt(id);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Activations.Clear();
            listView1.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var x = textBox3.Text;
            var y = textBox4.Text;

            var X = -1;
            var Y = -1;

            int.TryParse(x, out X);
            int.TryParse(y, out Y);

            if (X < 0 || Y < 0)
                return;

            PreviousActivations.Add(new Position(X, Y));
            listView2.Items.Add(new ListViewItem(new[] { x, y }));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var idx = listView2.SelectedIndices;

            foreach (int id in idx)
            {
                if (id >= 0)
                {
                    PreviousActivations.RemoveAt(id);
                    listView2.Items.RemoveAt(id);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PreviousActivations.Clear();
            listView2.Items.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var id = -1;
                int.TryParse(NewMapID, out id);

                if (id <= 0)
                {
                    MessageBox.Show("Error, it appears a valid map id was not set in the previous Window, at step 2.");
                    return;
                }


                if (SelectedArea == null || SelectedArea.ID <= 0)
                {
                    MessageBox.Show("Error, it appears a valid map id was not set in the previous Window, at step 2.");
                    return;
                }

                var LocationX = -1;
                var LocationY = -1;

                var PreviousX = -1;
                var PreviousY = -1;

                int.TryParse(textBox6.Text, out LocationX);
                int.TryParse(textBox5.Text, out LocationY);
                int.TryParse(textBox9.Text, out PreviousX);
                int.TryParse(textBox8.Text, out PreviousY);

                int levelReq = -1;
                int.TryParse(textBox7.Text, out levelReq);

                if (levelReq <= 0)
                    levelReq = 1;
                if (levelReq > 99)
                    levelReq = 99;

                if (LocationX < 0 || LocationY < 0 || PreviousX < 0 || PreviousY < 0)
                {
                    MessageBox.Show("Error, it appears you have invalid information set in step 4 or 5.");
                    return;
                }

                CreateTargetWarpTemplate(id, LocationX, LocationY, levelReq);
                CreateReturnWarpTemplate(id, PreviousX, PreviousY, levelReq);

                MessageBox.Show("Warp was created!");
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                this.Close();
            }
        }

        private void CreateReturnWarpTemplate(int id, int PreviousX, int PreviousY, int levelReq)
        {
            var template = new WarpTemplate();

            template.To = new Warp()
            {
                AreaID = SelectedArea.ID,
                Location = new Position(PreviousX, PreviousY),
                PortalKey = 0
            };

            template.ActivationMapId = id;
            template.Activations = new List<Warp>();

            foreach (var activation in PreviousActivations)
            {
                template.Activations.Add(new Warp()
                {
                    AreaID = id,
                    Location = activation,
                    PortalKey = 0
                });
            }

            template.LevelRequired = Convert.ToByte(levelReq);
            template.WarpType = WarpType.Map;
            template.WarpRadius = 0;
            template.Name = string.Format("Map Warp from {0} to {1}", id, SelectedArea.ID);

            StorageManager.WarpBucket.Save(template);
        }

        private void CreateTargetWarpTemplate(int id, int LocationX, int LocationY, int levelReq)
        {
            var template = new WarpTemplate();

            template.To = new Warp()
            {
                AreaID = id,
                Location = new Position(LocationX, LocationY),
                PortalKey = 0
            };

            template.ActivationMapId = SelectedArea.ID;
            template.Activations = new List<Warp>();

            foreach (var activation in Activations)
            {
                template.Activations.Add(new Warp()
                {
                    AreaID = SelectedArea.ID,
                    Location = activation,
                    PortalKey = 0
                });
            }

            template.LevelRequired = Convert.ToByte(levelReq);
            template.WarpType = WarpType.Map;
            template.WarpRadius = 0;
            template.Name = string.Format("Map Warp from {0} to {1}", SelectedArea.ID, id);

            StorageManager.WarpBucket.Save(template);
        }
    }
}
