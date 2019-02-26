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
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Types;
using System;

namespace Darkages.Scripting
{
    public abstract class MonsterScript : ObjectManager
    {
        public MonsterScript(Monster monster, Area map)
        {
            Monster = monster;
            Map = map;
        }

        public Monster Monster;
        public Area Map;

        public abstract void OnApproach(GameClient client);
        public abstract void OnAttacked(GameClient client);
        public abstract void OnCast(GameClient client);
        public abstract void OnClick(GameClient client);
        public abstract void OnDeath(GameClient client);
        public abstract void OnLeave(GameClient client);
        public abstract void Update(TimeSpan elapsedTime);

        public virtual void OnDamaged(GameClient client, int dmg) { }

        public abstract void OnSkulled(GameClient client);
    }
}
