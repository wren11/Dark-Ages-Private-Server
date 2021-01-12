#region

using Darkages;
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
    public interface IServer 
    {
        
    }

    public static class Program
    {
        private static void Main()
        {
            var providers = new LoggerProviderCollection();
            var logTemplate = "[{Level:u}] {Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), "Hades_General.txt")
                .WriteTo.File(new CompactJsonFormatter(), "Hades_Exceptions.txt", Serilog.Events.LogEventLevel.Error)
                .WriteTo.File(new CompactJsonFormatter(), "Hades_CrashLogs.txt", Serilog.Events.LogEventLevel.Fatal)
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
                .AddSingleton<IServer, Server>()
                .BuildServiceProvider();

            serviceProvider.GetService<IServer>();
            Thread.CurrentThread.Join();
        }
    }

    public class Server : IServer
    {
        private readonly ILogger<ServerContext> _logger;

        public Server(ILogger<ServerContext> logger, IServerContext context, IServerConstants configConstants,
            IOptions<LoruleOptions> loruleOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (loruleOptions.Value.Location == null)
                return;

            context.InitFromConfig(loruleOptions.Value.Location, loruleOptions.Value.ServerIP);
            _logger.LogInformation($"{configConstants.SERVER_TITLE}: Server Version: {LoruleVersion}.");
            context.Start(configConstants, logger);


            var batContents = new StringBuilder();
            batContents.AppendLine("@echo off");
            batContents.AppendLine("set PATH=\"net5.0\"");
            batContents.AppendLine("copy %PATH%\\\\LoruleConfig.json . >NUL"); 
            batContents.AppendLine("copy %PATH%\\\\MServerTable.xml . >NUL");
            batContents.AppendLine("copy %PATH%\\\\Notification.txt . >NUL");
            batContents.AppendLine("%PATH%\\\\Lorule.GameServer.exe");


            File.WriteAllText("..\\start_server.bat", batContents.ToString());
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