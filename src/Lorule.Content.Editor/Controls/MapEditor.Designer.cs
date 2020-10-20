using System.Collections.ObjectModel;
using devDept.Eyeshot;

namespace Lorule.Editor.Controls
{
    partial class MapEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            devDept.Eyeshot.CancelToolBarButton cancelToolBarButton1 = new devDept.Eyeshot.CancelToolBarButton("Cancel", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.ProgressBar progressBar1 = new devDept.Eyeshot.ProgressBar(devDept.Eyeshot.ProgressBar.styleType.Circular, 0, "Idle", System.Drawing.Color.Black, System.Drawing.Color.Transparent, System.Drawing.Color.Green, 1D, true, cancelToolBarButton1, false, 0.1D, true);
            devDept.Graphics.BackgroundSettings backgroundSettings1 = new devDept.Graphics.BackgroundSettings(devDept.Graphics.backgroundStyleType.Solid, System.Drawing.Color.DeepSkyBlue, System.Drawing.Color.DodgerBlue, System.Drawing.Color.White, 0.75D, null, devDept.Graphics.colorThemeType.Auto, 0.33D);
            devDept.Eyeshot.Camera camera1 = new devDept.Eyeshot.Camera(new devDept.Geometry.Point3D(1.4210854715202004E-14D, -4.9338977348156657E-15D, 72.22029999999998D), 147.11380000000003D, new devDept.Geometry.Quaternion(0.49999999999999994D, 0.49999999999999994D, 0.49999999999999994D, 0.49999999999999994D), devDept.Graphics.projectionType.Orthographic, 50D, 2.0136964316874808D, false, 0.001D);
            devDept.Eyeshot.MagnifyingGlassToolBarButton magnifyingGlassToolBarButton1 = new devDept.Eyeshot.MagnifyingGlassToolBarButton("Magnifying Glass", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.ZoomWindowToolBarButton zoomWindowToolBarButton1 = new devDept.Eyeshot.ZoomWindowToolBarButton("Zoom Window", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.ZoomToolBarButton zoomToolBarButton1 = new devDept.Eyeshot.ZoomToolBarButton("Zoom", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.PanToolBarButton panToolBarButton1 = new devDept.Eyeshot.PanToolBarButton("Pan", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.ZoomFitToolBarButton zoomFitToolBarButton1 = new devDept.Eyeshot.ZoomFitToolBarButton("Zoom Fit", devDept.Eyeshot.ToolBarButton.styleType.PushButton, true, true);
            devDept.Eyeshot.ToolBar toolBar1 = new devDept.Eyeshot.ToolBar(devDept.Eyeshot.ToolBar.positionType.HorizontalTopCenter, true, new ObservableCollection<ToolBarButton>()
            {((devDept.Eyeshot.ToolBarButton)(magnifyingGlassToolBarButton1)), ((devDept.Eyeshot.ToolBarButton)(zoomWindowToolBarButton1)), ((devDept.Eyeshot.ToolBarButton)(zoomToolBarButton1)), ((devDept.Eyeshot.ToolBarButton)(panToolBarButton1)),
                ((devDept.Eyeshot.ToolBarButton)(zoomFitToolBarButton1))
            });
            devDept.Eyeshot.Grid grid1 = new devDept.Eyeshot.Grid(new devDept.Geometry.Point3D(-100D, -100D, 0D), new devDept.Geometry.Point3D(100D, 100D, 0D), 1D, new devDept.Geometry.Plane(new devDept.Geometry.Point3D(0D, 0D, 0D), new devDept.Geometry.Vector3D(0D, 0D, 1D)), System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))), System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0))))), System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0))))), false, false, false, false, 10, 100, 10, System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90))))), System.Drawing.Color.Transparent, false, System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255))))));
            devDept.Eyeshot.RotateSettings rotateSettings1 = new devDept.Eyeshot.RotateSettings(new devDept.Eyeshot.MouseButton(devDept.Eyeshot.mouseButtonsZPR.Middle, devDept.Eyeshot.modifierKeys.None), 10D, false, 1D, devDept.Eyeshot.rotationType.Trackball, devDept.Eyeshot.rotationCenterType.CursorLocation, new devDept.Geometry.Point3D(0D, 0D, 0D), false);
            devDept.Eyeshot.ZoomSettings zoomSettings1 = new devDept.Eyeshot.ZoomSettings(new devDept.Eyeshot.MouseButton(devDept.Eyeshot.mouseButtonsZPR.Middle, devDept.Eyeshot.modifierKeys.Shift), 25, true, devDept.Eyeshot.zoomStyleType.AtCursorLocation, false, 1D, System.Drawing.Color.Empty, devDept.Eyeshot.Camera.perspectiveFitType.Accurate, false, 10, true);
            devDept.Eyeshot.PanSettings panSettings1 = new devDept.Eyeshot.PanSettings(new devDept.Eyeshot.MouseButton(devDept.Eyeshot.mouseButtonsZPR.Middle, devDept.Eyeshot.modifierKeys.Ctrl), 25, true);
            devDept.Eyeshot.NavigationSettings navigationSettings1 = new devDept.Eyeshot.NavigationSettings(devDept.Eyeshot.Camera.navigationType.Examine, new devDept.Eyeshot.MouseButton(devDept.Eyeshot.mouseButtonsZPR.Left, devDept.Eyeshot.modifierKeys.None), new devDept.Geometry.Point3D(-1000D, -1000D, -1000D), new devDept.Geometry.Point3D(1000D, 1000D, 1000D), 8D, 50D, 50D);
            devDept.Eyeshot.Viewport.SavedViewsManager savedViewsManager1 = new devDept.Eyeshot.Viewport.SavedViewsManager(8);
            this.SuspendLayout();
            this.Name = "MapEditor";
            this.Size = new System.Drawing.Size(772, 618);
            this.ResumeLayout(false);

        }

        #endregion

        private devDept.Eyeshot.ViewportLayout viewportLayout1;
    }
}
