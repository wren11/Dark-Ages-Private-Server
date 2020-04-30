///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Types;

namespace Darkages.Network.Game.Components
{
    public class MonolithComponent : GameServerComponent
    {
        public object SyncObj = new object();

        private readonly GameServerTimer _timer;

        public Dictionary<int, Spawn> Spawns = new Dictionary<int, Spawn>();

        public bool Updating { get; set; } = true;

        public MonolithComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContextBase.GlobalConfig.GlobalSpawnTimer));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Updating)
            {
                _timer.Update(elapsedTime);

                if (_timer.Elapsed)
                {
                    _timer.Reset();

                    var templates = ServerContextBase.GlobalMonsterTemplateCache;
                    if (templates.Count == 0)
                        return;

                    foreach (var map in ServerContextBase.GlobalMapCache.Values)
                    {
                        if (map == null || map.Rows == 0 || map.Cols == 0)
                            return;

                        var temps = templates.Where(i => i.AreaID == map.ID);


                        foreach (var template in temps)
                        {
                            var count = GetObjects<Monster>(map, i =>
                                i.Template != null && i.Template.Name == template.Name
                                                   && i.Template.AreaID == map.ID).Count();

                            if (template.ReadyToSpawn())
                            {
                                var spawn = new Spawn
                                {
                                    Capacity = template.SpawnMax,
                                    TotalSpawned = 0,
                                    LastSpawned = DateTime.UtcNow
                                };

                                if (count < template.SpawnMax) CreateFromTemplate(template, map);
                            }
                        }
                    }
                }
            }
        }

        public void CreateFromTemplate(MonsterTemplate template, Area map)
        {
            var newObj = Monster.Create(template, map);

            if (newObj != null) AddObject(newObj);
        }

        public class Spawn
        {
            public DateTime LastSpawned { get; set; }


            public int Capacity { get; set; }
            public int TotalSpawned { get; set; }
        }
    }
}