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
using Darkages.Interops;
using Darkages.Storage;
using Darkages.Types;
using MemoryMappedFileManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lorule
{
    class Program
    {
        public static Instance _Server;

        static void Main(string[] args)
        {

            _Server = new Instance();        
            _Server.Start();
            _Server.Report();
        }

        public class Instance : ServerContext
        {
            readonly DateTime SystemStartTime = DateTime.Now;

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
                        Thread.Sleep(15000);
                    }
                });

                Thread.CurrentThread.Join();
            }

            private void SendServerInfo(MemoryMappedFileCommunicator communicator)
            {
                if (Info == null)
                {
                    Info = new ServerInformation
                    {
                        ServerConfig = Config,
                        MonsterTemplates = new List<MonsterTemplate>(GlobalMonsterTemplateCache),
                        ItemTemplates = new List<ItemTemplate>(GlobalItemTemplateCache.Select(i => i.Value)),
                        SkillTemplates = new List<SkillTemplate>(GlobalSkillTemplateCache.Select(i => i.Value)),
                        SpellTemplates = new List<SpellTemplate>(GlobalSpellTemplateCache.Select(i => i.Value)),
                        MundaneTemplates = new List<MundaneTemplate>(GlobalMundaneTemplateCache.Select(i => i.Value)),
                        WarpTemplates = new List<WarpTemplate>(GlobalWarpTemplateCache),
                        Areas = new List<Area>(GlobalMapCache.Select(i => i.Value)),
                        Buffs = new List<Buff>(GlobalBuffCache.Select(i => i.Value)),
                        Debuffs = new List<Debuff>(GlobalDeBuffCache.Select(i => i.Value)),
                    };
                }

                Info.GameServerOnline   = true;
                Info.LoginServerOnline  = true;

                var players_online      = Game?.Clients.Where(i => i != null && i.Aisling != null && i.Aisling.LoggedIn);

                if (players_online != null)
                {
                    Info.PlayersOnline    = new List<Aisling>(players_online.Select(i => i.Aisling));
                    Info.GameServerStatus = $"Up time {Math.Round(Uptime.TotalDays, 2)}:{Math.Round(Uptime.TotalHours, 2)} | Online Users ({ players_online.Count() }) | Total Characters ({ StorageManager.AislingBucket.Count })";
                    Info.GameServerOnline = true;
                }
                else
                {
                    Info.PlayersOnline    = new List<Aisling>();
                    Info.GameServerOnline = false;
                    Info.GameServerStatus = "Offline.";
                }

                lock (communicator)
                {
                    var jsonWrap = JsonConvert.SerializeObject(Info, StorageManager.Settings);
                    communicator.Write(jsonWrap);
                }
            }


            public void Reboot(Instance instance)
            {
                instance.Shutdown();
                instance.Start();            
            }
        }
    }
}
