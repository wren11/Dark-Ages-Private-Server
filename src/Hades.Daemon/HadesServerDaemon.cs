using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hades.Daemon
{
    public class HadesServerDaemon
    {
        private Process _gameServerProcess;

        public HadesServerDaemon()
        {
            Console.WriteLine("Daemon Started.");
            var gameServerCheckThread = new Thread(CheckGameServer) {IsBackground = true};
            gameServerCheckThread.Start(); 
            Thread.CurrentThread.Join();
        }

        async Task Subscribe()
        {
            var client = new ClientWebSocket();
            var buffer = new byte[short.MaxValue];
            var msg = "{\"method\":\"SUBSCRIBE\"}";

            try
            {
                await client.ConnectAsync(new Uri("ws://localhost:2620"), default);
                await client.SendAsync(Encoding.ASCII.GetBytes(msg), WebSocketMessageType.Text, true, default);

                while (true)
                {
                    try
                    {

                        var response = await client.ReceiveAsync(buffer, CancellationToken.None);

                        unsafe
                        {
                            fixed (byte* p = &buffer[0])
                            {
                                var message = Encoding.ASCII.GetString(p, response.Count);
                                Console.WriteLine(message);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void GameServerProcessExited(object sender, EventArgs e)
        {
            Console.WriteLine("Server Crashed, Rebooting Game Server...");
        }


        private void CheckGameServer()
        {
            var exePath = "Lorule.GameServer.exe";

            if (!File.Exists(exePath))
            {
                return;
            }


            do
            {
                try
                {
                    Console.WriteLine("Starting Game Servers...");
                    _gameServerProcess = Process.Start(exePath);
                    _gameServerProcess.EnableRaisingEvents = true;
                    _gameServerProcess.Exited += GameServerProcessExited;

                    try
                    {
                        _ = Task.Run(async () => await Subscribe());
                    }
                    finally
                    {
                        _gameServerProcess.WaitForExit();
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    Thread.Sleep(1000);
                }

            } while (true);
        }
    }
}
