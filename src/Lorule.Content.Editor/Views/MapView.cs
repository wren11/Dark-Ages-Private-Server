using System;
using System.Windows.Forms;
using Lorule.Editor;

namespace Lorule.Content.Editor.Views
{
    public partial class MapView : Form
    {

        public MapView(EditorIOptions options)
        {
            InitializeComponent();

            model1.Unlock(options?.LicenseKey);
        }

        private void MapView_Load(object sender, EventArgs e)
        {
        }
    }
}
