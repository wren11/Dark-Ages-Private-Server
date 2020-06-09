using System;
using Darkages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Options;

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

            if (loruleOptions.Value.Location != null)
                InitFromConfig(storagePath: loruleOptions.Value.Location);
        }
    }
}
