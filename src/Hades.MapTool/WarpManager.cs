using Darkages;
using Darkages.Storage;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Darkages.Templates;

namespace Content_Maker
{
    public partial class WarpManager : Form
    {
        private readonly List<Position> _activations = new List<Position>();
        private readonly List<Position> _previousActivations = new List<Position>();

        private Area _selectedArea;


        public string NewMapId { get; set; }

        public WarpManager(string mapId)
        {
            NewMapId = mapId;
            InitializeComponent();
        }

        private void WarpManager_Load(object sender, EventArgs e)
        {
            var connectedMaps = ServerContext.GlobalMapCache.Select(i => i.Value.ID).Intersect(
                ServerContext.GlobalWarpTemplateCache.Select(i => i.ActivationMapId));

            var validContent = (
                from id in connectedMaps
                where ServerContext.GlobalMapCache.ContainsKey(id)
                select id + " |  " + ServerContext.GlobalMapCache[id].Name).ToList();

            comboBox1.DataSource = validContent.ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var id = comboBox1.Text.Split('|').FirstOrDefault()?.Trim();

            if (id == null)
                return;

            var area = ServerContext.GlobalMapCache[Convert.ToInt32(id)];

            if (area != null)
            {
                richTextBox1.Text = @"Selected Map : " + area.Name;
                richTextBox1.AppendText("\nMap Dimensions : " + area.Cols + "," + area.Rows);

                _selectedArea = area;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var x = textBox1.Text;
            var y = textBox2.Text;

            int.TryParse(x, out var xCoordinate);
            int.TryParse(y, out var yCoordinate);

            if (xCoordinate < 0 || yCoordinate < 0)
                return;

            _activations.Add(new Position(xCoordinate, yCoordinate));
            listView1.Items.Add(new ListViewItem(new[] {x, y}));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var idx = listView1.SelectedIndices;

            foreach (int id in idx)
            {
                if (id < 0)
                    continue;

                _activations.RemoveAt(id);
                listView1.Items.RemoveAt(id);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _activations.Clear();
            listView1.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var x = textBox1.Text;
            var y = textBox2.Text;

            int.TryParse(x, out var xCoordinate);
            int.TryParse(y, out var yCoordinate);

            if (xCoordinate < 0 || yCoordinate < 0)
                return;

            _previousActivations.Add(new Position(xCoordinate, yCoordinate));
            listView2.Items.Add(new ListViewItem(new[] {x, y}));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var idx = listView2.SelectedIndices;

            foreach (int id in idx)
            {
                if (id < 0)
                    continue;

                _previousActivations.RemoveAt(id);
                listView2.Items.RemoveAt(id);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _previousActivations.Clear();
            listView2.Items.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int.TryParse(NewMapId, out var id);

                if (id <= 0)
                {
                    MessageBox.Show(@"Error, it appears a valid map id was not set in the previous Window, at step 2.");
                    return;
                }


                if (_selectedArea == null || _selectedArea.ID <= 0)
                {
                    MessageBox.Show(@"Error, it appears a valid map id was not set in the previous Window, at step 2.");
                    return;
                }

                int.TryParse(textBox6.Text, out var locationX);
                int.TryParse(textBox5.Text, out var locationY);
                int.TryParse(textBox9.Text, out var previousX);
                int.TryParse(textBox8.Text, out var previousY);
                int.TryParse(textBox7.Text, out var levelReq);

                if (levelReq <= 0)
                    levelReq = 1;
                if (levelReq > 99)
                    levelReq = 99;

                if (locationX < 0 || locationY < 0 || previousX < 0 || previousY < 0)
                {
                    MessageBox.Show(@"Error, it appears you have invalid information set in step 4 or 5.");
                    return;
                }

                CreateTargetWarpTemplate(id, locationX, locationY, levelReq);
                CreateReturnWarpTemplate(id, previousX, previousY, levelReq);

                MessageBox.Show(@"Warp was created!");
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Close();
            }
        }

        private void CreateReturnWarpTemplate(int id, int previousX, int previousY, int levelReq)
        {
            var template = new WarpTemplate
            {
                To = new Warp {AreaID = _selectedArea.ID, Location = new Position(previousX, previousY)},
                ActivationMapId = id,
                Activations = new List<Warp>()
            };


            foreach (var activation in _previousActivations)
                template.Activations.Add(new Warp()
                {
                    AreaID = id,
                    Location = activation
                });

            template.LevelRequired = Convert.ToByte(levelReq);
            template.WarpType = WarpType.Map;
            template.WarpRadius = 0;
            template.Name = string.Format("Map Warp from {0} to {1}", id, _selectedArea.ID);

            StorageManager.WarpBucket.Save(template);
        }

        private void CreateTargetWarpTemplate(int id, int locationX, int locationY, int levelReq)
        {
            var template = new WarpTemplate
            {
                To = new Warp {AreaID = id, Location = new Position(locationX, locationY)},
                ActivationMapId = _selectedArea.ID,
                Activations = new List<Warp>()
            };

            foreach (var activation in _activations)
                template.Activations.Add(new Warp()
                {
                    AreaID = _selectedArea.ID,
                    Location = activation
                });

            template.LevelRequired = Convert.ToByte(levelReq);
            template.WarpType = WarpType.Map;
            template.WarpRadius = 0;
            template.Name = $"Map Warp from {_selectedArea.ID} to {id}";

            StorageManager.WarpBucket.Save(template);
        }
    }
}