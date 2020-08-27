using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DAClient
{
    internal class Program
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void Main(string[] args)
        {
            for (var i = 0; i < 500; i++)
            {
                try
                {
                    var username = RandomString(10);
                    var client = new Client(username, username);

                    client.Connect("174.87.11.79", 2610);
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                }

            }

            Thread.CurrentThread.Join();
        }
    }
}