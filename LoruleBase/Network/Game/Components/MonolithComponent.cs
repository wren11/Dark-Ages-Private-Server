#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game.Components
{
    public class MonolithComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public Dictionary<int, Spawn> Spawns = new Dictionary<int, Spawn>();
        public object SyncObj = new object();

        public MonolithComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContextBase.Config.GlobalSpawnTimer));
        }

        public bool Updating { get; set; } = true;

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