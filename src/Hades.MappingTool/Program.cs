using Darkages;
using Darkages.Network.Object;
using Lorule.Editor;
using Lorule.GameServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace Content_Maker
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("LoruleConfig.json");
            var config = builder.Build();
            var constants = config.GetSection("ServerConfig").Get<ServerConstants>();
            var editorSettings = config.GetSection("Editor").Get<EditorOptions>();
            using var serviceProvider = new ServiceCollection()
                .Configure<LoruleOptions>(config.GetSection("Content"))
                .AddOptions()
                .AddLogging(l => l.AddConsole())
                .AddSingleton(_ => editorSettings)
                .AddSingleton<IServerConstants, ServerConstants>(_ => constants)
                .AddSingleton<IServerContext, ServerContext>()
                .AddSingleton<IObjectManager, ObjectManager>()
                .AddScoped<AreaCreateWizard>()
                .BuildServiceProvider();

            var frm = serviceProvider.GetService<AreaCreateWizard>();
            if (frm != null) Application.Run(frm);
        }
    }
}