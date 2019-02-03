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
                Console.WriteLine("Lorule - Login Server: Online");
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Listening...");

                _server = new LoginServer(1000);
                _server.Start(2610);
            }

            ServerContext.LoadAndCacheStorage();
            Thread.CurrentThread.Join();
        }
    }
}
