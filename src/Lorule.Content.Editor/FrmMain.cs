using Darkages;
using Lorule.Client.Base.Dat;
using Lorule.Client.Base.Types;
using Lorule.Content.Editor.Dat;
using Lorule.Content.Editor.Views;
using Lorule.Editor;
using Lorule.GameServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Map = Lorule.Client.Base.Types.Map;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;

namespace Lorule.Content.Editor
{
    public partial class FrmMain : Form
    {
        private readonly LoadingIndicatorView _loadingIndicator;
        private readonly IServerContext _loruleServerContext;
        private readonly IServerConstants _serverConstants;
        private readonly MapView _mapView;
        private readonly IArchive _archiveService;
        private readonly IPaletteCollection _paletteService;
        private readonly EditorIOptions _editorSettings;
        private readonly IOptions<LoruleOptions> _loruleOptions;
        private readonly ILogger<FrmMain> _logger;

        private ArchivedItem _baseTileSet;

        public FrmMain(EditorIOptions editorSettings,
            IOptions<LoruleOptions> loruleOptions,
            ILogger<FrmMain> logger,
            LoadingIndicatorView loadingIndicator,
            IServerContext loruleServerContext,
            IServerConstants serverConstants,
            MapView mapView,
            IArchive archiveService,
            IPaletteCollection paletteService)
        {
            _editorSettings = editorSettings ?? throw new ArgumentNullException(nameof(editorSettings));
            _loruleOptions = loruleOptions ?? throw new ArgumentNullException(nameof(loruleOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadingIndicator = loadingIndicator ?? throw new ArgumentNullException(nameof(loadingIndicator));
            _loruleServerContext = loruleServerContext ?? throw new ArgumentNullException(nameof(loruleServerContext));
            _serverConstants = serverConstants ?? throw new ArgumentNullException(nameof(serverConstants));
            _mapView = mapView ?? throw new ArgumentNullException(nameof(mapView));
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
            _paletteService = paletteService ?? throw new ArgumentNullException(nameof(paletteService));

            InitializeComponent();

            _logger.LogInformation("Editor Started.");
            _logger.LogInformation("Location: {0}", _editorSettings.Location);
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            await RunOperationAsync(LoadLoruleData(), "Loading Lorule Data");
            await RunOperationAsync(LoadArchives(), "Loading Archives");
            await RunOperationAsync(LoadTiles(), "Loading BaseTileSet");
            await RunOperationAsync(CacheTiles(), "Caching BaseTileSet");
        }

        public async Task LoadTiles(bool snow = false)
        {
            GetBaseTiles(snow);
            LoadPalettes();
            LoadPaletteTables();

            var tileCollection = LoadBaseTiles();

            //load the starting map.
            RenderLoruleStart(tileCollection);

            await Task.CompletedTask;
        }

        private void RenderLoruleStart(TileCollection tileCollection)
        {
            _mapView.Show();

            if (_serverConstants.StartingMap == 0)
                return;

            var map = ServerContext.GlobalMapCache[_serverConstants.StartingMap];
            if (map == null || string.IsNullOrEmpty(map.FilePath))
                return;

            var mapTiles = Map.LoadMapTiles(map.FilePath);
            if (mapTiles == null)
                return;


            foreach (var tile in mapTiles)
            {
                if (tile == null || tile.Floor <= 0)
                    continue;

                var index = tile.Floor > 0 ? tile.Floor - 1 : 0;
                var floorTile = tileCollection[index];

                if (floorTile != null)
                {
                    var floorPalette = _paletteService.GetBackgroundPaletteIndex(tile.Floor + 1);
                    var bmp = RenderFloorTile(floorTile.Data, floorPalette.Item2);
                    bmp.MakeTransparent(Color.Black);
                    break;
                }
            }
        }

        private static void ExportFloorTile(Bitmap bmp, MapTile tile, string dir, bool transparent = false)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (transparent)
            {
                bmp.MakeTransparent(Color.Black);
            }

            bmp.Save(Path.Combine(dir, $"{tile.Floor + 1}.png"), ImageFormat.Png);
        }

        private static void ExportPalette((int, Palette) floorPalette, string dir)
        {
            var palPath = Path.Combine(dir, floorPalette.Item1 + ".pal");
            using var bw = new BinaryWriter(File.OpenWrite(palPath));
            bw.Seek(0, SeekOrigin.Begin);
            foreach (var color in floorPalette.Item2.Colors)
            {
                bw.Write(color.R);
                bw.Write(color.G);
                bw.Write(color.B);
            }
        }



        private static unsafe Bitmap RenderFloorTile(IReadOnlyList<byte> data, Palette palette)
        {
            var image = new Bitmap(56, 27);
            var bmd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly,
                image.PixelFormat);

            for (var y = 0; y < bmd.Height; y++)
            {
                var row = (byte*) bmd.Scan0 + y * bmd.Stride;

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

        public async Task LoadLoruleData()
        {
            _loadingIndicator.SetCaption("Loading Lorule Data...");
            {
                await Task.Run(() => ServerContext.LoadAndCacheStorage(true));
            }
        }

        public async Task LoadArchives()
        {
            _loadingIndicator.SetCaption("Loading Archives");
            {
                await _archiveService.Load("seo\\seo.dat");
                await _archiveService.Load("ia\\ia.dat");
            }
        }

        private void LoadPaletteTables()
        {
            _loadingIndicator.SetCaption("Loading Palette Tables");
            {
                _paletteService.LoadTables("ia\\ia.dat", "stc");
                _paletteService.LoadTables("seo\\seo.dat", "mpt");
            }
        }

        private void LoadPalettes()
        {
            _loadingIndicator.SetCaption("Loading Palettes");
            {
                _paletteService.LoadPalettes("seo\\seo.dat", "mpt");
                _paletteService.LoadPalettes("ia\\ia.dat", "stc");
            }
        }

        private TileCollection LoadBaseTiles()
        {
            var tileLoader = new TileCollection(_baseTileSet);
            _loadingIndicator.SetCaption("Loading Base Tiles");
            {
                tileLoader.Load();
            }

            return tileLoader;
        }

        private void GetBaseTiles(bool snow)
        {
            _loadingIndicator.SetCaption("Loading Required Archives");
            {
                _baseTileSet = snow
                    ? _archiveService.Get("TILEAS.BMP", "seo\\seo.dat")
                    : _archiveService.Get("TILEA.BMP", "seo\\seo.dat");
            }
        }

        public async Task CacheTiles()
        {
            _loadingIndicator.SetCaption("Caching Map BaseTileSet");
            {
                await Task.CompletedTask;
            }
        }

        public async Task RunOperationAsync(Task task, string operationName, CancellationTokenSource token = default)
        {
            _logger.LogInformation($"{operationName} Started.");
            {
                await _loadingIndicator.BeginLoad(
                    Width - _loadingIndicator.Width - 15, Height - _loadingIndicator.Height - 30,
                    task,
                    token ?? new CancellationTokenSource(TimeSpan.FromSeconds(30)),
                    () =>
                    {
                        _logger.LogInformation(
                            $"Warning, {operationName} was cancelled or took longer then 30 seconds to complete.");
                    });
            }
            _logger.LogInformation($"{operationName} Completed.");
        }
    }
}