using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    class Program
    {

        static void Main(string[] args)
        {

            for (int i = 0; i < 2500; i++)
            { 
                var _client = new Client("lol", "lol");
                //_client.Connect("127.0.0.1", 2610);
                _client.Connect("174.87.2.164", 2610);

                Thread.Sleep(300);
            }

            Thread.CurrentThread.Join();
        }
    }
}
