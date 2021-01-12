using Darkages.Network.Game;
using Darkages.Types;
using System;

namespace Darkages.Scripting
{
    public abstract class AreaScript : IScriptBase
    {
        public GameServerTimer Timer { get; set; }
        public Area Area;

        protected AreaScript(Area area)
        {
            Area = area;
        }

        public abstract void Update(TimeSpan elapsedTime);

        public virtual void OnItemDropped(GameClient client, Item item, Position location)
        {

        }

        public virtual void OnPlayerWalk(GameClient client, Position oldLocation, Position newLocation)
        {

        }

        public virtual void OnMapEnter(GameClient client)
        {

        }

        public virtual void OnMapExit(GameClient client)
        {

        }
    }
}
