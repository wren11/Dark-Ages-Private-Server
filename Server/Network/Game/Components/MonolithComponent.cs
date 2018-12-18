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

using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Network.Game.Components
{
    public class MonolithComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public readonly Queue<Spawn> SpawnQueue = new Queue<Spawn>();

        public MonolithComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContext.Config.GlobalSpawnTimer));

            var spawnThread = new Thread(SpawnEmitter) { IsBackground = true };
            spawnThread.Start();
        }

        private void SpawnEmitter()
        {
            while (true)
            {
                if (!ServerContext.Paused)
                {
                    if (SpawnQueue.Count > 0)
                        ConsumeSpawns();
                }

                Thread.Sleep(50);
            }
        }

        private void ConsumeSpawns()
        {
            if (ServerContext.Paused)
                return;

            try
            {
                Spawn spawn;

                lock (SpawnQueue)
                {
                    spawn = SpawnQueue.Dequeue();
                }

                if (spawn != null)
                {
                    SpawnOn(spawn.Template, spawn.Map);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused)
                return;


            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                _timer.Reset();

                var templates = ServerContext.GlobalMonsterTemplateCache;
                if (templates.Count == 0)
                    return;

                foreach (var map in ServerContext.GlobalMapCache.Values)
                {
                    if (map == null || map.Rows == 0 || map.Cols == 0)
                        return;

                    lock (templates)
                    {
                        var temps = templates.Where(i => i.AreaID == map.ID);
                        foreach (var template in temps)
                        {
                            if (template.ReadyToSpawn())
                            {
                                var spawn = new Spawn
                                {
                                    Template = template,
                                    Map = map
                                };

                                lock (SpawnQueue)
                                {
                                    SpawnQueue.Enqueue(spawn);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool SpawnOn(MonsterTemplate template, Area map)
        {
            var count = GetObjects<Monster>(i => i.Template.Name == template.Name && i.CurrentMapId == map.ID).Count();

            if (count < Math.Abs(template.SpawnMax))
            {
                CreateFromTemplate(template, map, template.SpawnSize);
                return true;
                     
            }

            return false;
        }


        public async void CreateFromTemplate(MonsterTemplate template, Area map, int count)
        {
            var objectsAdded = 0;

            try
            {

                for (int i = 0; i < count; i++)
                {
                    var newObj = await Monster.Create(template, map);

                    if (newObj != null)
                    {
                        AddObject(newObj);
                        objectsAdded++;
                    }
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public class Spawn
        {
            public DateTime LastSpawned { get; set; }
            public MonsterTemplate Template { get; set; }
            public Area Map { get; set; }
        }
    }
}

