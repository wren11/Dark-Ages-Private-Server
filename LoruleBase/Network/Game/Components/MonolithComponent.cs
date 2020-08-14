#region

using Darkages.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Darkages.Network.Game.Components
{
    public class MonolithComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public MonolithComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContextBase.Config.GlobalSpawnTimer));
        }

        public override UpdateType UpdateMethodType => UpdateType.Async;

        public void CreateFromTemplate(MonsterTemplate template, Area map)
        {
            var newObj = Monster.Create(template, map);

            if (newObj != null)
                AddObject(newObj);
        }

        public override Task Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                Lorule.Update(ManageSpawns);
                _timer.Reset();

            }

            return Task.CompletedTask;
        }

        private void ManageSpawns()
        {
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

                    if (!template.ReadyToSpawn())
                        continue;

                    if (count < template.SpawnMax)
                        CreateFromTemplate(template, map);
                }
            }
        }
    }
}