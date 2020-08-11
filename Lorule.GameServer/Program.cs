#region

using Darkages;
using Darkages.Network.Object;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

#endregion

namespace Lorule.GameServer
{
    //updated again
    public interface IServer
    {
        void Log(string logMessage);
    }

    public static class Program
    {
        private static void Main()
        {
            var providers = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), "lorule_logs.txt")
                .WriteTo.Console()
                .CreateLogger();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("ServerConfig.Local.json");

            var config = builder.Build();
            var constants = config.GetSection("ServerConfig").Get<ServerConstants>();

            var serviceProvider = new ServiceCollection()
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
                .Configure<LoruleOptions>(config.GetSection("Content"))
                .AddSingleton<IServerConstants, ServerConstants>(_ => constants)
                .AddSingleton<IServerContext, ServerContext>()
                .AddSingleton<IObjectManager, ObjectManager>()
                .AddSingleton<IServer, Server>()
                .BuildServiceProvider();

            serviceProvider.GetService<IServer>();
            Thread.CurrentThread.Join();
        }
    }

    public class Server : IServer
    {
        private readonly ILogger<Server> _logger;

        public Server(ILogger<Server> logger, IServerContext context, IServerConstants configConstants,
            IOptions<LoruleOptions> loruleOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (loruleOptions.Value.Location == null)
                return;

            context.InitFromConfig(loruleOptions.Value.Location);
            _logger.LogInformation($"{configConstants.SERVER_TITLE}: Server Version: {LoruleVersion}.");
            context.Start(configConstants, Log, Error);
        }

        public string LoruleVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void Error(Exception ex)
        {
            _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);
        }

        public void Log(string logMessage)
        {
            _logger.LogInformation(logMessage);
        }
    }
}