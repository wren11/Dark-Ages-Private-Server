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
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lorule
{
    class Program
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();
        public static Instance _Server;

        static void Main(string[] args)
        {
            InitLogger();

#if ISDEAN
            DisplayEnumReferences();
#endif
            _Server = new Instance();
            _Server.Start();
            _Server.Report();
        }

        private static void DisplayEnumReferences()
        {
            foreach (var asm in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                if (asm.FullName.Contains("Darkages"))
                {
                    var query = Assembly.Load(asm.FullName)
                            .GetTypes()
                            .Where(t => t.IsEnum && t.IsPublic);


                    foreach (Type t in query)
                    {
                        logger.Debug(t.Name);

                        var type = Enum.GetValues(t);
                        var names = Enum.GetNames(t);

                        var idx = 0;
                        var pair = new Dictionary<string, int>();

                        foreach (var obj in type)
                        {
                            var jsonobj = JsonConvert.SerializeObject(obj, Formatting.Indented);
                            var key = names[idx];

                            pair[key] = Convert.ToInt32(jsonobj);

                            logger.Trace(key + ":" + jsonobj);

                            idx++;
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        private static void InitLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();

            config.AddTarget("console", consoleTarget);

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${message}";
            fileTarget.FileName  = "${basedir}/serverlog.txt";
            fileTarget.Layout    = "${message}";

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, fileTarget));

            LogManager.Configuration = config;
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
                        Console.Title = $"Lorule - Server Uptime {Math.Round(Uptime.TotalDays, 2)}:{Math.Round(Uptime.TotalHours, 2)} - { _Server.GetObjects<Aisling>(i => i.LoggedIn).Length} Players Online | Total Characters ({ StorageManager.AislingBucket.Count })";
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
