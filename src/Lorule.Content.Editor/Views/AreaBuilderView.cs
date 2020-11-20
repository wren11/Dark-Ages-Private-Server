using Darkages;
using Lorule.Client.Base.Dat;
using Lorule.Client.Base.Types;
using Lorule.Content.Editor.Dat;
using Lorule.Editor;
using Lorule.GameServer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Map = Lorule.Client.Base.Types.Map;

namespace Lorule.Content.Editor.Views
{
    public partial class AreaBuilderView : Form
    {
        private readonly IArchive _archiveService;
        private readonly IServerContext _serverContext;
        private readonly EditorOptions _editorOptions;
        private readonly IOptions<LoruleOptions> _lorOptions;
        private readonly IServerConstants _serverConstants;
        private readonly IPaletteCollection _paletteCollection;

        private ArchivedItem _baseTileSet;
        private TileCollection _tileCollection;
        private Point _startingPoint = Point.Empty;
        private Point _movingPoint = Point.Empty;
        private List<(int, Tile2D)> _grid;

        private bool _panning;
        private int _xPadding;
        private int _yPadding;

        public AreaBuilderView(
            IArchive archiveService, 
            IServerContext serverContext,
            EditorOptions editorOptions, 
            IOptions<LoruleOptions> lorOptions, 
            IServerConstants serverConstants, 
            IPaletteCollection paletteCollection)
        {
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
            _serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
            _editorOptions = editorOptions ?? throw new ArgumentNullException(nameof(editorOptions));
            _lorOptions = lorOptions ?? throw new ArgumentNullException(nameof(lorOptions));
            _serverConstants = serverConstants ?? throw new ArgumentNullException(nameof(serverConstants));
            _paletteCollection = paletteCollection ?? throw new ArgumentNullException(nameof(paletteCollection));

            InitializeComponent();
        }

        private void ZoomPictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            RenderGrid(e.Graphics);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        public class Tile2D
        {
            public readonly int X, Y, IsometricX, IsometricY, ScreenX, ScreenY, Row, Column;
            public readonly Bitmap TileBitmap;

            public Tile2D(int row, int column, int x, int y, int isometricX, int isometricY, int screenX, int screenY, Bitmap tileBitmap)
            {
                Row = row;
                Column = column;

                X = x;
                Y = y;

                IsometricX = isometricX;
                IsometricY = isometricY;

                ScreenX = screenX;
                ScreenY = screenY;

                TileBitmap = tileBitmap;
            }
        }

        private void FrmTileMaker_Load(object sender, EventArgs e)
        {
            _grid = new List<(int, Tile2D)>();

            //Load Default Map.

            var startingMapNumber = _serverConstants.StartingMap;

            if (ServerContext.GlobalMapCache.ContainsKey(startingMapNumber) && ServerContext.GlobalMapCache[startingMapNumber] != null)
            {
                var startingMap = ServerContext.GlobalMapCache[startingMapNumber];

                var mapTiles = Map.LoadMapTiles(startingMap.FilePath).ToArray();
                if (mapTiles.Length == 0)
                    return;

                var mapWidth = startingMap.Cols * TileWidth;
                var mapHeight = startingMap.Rows * TileHeight;

                for (var i = 0; i < startingMap.Rows; i++)
                {
                    for (var j = 0; j < startingMap.Cols; j++)
                    {
                        //tiles[y * width + x];
                        var tile = mapTiles[i * startingMap.Rows + j];
                        var index = tile.Floor > 0 ? tile.Floor - 1 : 0;
                        var floorTile = _tileCollection[index];

                        if (floorTile != null)
                        {
                            var floorPalette = _paletteCollection.GetBackgroundPaletteIndex(tile.Floor + 1);
                            var bmp = CreateTileBmp(floorTile.Data, floorPalette.Item2);

                            bmp.MakeTransparent(Color.Black);
                            var x = j * TileWidth;
                            var y = i * TileHeight;

                            var mx = (x - x);
                            var my = (x + y) / 2;

                            var screenX = (j * TileWidth / 2) - (i * TileWidth / 2);
                            var screenY = (i * TileHeight / 2) + (j * TileHeight / 2);

                            _grid.Add((startingMapNumber, new Tile2D(j, i, x, y, mx, my, screenX, screenY, bmp)));
                        }
                    }
                }

                pictureBox1.Image = new Bitmap(mapWidth, mapHeight);
            }

            pictureBox1.Paint += ZoomPictureBox1_Paint;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
        }

        private unsafe Bitmap CreateTileBmp(IReadOnlyList<byte> data, Palette palette)
        {
            var image = new Bitmap(56, 27);
            var bmd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);

            for (var y = 0; y < bmd.Height; y++)
            {
                var row = (byte*)bmd.Scan0 + y * bmd.Stride;

                for (var x = 0; x < bmd.Width; x++)
                {
                    var index = y * 56 + x;
                    var colorIndex = data[index];

                    if (colorIndex <= 0)
                        continue;

                    row[x * 4] = palette[colorIndex].B;
                    row[x * 4 + 1] = palette[colorIndex].G;
                    row[x * 4 + 2] = palette[colorIndex].R;
                    row[x * 4 + 3] = palette[colorIndex].A;
                }
            }

            image.UnlockBits(bmd);
            return image;
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


        private int Centerx => _xPadding + pictureBox1.Width / 2;
        private int Centery => _yPadding + pictureBox1.Height / 2;

        public int TileWidth  = 56;
        public int TileHeight = 27;

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_panning)
                return;

            _movingPoint = new Point(e.Location.X - _startingPoint.X, e.Location.Y - _startingPoint.Y);

            _xPadding = _movingPoint.X;
            _yPadding = _movingPoint.Y;
            pictureBox1.Invalidate();
        }

        private void RenderGrid(Graphics gfx)
        {
            var tileColumnOffset = TileWidth;
            var tileRowOffset    = TileHeight;


            foreach (var (_, Tile2D) in _grid)
            {
                var offsetX = Tile2D.ScreenX + Centerx;
                var offsetY = Tile2D.ScreenY + Centery;

                gfx.FillEllipse(Brushes.Orange, offsetX + (TileWidth / 2), offsetY + (TileHeight / 2), 2, 2);
           
                gfx.DrawImageUnscaled(Tile2D.TileBitmap, offsetX, offsetY);

                if (_showGrid)
                {
                    gfx.DrawLine(Pens.DarkGray, offsetX, offsetY + tileRowOffset / 2, offsetX + tileColumnOffset / 2,
                        offsetY);
                    gfx.DrawLine(Pens.DarkGray, offsetX + tileColumnOffset / 2, offsetY, offsetX + tileColumnOffset,
                        offsetY + tileRowOffset / 2);
                    gfx.DrawLine(Pens.DarkGray, offsetX + tileColumnOffset, offsetY + tileRowOffset / 2,
                        offsetX + tileColumnOffset / 2, offsetY + tileRowOffset);
                    gfx.DrawLine(Pens.DarkGray, offsetX + tileColumnOffset / 2, offsetY + tileRowOffset, offsetX,
                        offsetY + tileRowOffset / 2);
                }
            }
        }

        public void Initialize(ArchivedItem baseTileSet, TileCollection tileCollection)
        {
            _baseTileSet = baseTileSet;
            _tileCollection = tileCollection;
        }

        private bool _showGrid => gridToolStripMenuItem.Checked;

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
