using Darkages;
using System;
using System.Threading;

namespace LoginServer
{
    class Program
    {
        static LoginServer _server;

        static void Main(string[] args)
        {
            ServerContext.LoadConstants();


            if (ServerContext.Config != null)
            {
                ServerContext.Info?.Info("Lorule - Login Server: Online");
                ServerContext.Info?.Info("---------------------------------------");
                ServerContext.Info?.Info("Listening...");

                _server = new LoginServer(1000);
                _server.Start(2610);
            }

            ServerContext.LoadAndCacheStorage();
            Thread.CurrentThread.Join();
        }
    }
}
