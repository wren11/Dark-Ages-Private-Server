using Darkages;
using Lorule.Content.Editor.Dat;
using Lorule.Content.Editor.Views;
using Lorule.Editor;
using Lorule.GameServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lorule.Content.Editor
{
    public partial class FrmMain : Form
    {
        private readonly ILogger<FrmMain> _logger;
        private readonly LoadingIndicatorView _loadingIndicator;
        private readonly IServerContext _loruleServerContext;
        private readonly IServerConstants _serverConstants;
        private readonly MapView _mapView;
        private readonly IArchive _archiveService;
        private readonly EditorIOptions _editorSettings;
        private readonly IOptions<LoruleOptions> _loruleOptions;

        public FrmMain(EditorIOptions editorSettings,
            IOptions<LoruleOptions> loruleOptions, 
            ILogger<FrmMain> logger, 
            LoadingIndicatorView loadingIndicator,
            IServerContext loruleServerContext, 
            IServerConstants serverConstants, 
            MapView mapView,
            IArchive archiveService)
        {
            _editorSettings = editorSettings ?? throw new ArgumentNullException(nameof(editorSettings));
            _loruleOptions = loruleOptions ?? throw new ArgumentNullException(nameof(loruleOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadingIndicator = loadingIndicator ?? throw new ArgumentNullException(nameof(loadingIndicator));
            _loruleServerContext = loruleServerContext ?? throw new ArgumentNullException(nameof(loruleServerContext));
            _serverConstants = serverConstants ?? throw new ArgumentNullException(nameof(serverConstants));
            _mapView = mapView ?? throw new ArgumentNullException(nameof(mapView));
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));

            InitializeComponent();

            _logger.LogInformation("Editor Started.");
            _logger.LogInformation("Location: {0}", _editorSettings.Location);
            _logger.LogInformation("Es License: {0}", _editorSettings.LicenseKey);
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            await RunOperationAsync(LoadLoruleData(), "Loading Lorule Data");
            await RunOperationAsync(LoadArchives(), "Loading Archives");
            await RunOperationAsync(LoadTiles(), "Loading Tiles");
            await RunOperationAsync(CacheTiles(), "Caching Tiles");

            _mapView.ShowDialog();
        }

        private async Task LoadArchives()
        {
            _loadingIndicator.SetCaption("Loading Archives : seo");
            await _archiveService.Load("seo\\seo.dat");
            _loadingIndicator.SetCaption("Loading Archives : ia");
            await _archiveService.Load("ia\\ia.dat");
            _loadingIndicator.SetCaption("Loading Archives : hades");
            await _archiveService.Load("hades\\hades.dat");
            _loadingIndicator.SetCaption("Loading Archives : roh");
            await _archiveService.Load("roh\\roh.dat");
            _loadingIndicator.SetCaption("Loading Archives : setoa");
            await _archiveService.Load("setoa\\setoa.dat");
            _loadingIndicator.SetCaption("Loading Archives : legend");
            await _archiveService.Load("legend\\legend.dat");
            _loadingIndicator.SetCaption("Loading Archives : cious");
            await _archiveService.Load("cious\\cious.dat");
            _loadingIndicator.SetCaption("Loading Archives : khan");
            await _archiveService.Load("khan\\khan.dat");
            _loadingIndicator.SetCaption("Loading Archives : khan2");
            await _archiveService.Load("khan2\\khan2.dat");
        }

        private async Task LoadTiles()
        {
            _loadingIndicator.SetCaption("Loading Map Tiles");

            //TODO
            await Task.Delay(500);
        }

        private async Task CacheTiles()
        {
            _loadingIndicator.SetCaption("Caching Map Tiles");

            //TODO
            await Task.Delay(500);
        }

        private async Task LoadLoruleData()
        {
            _loadingIndicator.SetCaption("Loading Lorule Data...");
            await Task.Run(() => ServerContext.LoadAndCacheStorage(true));
        }

        #region Utils
        private async Task RunOperationAsync(Task task, string operationName)
        {
            await _loadingIndicator.BeginLoad(
                Width - _loadingIndicator.Width - 10,
                Height - _loadingIndicator.Height - 30,
                task,
                new CancellationTokenSource(TimeSpan.FromSeconds(30)),
                () =>
                {
                    _logger.LogInformation(
                        $"Warning, {operationName} was cancelled or took longer then 30 seconds to complete.");
                });
            _logger.LogInformation($"{operationName} Completed.");
        }
        #endregion
    }
}
