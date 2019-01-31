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
using System.Threading.Tasks;

namespace Darkages.Network.Game.Components
{
    public class MundaneComponent : GameServerComponent
    {
        public MundaneComponent(GameServer server) : base(server)
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(ServerContext.Config.MundaneRespawnInterval));
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
            foreach (var mundane in ServerContext.GlobalMundaneTemplateCache)
            {
                if (mundane.Value == null || mundane.Value.AreaID == 0)
                    continue;

                if (!ServerContext.GlobalMapCache.ContainsKey(mundane.Value.AreaID))
                    continue;

                var map = ServerContext.GlobalMapCache[mundane.Value.AreaID];

                if (map == null || !map.Ready)
                    continue;

                var npc = GetObject<Mundane>(map, i => i.CurrentMapId == map.ID && i.Template != null
                                                                           && i.Template.Name == mundane.Value.Name);

                if (npc != null && npc.CurrentHp > 0)
                    continue;

                Mundane.Create(mundane.Value);
            }
        }
    }
}
