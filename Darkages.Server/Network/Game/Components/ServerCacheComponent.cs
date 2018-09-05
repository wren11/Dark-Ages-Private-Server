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
using Darkages.Interops;
using Darkages.Services;
using Darkages.Storage;
using Darkages.Types;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Darkages.Network.Game.Components
{
    public class ServerCacheComponent : GameServerComponent
    {
        readonly IService<ServerStatistics> CollectionService = new ServerInfoService();

        GameServerTimer Timer;
        GameServerTimer CollectionTimer;
        GameServerTimer CollectionServiceTimer;

        ServerStatistics CollectedMeta = new ServerStatistics();

        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        public ServerCacheComponent(GameServer server) : base(server)
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(45));

            if (ServerContext.Config.UsingDatabase)
            {
                ResetCounters();

                CollectionTimer = new GameServerTimer(TimeSpan.FromSeconds(10));
                CollectionServiceTimer = new GameServerTimer(TimeSpan.FromSeconds(5));
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (ServerContext.Config.UsingDatabase)
            {
                if (string.IsNullOrEmpty(ServerContext.Config.SERVER_TITLE))
                    return;

                CollectionTimer.Update(elapsedTime);
                CollectionServiceTimer.Update(elapsedTime);

                if (CollectionTimer.Elapsed)
                {
                    Task.Run(() => RunServices());
                    CollectionTimer.Reset();
                }
                else
                {
                    if (CollectionServiceTimer.Elapsed)
                    {
                        Task.Run(() => CollectData());
                        CollectionServiceTimer.Reset();
                    }
                }
            }

            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                ServerContext.SaveCommunityAssets();
                Timer.Reset();
            }
        }

        private bool CollectData()
        {
            CollectedMeta = CollectedMeta ?? new ServerStatistics();
            {
                CollectedMeta.LastCollected = DateTime.UtcNow;
                CollectedMeta.CollectionInqueries++;
            }

            if (CollectedMeta != null)
            {
                var objCache = GetObjects(i => true, Get.All);
                {
                    CollectedMeta.Items = objCache.OfType<Item>().Count();
                    CollectedMeta.Aislings = objCache.OfType<Aisling>().Count();
                    CollectedMeta.Mundanes = objCache.OfType<Mundane>().Count();
                    CollectedMeta.Monsters = objCache.OfType<Monster>().Count();
                    CollectedMeta.ServerName = ServerContext.Config.SERVER_TITLE;
                    CollectedMeta.Version = ServerContext.Config.Version.ToString();
                    CollectedMeta.EndPoint = ServerContext.Ipaddress.ToString();
                    CollectedMeta.WelcomeMessage = ServerContext.GlobalMessage;
                    CollectedMeta.Accounts = StorageManager.AislingBucket.Count;
                    CollectedMeta.Maps = StorageManager.AreaBucket.Count;

                    CollectedMeta.CPU    = getCurrentCpuUsage();
                    CollectedMeta.Memory = getAvailableRAM();
                }
                return true;
            }
            return false;
        }

        public string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue() + "%";
        }

        public string getAvailableRAM()
        {
            return ramCounter.NextValue() + "MB";
        }

        private bool RunServices()
        {
            if (CollectedMeta != null)
            {
                CollectionService.AddOrUpdate(CollectedMeta);
            }

            ResetCollector();
            return true;
        }

        private void ResetCollector()
        {
            ResetCounters();

            CollectedMeta = null;
        }

        private void ResetCounters()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
    }
}
