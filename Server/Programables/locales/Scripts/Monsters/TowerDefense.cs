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
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("Tower Defense", "Dean")]
    public class TowerDefense : MonsterScript
    {
        public TowerDefense(Monster monster, Area map)
            : base(monster, map)
        {
            Monster.Template.SpawnSize  = 100;
            Monster.Template.SpawnMax   = 100;
            Monster.Template.SpawnRate  = 1;
        }

        public override void OnApproach(GameClient client)
        {
            Monster.Target = client.Aisling;
        }

        public override void OnAttacked(GameClient client)
        {

        }

        public override void OnCast(GameClient client)
        {

        }

        public override void OnClick(GameClient client)
        {

        }

        public override void OnDeath(GameClient client)
        {
            var remaining = GetObjects<Monster>(i => i.CurrentMapId == client.Aisling.AreaID
                && i.Template.Name == Monster.Template.Name).Count();

            if (remaining <= 1)
            {

                var temp = Monster.Template;
                temp.Image += 2;
                temp.MovementSpeed -= 50;
                temp.MaximumHP *= 2;
                temp.SpawnMax+=2;
                temp.SpawnRate--;
                temp.SpawnSize++;

                if (temp.MovementSpeed <= 50)
                    temp.MovementSpeed = 50;

                Monster.Template = temp;

                client.SendMessage(0x02, string.Format("[Difficulty: {0}] Creeps get stronger ...", temp.Level));
            }

            if (GetObject<Monster>(i => i.Serial == Monster.Serial) != null)
                DelObject(Monster);
        }

        public override void OnLeave(GameClient client)
        {
            Monster.Target = null;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!Monster.IsAlive)
                return;

            Monster.WalkTimer.Update(elapsedTime);

            if (Monster.WalkTimer.Elapsed)
            {
                Monster.WalkTimer.Reset();

                if (Monster.WalkEnabled)
                {
                    Walk(true);
                }
            }
        }

        private void Walk(bool ignoreWalls = false)
        {
            if (!Monster.CanMove)
                return;


            if (Monster.Template.Waypoints.Count > 0)
                Monster.Patrol(ignoreWalls);

            if (Monster.CurrentMapId == 510 && Monster.Position.DistanceFrom(new Position(0, 12)) <= 1)
            {
                Monster.CurrentHp = 0;

                if (Monster.Target != null)
                {
                    Monster.Target.ApplyDamage(Monster, Monster.Target.MaximumHp / 10, true, 88);
                }
            }
        }
    }
}
