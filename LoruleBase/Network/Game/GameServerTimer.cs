#region

using System;

#endregion

namespace Darkages.Network.Game
{
    public class GameServerTimer
    {
        public GameServerTimer(TimeSpan delay)
        {
            Timer = TimeSpan.Zero;
            Delay = delay;
        }

        public TimeSpan Delay { get; set; }
        public bool Disabled { get; set; }
        public bool Elapsed => Timer >= Delay;
        public int Tick { get; set; }
        public TimeSpan Timer { get; set; }

        public void Reset()
        {
            Timer = TimeSpan.Zero;
        }

        public void Update(TimeSpan elapsedTime)
        {
            Timer += elapsedTime;
        }
    }
}