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
        }

        private void MapView_Load(object sender, EventArgs e)
        {

        }
    }
}
