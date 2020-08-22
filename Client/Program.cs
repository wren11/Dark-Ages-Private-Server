using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new Client("lol", "lol");
            client.Connect("174.87.11.79", 2610);

            Thread.CurrentThread.Join();
        }
    }
}