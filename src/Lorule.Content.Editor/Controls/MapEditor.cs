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
        private readonly IOptions<EditorSettings> _editorOptions;

        public ViewportLayoutBase GetLayout()
        {
            return viewportLayout1;
        }

        public IViewport View =>

            viewportLayout1 != null && (viewportLayout1.ActiveViewport >= 0 && viewportLayout1.Viewports.Count > viewportLayout1.ActiveViewport)
                ? viewportLayout1.Viewports[viewportLayout1.ActiveViewport]
                : null;

        public MapEditor(IOptions<EditorSettings> editorOptions)
        {
            InitializeComponent();

            _editorOptions = editorOptions ?? throw new ArgumentNullException(nameof(editorOptions));
            if (_editorOptions?.Value != null)
                viewportLayout1.Unlock(_editorOptions.Value.LicenseKey);
        }
    }
}
