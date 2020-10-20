using Darkages;
using Lorule.Editor.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lorule.GameServer;

namespace Lorule.Editor
{
    public partial class FrmMain : Form
    {
        private readonly ILogger<FrmMain> _logger;
        private readonly LoadingIndicatorView _loadingIndicator;
        private readonly IServerContext _loruleServerContext;
        private readonly IServerConstants _serverConstants;
        private readonly EditorIOptions _editorSettings;
        private readonly IOptions<LoruleOptions> _loruleOptions;

        public FrmMain(EditorIOptions editorSettings, IOptions<LoruleOptions> loruleOptions, ILogger<FrmMain> logger, LoadingIndicatorView loadingIndicator, IServerContext loruleServerContext, IServerConstants serverConstants)
        {
            _editorSettings = editorSettings ?? throw new ArgumentNullException(nameof(editorSettings));
            _loruleOptions = loruleOptions ?? throw new ArgumentNullException(nameof(loruleOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadingIndicator = loadingIndicator ?? throw new ArgumentNullException(nameof(loadingIndicator));
            _loruleServerContext = loruleServerContext ?? throw new ArgumentNullException(nameof(loruleServerContext));
            _serverConstants = serverConstants ?? throw new ArgumentNullException(nameof(serverConstants));

            InitializeComponent();

            _logger.LogInformation("Editor Started.");
            _logger.LogInformation("Location: {0}", _editorSettings.Location);
            _logger.LogInformation("Es License: {0}", _editorSettings.LicenseKey);
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            var task = LoadArchives();
            await _loadingIndicator.BeginLoad(
                Width - Width / 2,
                Height - Height / 2 + 80,
                task,
                new CancellationTokenSource(TimeSpan.FromSeconds(30)), () =>
                {
                    _logger.LogInformation("Warning, Loading Archives operation was cancelled or took longer then 30 seconds to load.");
                });
            _logger.LogInformation("All Archives completed loading.");
        }

        private async Task LoadArchives()
        {
            ServerContext.StoragePath = _loruleOptions.Value.Location;

            _loadingIndicator.SetCaption("Loading Lorule Data...");
            await Task.Run(() => ServerContext.LoadAndCacheStorage(true));
            _loadingIndicator.SetCaption("Loading Lorule Data... Completed.");

            await Task.CompletedTask;
        }
    }
}
