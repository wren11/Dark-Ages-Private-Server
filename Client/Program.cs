using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    class Program
    {

        static void Main(string[] args)
        {

            for (int i = 0; i < 6; i++)
            { 
                var _client = new Client("bot", "bot");
                _client.Connect("54.213.128.251", 2610);

                Thread.Sleep(2000);
            }

            Thread.CurrentThread.Join();
        }
    }
}
