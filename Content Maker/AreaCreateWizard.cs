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

        private void button2_Click(object sender, EventArgs e)
        {
            new WarpManager(textBox3.Text).ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
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


        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button4.Enabled = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button4.Enabled = false;
        }

        private void AreaCreateWizard_Load(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button4.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ServerContext.GlobalMapCache.Count(i => i.Value.Name.Equals(textBox4.Text, StringComparison.OrdinalIgnoreCase)
                || i.Value.Number == Convert.ToInt32(textBox3.Text)) > 0)
            {
                MessageBox.Show("This map appears to exist already.");
                return;
            }


            try
            {
                var map = new Area();
                map.Name = textBox4.Text;
                map.Rows = Convert.ToUInt16(textBox2.Text);
                map.Cols = Convert.ToUInt16(textBox1.Text);
                map.ID = Convert.ToInt32(textBox3.Text);
                map.Music = Convert.ToInt32(textBox5.Text);
                map.Number = map.ID;
                map.Ready = false;

                map.Flags = radioButton2.Checked ? Darkages.Types.MapFlags.Default : Darkages.Types.MapFlags.PlayerKill;
                {
                    var path = ServerContext.StoragePath + string.Format(@"\maps\lod{0}.map", map.Number);

                    if (!File.Exists(path))
                    {
                        File.Move(SelectedMap, path);
                    }

                    StorageManager.AreaBucket.Save(map);
                    ServerContext.LoadAndCacheStorage();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error, Please check your information.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new WorldManager().ShowDialog();
        }
    }
}
