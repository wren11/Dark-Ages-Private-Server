using Darkages.Interops;
using Darkages.Services.www;
using Darkages.Storage;
using MemoryMappedFileManager;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoruleWebService
{
    class Program
    {
        public static MemoryMappedFileCommunicator communicator = new MemoryMappedFileCommunicator("lorule", 10485760);
        public static WebServer Web { get; set; }

        const string ServiceAPI = "http://localhost:2620/";

        static void Main(string[] args)
        {
            communicator.ReadPosition = 0;
            communicator.WritePosition = 0;

            communicator.DataReceived += new EventHandler<MemoryMappedDataReceivedEventArgs>(communicator_DataReceived);
            communicator.StartReader();

            Task.Run(() =>
            {
                Web = new WebServer(WebServer.Lorule, ServiceAPI);
                Web.Run();
                {
                    Process.Start(ServiceAPI);
                }
            });

            Thread.CurrentThread.Join();
        }

        public static string Lorule(HttpListenerRequest arg)
        {
            var path = Path.GetFullPath($"{Environment.CurrentDirectory}\\services\\www\\http\\index.html");
            return File.ReadAllText(path);
        }

        private static void communicator_DataReceived(object sender, MemoryMappedDataReceivedEventArgs e)
        {


            var data = ASCIIEncoding.UTF8.GetString(e.Data);
            var obj  = JsonConvert.DeserializeObject<ServerInformation>(data, StorageManager.Settings);

            if (obj != null)
            {
                Console.WriteLine("Server Info Received.");
                Web.Info = obj;
            }
        }
    }
}
