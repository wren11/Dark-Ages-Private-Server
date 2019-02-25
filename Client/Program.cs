using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    class Program
    {

        static void Main(string[] args)
        {

            for (int i = 0; i < 250; i++)
            { 
                var _client = new Client("lol", "lol");
              //  _client.Connect("127.0.0.1", 2610);
                _client.Connect("54.213.128.251", 2610);

                Thread.Sleep(1000);
            }

            Thread.CurrentThread.Join();
        }
    }
}
