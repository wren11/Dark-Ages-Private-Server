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
        private readonly List<Position> _worldactivations = new List<Position>();
        private readonly List<Position> _activations = new List<Position>();
        private readonly List<Position> _previousActivations = new List<Position>();

        private Area _selectedArea, _connectingArea;


        public string NewMapId { get; set; }

        public WarpManager(string mapId)
        {
            NewMapId = mapId;
            InitializeComponent();
        }

        private void WarpManager_Load(object sender, EventArgs e)
        {
            ServerContext.LoadAndCacheStorage(true);

            var connectedMaps = ServerContext.GlobalMapCache.Select(i => i.Value.ID);

            var validContent = (
                from id in connectedMaps
                where ServerContext.GlobalMapCache.ContainsKey(id)
                select id + " |  " + ServerContext.GlobalMapCache[id].Name).ToList();

            comboBox1.DataSource = validContent.ToList();
            comboBox2.DataSource = validContent.ToList();

            comboBox3.DataSource = ServerContext.GlobalWorldMapTemplateCache.Select(n => n.Value.Name + $"({n.Value.FieldNumber})").ToList();
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
            var x = textBox3.Text;
            var y = textBox4.Text;

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
                

                if (_selectedArea == null || _selectedArea.ID <= 0)
                {
                    MessageBox.Show(@"Error, No Selected Area. Select an existing Map.");
                    return;
                }

                if (_connectingArea == null || _connectingArea.ID <= 0)
                {
                    MessageBox.Show(@"Error, No Selected Area. Select a connecting Map.");
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

                if (_selectedArea != null && _connectingArea != null)
                {
                    CreateTargetWarpTemplate(locationX, locationY, levelReq);
                    CreateReturnWarpTemplate(previousX, previousY, levelReq);
                    MessageBox.Show(@"Warp was created!");
                }
            }
            catch (Exception)
            {
                // ignored
            }

        }

        private void CreateReturnWarpTemplate(int previousX, int previousY, int levelReq)
        {
            var template = new WarpTemplate
            {
                To = new Warp {AreaID = _selectedArea.ID, Location = new Position(previousX, previousY)},
                ActivationMapId = _connectingArea.ID,
                Activations = new List<Warp>()
            };


            foreach (var activation in _previousActivations)
                template.Activations.Add(new Warp()
                {
                    AreaID = _connectingArea.ID,
                    Location = activation
                });

            template.LevelRequired = Convert.ToByte(levelReq);
            template.WarpType = WarpType.Map;
            template.WarpRadius = 0;
            template.Name = $"Map Warp from {_connectingArea.ID} to {_selectedArea.ID}";

            StorageManager.WarpBucket.Save(template);
        }

        private void CreateTargetWarpTemplate(int locationX, int locationY, int levelReq)
        {
            var template = new WarpTemplate
            {
                To = new Warp {AreaID = _connectingArea.ID, Location = new Position(locationX, locationY)},
                ActivationMapId =_selectedArea.ID,
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
            template.Name = $"Map Warp from {_selectedArea.ID} to {_connectingArea.ID}";

            StorageManager.WarpBucket.Save(template);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            var world = ServerContext.GlobalWorldMapTemplateCache.Values.ElementAt(comboBox3.SelectedIndex);
            var template = new WarpTemplate();

            template.To = new Warp()
            {
                AreaID = 0,
                PortalKey = (int)world.FieldNumber
            };

            template.ActivationMapId = _selectedArea.ID;
            template.Activations = new List<Warp>();

            foreach (var activation in _worldactivations)
                template.Activations.Add(new Warp()
                {
                    AreaID = _selectedArea.ID,
                    Location = activation
                });

            template.LevelRequired = 1;
            template.WarpType = WarpType.World;
            template.WarpRadius = 0;
            template.Name = $"Warp from {_selectedArea.ID} to World Map.";

            StorageManager.WarpBucket.Save(template);
            ServerContext.LoadAndCacheStorage(true);
            MessageBox.Show("World Map Warp Created.");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            var x = textBox11.Text;
            var y = textBox10.Text;

            int.TryParse(x, out var xCoordinate);
            int.TryParse(y, out var yCoordinate);

            if (xCoordinate < 0 || yCoordinate < 0)
                return;

            _worldactivations.Add(new Position(xCoordinate, yCoordinate));
            listView3.Items.Add(new ListViewItem(new[] { x, y }));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var idx = listView3.SelectedIndices;

            foreach (int id in idx)
            {
                if (id < 0)
                    continue;

                _worldactivations.RemoveAt(id);
                listView3.Items.RemoveAt(id);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _worldactivations.Clear();
            listView3.Clear();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var id = comboBox2.Text.Split('|').FirstOrDefault()?.Trim();

            if (id == null)
                return;

            var area = ServerContext.GlobalMapCache[Convert.ToInt32(id)];

            if (area != null)
            {
                richTextBox2.Text = @"Selected Map : " + area.Name;
                richTextBox2.AppendText("\nMap Dimensions : " + area.Cols + "," + area.Rows);

                _connectingArea = area;
            }
        }
    }
}