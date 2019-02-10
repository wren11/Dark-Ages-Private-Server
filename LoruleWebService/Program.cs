using Darkages.Interops;
using Darkages.Storage;
using MemoryMappedFileManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoruleWebService
{
    class Program
    {
        static MemoryMappedFileCommunicator communicator = new MemoryMappedFileCommunicator("lorule", 10485760);

        static void Main(string[] args)
        {
            communicator.ReadPosition  = 0;
            communicator.WritePosition = 0;

            communicator.DataReceived += new EventHandler<MemoryMappedDataReceivedEventArgs>(communicator_DataReceived);
            communicator.StartReader();

            Thread.CurrentThread.Join();
        }

        private static void communicator_DataReceived(object sender, MemoryMappedDataReceivedEventArgs e)
        {
            var data = ASCIIEncoding.UTF8.GetString(e.Data);
            var obj  = JsonConvert.DeserializeObject<ServerInformation>(data, StorageManager.Settings);
        }
    }
}
