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

namespace Darkages.Network.Game.Components
{
    public class MonolithComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public MonolithComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContext.Config.GlobalSpawnTimer));
        }

        public override void Update(TimeSpan elapsedTime)
        {
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

                    var temps = templates.Where(i => i.AreaID == map.ID);

                    foreach (var template in temps)
                    {
                        if (template.ReadyToSpawn())
                        {
                            var spawn = new Spawn
                            {
                                 Capacity = template.SpawnMax,
                                 TotalSpawned = 0,
                                 LastSpawned = DateTime.UtcNow,
                            };

                            if (template.SpawnCount < template.SpawnMax)
                            {
                                CreateFromTemplate(template, map);
                                template.SpawnCount++;
                            }
                        }
                    }
                }
            }
        }

        public void CreateFromTemplate(MonsterTemplate template, Area map)
        {
            var newObj = Monster.Create(template, map);

            if (newObj != null)
            {
                AddObject(newObj);
            }
        }

        public Dictionary<int, Spawn> _spawns = new Dictionary<int, Spawn>();

        public class Spawn
        {
            public DateTime LastSpawned { get; set; }


            public int Capacity { get; set; }
            public int TotalSpawned { get; set; }          
        }
    }
}

