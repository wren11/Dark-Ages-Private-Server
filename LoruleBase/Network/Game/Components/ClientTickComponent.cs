using System;
using System.Diagnostics;

namespace Darkages.Network.Game.Components
{
    public class ClientTickComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer;

        public ClientTickComponent(GameServer server)
            : base(server)
        {
            _timer = new GameServerTimer(
                TimeSpan.FromSeconds(30));
        }

        public bool IsUpdating { get; set; } = false;

        public override void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);



            if (_timer.Elapsed)
            {
                _timer.Reset();
                ServerContextBase.Debug($"Server Running on {Process.GetCurrentProcess().Threads.Count} Threads.");
            }

        }
    }
}