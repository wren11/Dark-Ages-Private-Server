using System;

namespace Darkages.Network.Game
{
    public class GameServerTimer
    {
        public GameServerTimer(TimeSpan delay)
        {
            Timer = TimeSpan.Zero;
            Delay = delay;
        }

        public TimeSpan Timer { get; set; }

        public TimeSpan Delay { get; set; }

        public bool Elapsed => Timer >= Delay;

        public bool Disabled { get; set; }
        public int Interval { get; set; }
        public int Tick { get; set; }

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