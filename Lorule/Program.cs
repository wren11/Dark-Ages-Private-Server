///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages;
using Darkages.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lorule
{
    class Program
    {
        public static Instance _Server;

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _Server = new Instance();        
            _Server.Start();
            _Server.Report();
        }

        static int r = 0;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Interlocked.Increment(ref r);
            Console.WriteLine("unknown error handled. {0}", r);

            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = string.Format("Error {0}", r);

            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
        }

        public class Instance : ServerContext
        {
            DateTime SystemStartTime = DateTime.Now;
            TimeSpan Uptime => (DateTime.Now - SystemStartTime);

            public Instance()
            {
                LoadConstants();
            }

            public bool IsRunning => Running;

            public void Report()
            {

                new TaskFactory().StartNew(() =>
                {
                    while (Running)
                    {
                        Console.Title = $"Lorule - Server Uptime {Math.Round(Uptime.TotalDays, 2)}:{Math.Round(Uptime.TotalHours, 2)} - { _Server.GetObjects<Aisling>(i => i.LoggedIn).Count()} Players Online | Total Characters ({ StorageManager.AislingBucket.Count })";
                        Thread.Sleep(5000);
                    }
                });

                Application.Run(new Controller());

            }

            public void Reboot(Instance instance)
            {
                instance.Shutdown();
                instance.Start();            
            }
        }
    }
}
