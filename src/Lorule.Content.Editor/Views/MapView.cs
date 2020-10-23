using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
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
            model1.ActiveViewport.ToolBar.Visible = false;
            model1.ActiveViewport.ViewCubeIcon.Visible = false;
            model1.ActiveViewport.CoordinateSystemIcon.Visible = false;
            model1.ActiveViewport.OriginSymbol.Visible = false;
            model1.ActiveViewport.Grid.Visible = false;

            model1.ActiveViewport.DisplayMode = displayType.Rendered;
            model1.ActiveViewport.Camera.ProjectionMode = projectionType.Orthographic;
            model1.ActiveViewport.SetView(viewType.Isometric);
            model1.ActiveViewport.Rotate.Enabled = false;
            model1.Entities.Add(new devDept.Eyeshot.Entities.Circle(Plane.XY, 20.0)
            {
                Color = Color.DarkOrange,
                ColorMethod = colorMethodType.byEntity
            });
            model1.ZoomFitMode = zoomFitType.Standard;
            model1.ZoomFit();
            model1.ActionMode = actionType.ZoomWindow;
        }
    }
}
