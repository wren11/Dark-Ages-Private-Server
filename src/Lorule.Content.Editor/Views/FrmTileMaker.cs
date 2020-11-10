using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lorule.Content.Editor.Views
{
    public partial class FrmTileMaker : Form
    {
        public int GridSize = 10;

        public int XPadding, YPadding;
        
        private Point _startingPoint = Point.Empty;
        
        private Point _movingPoint = Point.Empty;
        
        private bool _panning;

        public FrmTileMaker()
        {
            InitializeComponent();
        }

        private void ZoomPictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RenderGrid(e.Graphics);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void FrmTileMaker_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(560, 480);
            pictureBox1.Paint += ZoomPictureBox1_Paint;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
        }


        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _panning = false;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _panning = true;
            _startingPoint = new Point(e.Location.X - _movingPoint.X, e.Location.Y - _movingPoint.Y);
        }


        private int Centerx => XPadding + pictureBox1.Width / 2;
        private int Centery => YPadding + pictureBox1.Height / 2;

        public int TileWidth = 56;
        public int TileHeight = 27;

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_panning)
                return;

            _movingPoint = new Point(e.Location.X - _startingPoint.X, e.Location.Y - _startingPoint.Y);

            XPadding = _movingPoint.X;
            YPadding = _movingPoint.Y;

            pictureBox1.Invalidate();
        }

        private void RenderGrid(Graphics gfx)
        {
            var tileColumnOffset = TileWidth;
            var tileRowOffset = TileHeight;

            for (var xi = -GridSize; xi < +GridSize; xi++)
            {
                for (var yi = -GridSize; yi < +GridSize; yi++)
                {
                    var offX = xi * tileColumnOffset / 2 + yi * tileColumnOffset / 2 + Centerx;
                    var offY = yi * tileRowOffset / 2 - xi * tileRowOffset / 2 + Centery;

                    gfx.DrawLine(Pens.SteelBlue, offX, offY + tileRowOffset / 2, offX + tileColumnOffset / 2, offY);
                    gfx.DrawLine(Pens.SteelBlue, offX + tileColumnOffset / 2, offY, offX + tileColumnOffset, offY + tileRowOffset / 2);
                    gfx.DrawLine(Pens.SteelBlue, offX + tileColumnOffset, offY + tileRowOffset / 2, offX + tileColumnOffset / 2, offY + tileRowOffset);
                    gfx.DrawLine(Pens.SteelBlue, offX + tileColumnOffset / 2, offY + tileRowOffset, offX, offY + tileRowOffset / 2);
                }
            }
        }
    }
}
