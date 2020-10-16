using Lorule.Editor.Controls;
using System;
using System.Windows.Forms;

namespace Lorule.Editor.Views
{
    public partial class MapView : Form
    {
        private readonly IMapEditor _mapEditor;

        public MapView(IMapEditor mapEditor)
        {
            InitializeComponent();

            _mapEditor = mapEditor ?? throw new ArgumentNullException(nameof(mapEditor));
            _mapEditor.GetLayout().Dock = DockStyle.Fill;
        }

        private void MapView_Load(object sender, EventArgs e)
        {
            if (_mapEditor.GetLayout() != null) panel1?.Controls.Add(_mapEditor.GetLayout());
        }
    }
}
