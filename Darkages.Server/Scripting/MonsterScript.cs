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

        public Monster Monster { get; set; }
        public Area Map { get; set; }

        public abstract void OnApproach(GameClient client);
        public abstract void OnAttacked(GameClient client);
        public abstract void OnCast(GameClient client);
        public abstract void OnClick(GameClient client);
        public abstract void OnDeath(GameClient client);
        public abstract void OnLeave(GameClient client);
        public abstract void Update(TimeSpan elapsedTime);

        public virtual void OnDamaged(GameClient client, int dmg) { }
    }
}