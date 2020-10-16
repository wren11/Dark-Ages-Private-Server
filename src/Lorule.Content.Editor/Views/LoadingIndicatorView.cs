using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lorule.Editor.Views
{
    public partial class LoadingIndicatorView : Form
    {
        private readonly ILogger<LoadingIndicatorView> _logger;

        public LoadingIndicatorView(ILogger<LoadingIndicatorView> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
        }

        private CancellationTokenSource _cancellationToken;

        public async Task BeginLoad(int x, int y, Task task, CancellationTokenSource cancellationToken, Action OnCancelled)
        {
            if (cancellationToken != null)
            {
                _cancellationToken = cancellationToken;
                _cancellationToken.Token.Register(OnCancelled);
                {
                    Location = new Point(x, y);

                    Show();
                    await Task.Run(() => task, _cancellationToken.Token);
                    Hide();
                }
            }
        }

        private void LoadingIndicatorView_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            {
                _cancellationToken?.Cancel(false);
                Hide();
            }
        }

        public void SetCaption(string newText)
        {
            Text = newText;
        }
    }
}
