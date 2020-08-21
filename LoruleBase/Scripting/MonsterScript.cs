#region

using System;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Types;

#endregion

namespace Darkages.Scripting
{
    public abstract class MonsterScript : ObjectManager
    {
        public Area Map;

        public Monster Monster;

        public MonsterScript(Monster monster, Area map)
        {
            Monster = monster;
            Map = map;
        }

        public abstract void OnApproach(GameClient client);

        public abstract void OnAttacked(GameClient client);

        public abstract void OnCast(GameClient client);

        public abstract void OnClick(GameClient client);

        public virtual void OnDamaged(GameClient client, int dmg, Sprite source)
        {
        }

        public abstract void OnDeath(GameClient client);

        public abstract void OnLeave(GameClient client);

        public abstract void OnSkulled(GameClient client);

        public abstract void Update(TimeSpan elapsedTime);
    }
}