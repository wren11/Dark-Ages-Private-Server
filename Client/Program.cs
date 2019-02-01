using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    class Program
    {

        static void Main(string[] args)
        {

            for (int i = 0; i < 999; i++)
            { 
                var _client = new Client("bot", "bot");
                _client.Connect("127.0.0.1", 2610);

                Thread.Sleep(2000);
            }

            Thread.CurrentThread.Join();
        }
    }
}
