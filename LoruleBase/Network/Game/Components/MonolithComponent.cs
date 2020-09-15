#region

using System;
using System.Linq;
using Darkages.Types;

#endregion

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

        public void CreateFromTemplate(MonsterTemplate template, Area map)
        {
            var newObj = Monster.Create(template, map);

            if (newObj != null)
                AddObject(newObj);
        }

        protected internal override void Update(TimeSpan elapsedTime)
        {
            if (_timer.Update(elapsedTime))
                Lorule.Update(ManageSpawns);
        }

        private void ManageSpawns()
        {
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
                    var count = GetObjects<Monster>(map, i =>
                        i.Template != null && i.Template.Name == template.Name
                                           && i.Template.AreaID == map.ID).Count();

                    if (!template.ReadyToSpawn())
                        continue;

                    if (count < template.SpawnMax)
                        if (count < map.Rows * map.Cols / 6)
                            CreateFromTemplate(template, map);
                }
            }
        }
    }
}