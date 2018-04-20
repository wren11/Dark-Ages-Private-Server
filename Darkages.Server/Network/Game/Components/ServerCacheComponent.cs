using System;

namespace Darkages.Network.Game.Components
{
    public class ServerCacheComponent : GameServerComponent
    {
        GameServerTimer Timer;
        public ServerCacheComponent(GameServer server) : base(server)
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(45));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                ServerContext.SaveCommunityAssets();
                Timer.Reset();
            }
        }
    }
}
