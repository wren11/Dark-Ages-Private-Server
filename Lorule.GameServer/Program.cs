using Darkages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Lorule.GameServer
{
    public static class Program
    {

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("ServerConfig.Local.json");



           var config    = builder.Build();
           var constants = config.GetSection("ServerConfig").Get<ServerConstants>();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<LoruleOptions>(config.GetSection("Content"))
                .AddSingleton<IServerConstants, ServerConstants>(_ => constants)
                .AddSingleton<IServer, Server>()
                .BuildServiceProvider();

            serviceProvider.GetService<IServer>().Start();
            Thread.CurrentThread.Join();
        }
    }

    public class Server : ServerContext, IServer
    {
        public Server(IServerConstants constants, IOptions<LoruleOptions> loruleOptions)
        {
            GlobalConfig = constants ?? throw new ArgumentNullException(nameof(constants));

            Debug("Loading Config: {0}", loruleOptions.Value.Location);

            if (loruleOptions.Value.Location != null)
                InitFromConfig(storagePath: loruleOptions.Value.Location);
        }
    }
}
