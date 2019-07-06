using System;

namespace Darkages.Network.Game.Components
{
    public class ClientTickComponent : GameServerComponent
    {
        private readonly GameServerTimer timer;

        public ClientTickComponent(GameServer server)
            : base(server)
        {
            timer = new GameServerTimer(
                TimeSpan.FromSeconds(1));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            timer.Update(elapsedTime);

            if (timer.Elapsed) timer.Reset();
        }
    }
}