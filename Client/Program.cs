using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    class Program
    {

        static void Main(string[] args)
        {

            for (int i = 0; i < 5; i++)
            { 
                var _client = new Client("r", "123");
                //_client.Connect("127.0.0.1", 2610);
                _client.Connect("174.87.11.79", 2610);


                Thread.Sleep(3000);
            }

            Thread.CurrentThread.Join();
        }
    }
}
