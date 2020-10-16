using Lorule.Editor.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Darkages;
using Darkages.Network.Object;
using Lorule.Editor.Controls;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;


namespace Lorule.Editor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var providers = new LoggerProviderCollection();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), "Editor_logs.txt")
                .CreateLogger();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("LoruleConfig.json");

            var config = builder.Build(); 
            var constants = config.GetSection("ServerConfig").Get<ServerConstants>();
            var editorSettings = config.GetSection("Editor").Get<EditorSettings>();
            using (var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddSingleton(providers)
                .AddSingleton<ILoggerFactory>(sc =>
                {
                    var providerCollection = sc.GetService<LoggerProviderCollection>();
                    var factory = new SerilogLoggerFactory(null, true, providerCollection);

                    foreach (var provider in sc.GetServices<ILoggerProvider>())
                        factory.AddProvider(provider);

                    return factory;
                })
                .AddLogging(l => l.AddConsole())
                .AddSingleton<EditorSettings>(_ => editorSettings)
                .AddSingleton<LoadingIndicatorView>()
                .AddSingleton<IServerConstants, ServerConstants>(_ => constants)
                .AddSingleton<IServerContext, ServerContext>()
                .AddSingleton<IObjectManager, ObjectManager>()
                .AddScoped<FrmMain>()
                .AddScoped<Form, MapView>()
                .AddScoped<IMapEditor, MapEditor>()
                .BuildServiceProvider())
            {

                var frm = serviceProvider.GetService<FrmMain>();
                if (frm != null) Application.Run(frm);
            }
        }
    }
}
