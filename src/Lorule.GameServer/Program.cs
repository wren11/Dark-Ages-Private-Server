#region

using Darkages;
using Darkages.Network.Object;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using ServiceStack;

#endregion

namespace Lorule.GameServer {

    public interface IServer 
    {
        
    }

    public static class Program
    {


        private static void Main(string[] args)
        {
            var providers = new LoggerProviderCollection();
            var logTemplate = "[{Level:u}] {Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Hades_General.txt")
                .WriteTo.File("Hades_Exceptions.txt", Serilog.Events.LogEventLevel.Error)
                .WriteTo.File("Hades_CrashLogs.txt", Serilog.Events.LogEventLevel.Fatal)
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose, outputTemplate: logTemplate)
                .CreateLogger();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("LoruleConfig.json");

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
        private readonly ILogger<ServerContext> _logger;
        private readonly IObjectManager _objectManager;

        public Server(ILogger<ServerContext> logger, IServerContext context, IServerConstants configConstants, Microsoft.Extensions.Options.IOptions<LoruleOptions> loruleOptions, IObjectManager objectManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectManager = objectManager;

            if (loruleOptions.Value.Location == null)
                return;

            context.InitFromConfig(loruleOptions.Value.Location, loruleOptions.Value.ServerIP);
            _logger.LogInformation($"{configConstants.SERVER_TITLE}: Server Version: {LoruleVersion}.");
            context.Start(configConstants, logger);
        }

        internal string LoruleVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                    return version.ToString();

                return string.Empty;
            }
        }
    }
}