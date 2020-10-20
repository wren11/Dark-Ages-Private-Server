using Microsoft.Extensions.Options;
using System;
using System.Windows.Forms;
using devDept.Eyeshot;
using devDept.Graphics;

namespace Lorule.Editor.Controls
{
    public interface IMapEditor
    {
        ViewportLayoutBase GetLayout();

        IViewport View { get; }
    }

    public partial class MapEditor : UserControl, IMapEditor
    {
        private readonly IOptions<EditorIOptions> _editorOptions;

        public ViewportLayoutBase GetLayout()
        {
            return viewportLayout1;
        }

        public IViewport View => null;

        public MapEditor(IOptions<EditorIOptions> editorOptions)
        {
            InitializeComponent();

            _editorOptions = editorOptions ?? throw new ArgumentNullException(nameof(editorOptions));
            if (_editorOptions?.Value != null)
                viewportLayout1.Unlock(_editorOptions.Value.LicenseKey);
        }
    }
}
