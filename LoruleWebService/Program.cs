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

        public static readonly string ContextPath = Path.Combine(Environment.CurrentDirectory, "WebContext.json");

        public static ReaderWriterLock _lock = new ReaderWriterLock();

        const string ServiceAPI = "http://localhost:2620/";

        static void Main(string[] args)
        {
            communicator.ReadPosition = 0;
            communicator.WritePosition = 0;

            communicator.DataReceived += new EventHandler<MemoryMappedDataReceivedEventArgs>(Communicator_DataReceived);
            communicator.StartReader();

            var t = new Task(() =>
            {
                Web = new WebServer(WebServer.Lorule, ServiceAPI);
                Web.Run();
                {
                    try
                    {
                        if (File.Exists(ContextPath))
                        {
                           WebServer.Info = JsonConvert.DeserializeObject<ServerInformation>(File.ReadAllText(ContextPath));
                           WebServer.Info.Debug("Web Info Recovered.");
                        }
                    }
                    finally
                    {
                    }
                }
            });

            t.Start();

            Process.Start(ServiceAPI);

            Thread.CurrentThread.Join();
        }

        public static string Lorule(HttpListenerRequest arg)
        {
            var path = Path.GetFullPath($"{Environment.CurrentDirectory}\\services\\www\\http\\index.html");
            return File.ReadAllText(path);
        }

        private static void Communicator_DataReceived(object sender, MemoryMappedDataReceivedEventArgs e)
        {
            _lock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                var data = ASCIIEncoding.UTF8.GetString(e.Data);
                {
                    File.WriteAllText(ContextPath, data, Encoding.GetEncoding(949));
                }

                var obj = JsonConvert.DeserializeObject<ServerInformation>(data, StorageManager.Settings);

                if (obj != null)
                {
                    WebServer.Info = obj;
                    WebServer.Info.Debug("Web Info Updated.");
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }
    }
}
