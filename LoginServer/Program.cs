using Darkages;
using System;
using System.Threading;
using System.Threading.Tasks;

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

                Task.Run(() => UpdateClients());
            }

            ServerContext.LoadAndCacheStorage();
            Thread.CurrentThread.Join();
        }

        private static void UpdateClients()
        {
            while (_server != null)
            {
                lock (_server.Clients)
                {
                    foreach (var client in _server.Clients)
                    {
                        if (client != null && client.WorkSocket.Connected)
                        {
                            client.Update();
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
