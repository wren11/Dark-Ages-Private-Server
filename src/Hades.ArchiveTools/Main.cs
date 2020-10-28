using Lorule.Client.Base.Dat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Hades.ArchiveTools
{
    public partial class Main : Form
    {
        private readonly IArchive _archivesArchive;

        public List<ArchivedItem> LoadedArchivedItems = new List<ArchivedItem>();

        public Main(IArchive archivesArchive)
        {
            _archivesArchive = archivesArchive ?? throw new ArgumentNullException(nameof(archivesArchive));
            InitializeComponent();
        }

        private async void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadedArchivedItems = new List<ArchivedItem>();

            using var ofd = new OpenFileDialog
            {
                Multiselect = false, Filter = "DAT Archives|*.dat", AutoUpgradeEnabled = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _archivesArchive.SetLocation(ofd.FileName);

                var name = Path.GetFileNameWithoutExtension(ofd.FileName);

                if (string.IsNullOrEmpty(name))
                    return;

                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Value = 0;
                progressBar1.Visible = true;

                await foreach (var item in _archivesArchive.UnpackArchive(ofd.FileName))
                {
                    LoadedArchivedItems.Add(item);
                }

                progressBar1.Value = 0;
                progressBar1.Visible = false;

                MessageBox.Show($@"Success, Archive loaded.");
            }
        }

        private async void compileAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var bfd = new FolderBrowserDialog {Description = @"Select Location", ShowNewFolderButton = true};

            if (bfd.ShowDialog() != DialogResult.OK)
                return;

            var savingLocation = bfd.SelectedPath;

            if (!Directory.Exists(savingLocation))
                return;

            var count = 0;
            var total = LoadedArchivedItems.Count;

            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Visible = true;
            progressBar1.Value = 0;

            foreach (var item in LoadedArchivedItems)
            {
                await item.Save(savingLocation);
                count++;

                progressBar1.Value = Math.Abs(count * 100 / total);
            }

            progressBar1.Value = 0;
            progressBar1.Visible = false;

            MessageBox.Show($@"Success, All {count} files have been extracted.");
        }

        private void compileFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var bfd = new FolderBrowserDialog { Description = @"Select extracted Data to Compile.", ShowNewFolderButton = false };

            if (bfd.ShowDialog() != DialogResult.OK) return;
            var importingLocation = bfd.SelectedPath;

            if (!Directory.Exists(importingLocation))
                return;

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            using var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "DAT Archives|*.dat";
            saveDialog.CreatePrompt = true;
            saveDialog.AutoUpgradeEnabled = true;

            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            var outputLocation = saveDialog.FileName;

            _archivesArchive.PackArchive(importingLocation, outputLocation);

            progressBar1.Value = 0;
            progressBar1.Visible = false;

            MessageBox.Show($@"Success, File has been created.");
        }
    }
}
