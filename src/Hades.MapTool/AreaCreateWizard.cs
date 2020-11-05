using Darkages;
using Darkages.Storage;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Content_Maker.Properties;
using Lorule.Editor;
using Lorule.GameServer;
using Microsoft.Extensions.Options;

namespace Content_Maker
{
    public partial class AreaCreateWizard : Form
    {
        private readonly IServerContext _serverContext;
        private readonly IOptions<LoruleOptions> _loruleOptions;
        private readonly EditorOptions _editorOptions;
        private string SelectedMap { get; set; }

        public AreaCreateWizard(IServerContext serverContext, IOptions<LoruleOptions> loruleOptions, EditorOptions editorOptions)
        {
            _serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
            _loruleOptions = loruleOptions ?? throw new ArgumentNullException(nameof(loruleOptions));
            _editorOptions = editorOptions ?? throw new ArgumentNullException(nameof(editorOptions));
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            new WarpManager(textBox3.Text).ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = @"Map Files|*.map",
                Multiselect = false,
                InitialDirectory = _editorOptions.Location + "\\Assets\\MapFiles"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var match = Regex.Match(ofd.FileName, @"lod\d*[^.map]");
                var mapId = match.Success ? match.Value.Replace("lod", string.Empty) : "error";

                if (mapId != "error") textBox3.Text = mapId;

                SelectedMap = ofd.FileName;
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

        private async void AreaCreateWizard_Load(object sender, EventArgs e)
        {
            var title = Text;

            void SetControls(bool val)
            {
                foreach (Control control in Controls) control.Enabled = val;
            }

            Text += Resources.AreaCreateWizard_AreaCreateWizard_Load______Loading_Content___;
            SetControls(false);
            await Task.Run(() => ServerContext.LoadAndCacheStorage(true));
            SetControls(true);
            Text = title;

            button2.Enabled = true;
            button4.Enabled = false;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (ServerContext.GlobalMapCache.Count(i =>
                i.Value.Name.Equals(textBox4.Text, StringComparison.OrdinalIgnoreCase)
                || i.Value.ID == Convert.ToInt32(textBox3.Text)) > 0)
            {
                MessageBox.Show(Resources.AreaCreateWizard_Button3_Click_This_map_appears_to_exist_already_);
                return;
            }


            try
            {
                var map = new Area
                {
                    Name = textBox4.Text,
                    ContentName = textBox6.Text,
                    Rows = Convert.ToUInt16(textBox2.Text),
                    Cols = Convert.ToUInt16(textBox1.Text),
                    ID = Convert.ToInt32(textBox3.Text),
                    Music = Convert.ToInt32(textBox5.Text),
                    Ready = false,
                    Flags = radioButton2.Checked
                        ? Darkages.Types.MapFlags.Default
                        : Darkages.Types.MapFlags.PlayerKill
                };

                {
                    var path = Path.GetFullPath(ServerContext.StoragePath + $@"\maps\lod{map.ID}.map");

                    if (!File.Exists(path))
                        File.Copy(SelectedMap, path, true);

                    StorageManager.AreaBucket.Save(map);
                    ServerContext.LoadAndCacheStorage(true);

                    MessageBox.Show(@"New Area Saved.");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            new WorldManager().ShowDialog();
        }
    }
}