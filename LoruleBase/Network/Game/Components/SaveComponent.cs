#region

using System;

#endregion

namespace Darkages.Network.Game.Components
{
    public class Save : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public Save(GameServer server) : base(server)
        {
            _timer = new GameServerTimer(TimeSpan.FromSeconds(45));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                ServerContextBase.SaveCommunityAssets();
                _timer.Reset();
            }
        }
    }
}