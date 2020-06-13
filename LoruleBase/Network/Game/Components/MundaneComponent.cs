#region

using System;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game.Components
{
    public class MundaneComponent : GameServerComponent
    {
        public MundaneComponent(GameServer server) : base(server)
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(ServerContextBase.Config.MundaneRespawnInterval));
        }

        public GameServerTimer Timer { get; set; }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                SpawnMundanes();
                Timer.Reset();
            }
        }

        public void SpawnMundanes()
        {
            foreach (var mundane in ServerContextBase.GlobalMundaneTemplateCache)
            {
                if (mundane.Value == null || mundane.Value.AreaID == 0)
                    continue;

                if (!ServerContextBase.GlobalMapCache.ContainsKey(mundane.Value.AreaID))
                    continue;

                var map = ServerContextBase.GlobalMapCache[mundane.Value.AreaID];

                if (map == null || !map.Ready)
                    continue;

                var npc = GetObject<Mundane>(map, i => i.CurrentMapId == map.ID && i.Template != null
                                                                                && i.Template.Name ==
                                                                                mundane.Value.Name);

                if (npc != null && npc.CurrentHp > 0)
                    continue;

                Mundane.Create(mundane.Value);
            }
        }
    }
}