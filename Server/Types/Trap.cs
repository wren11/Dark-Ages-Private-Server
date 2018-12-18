using Darkages.Common;
using Darkages.Network.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace Darkages.Types
{
    public class Trap
    {
        public static ConcurrentDictionary<int, Trap> Traps = new ConcurrentDictionary<int, Trap>();

        public Position Location { get; set; }
        public int      Duration { get; set; }
        public int      Serial { get; set; }
        public int      CurrentMapId { get; set; }
        public int      Radius { get; set; }
        
        [JsonIgnore]
        public Sprite Owner { get; set; }

        public Action<Sprite, Sprite> Tripped { get; set; }

        private int _Ticks;

        public Trap()
        {
            _Ticks = 0;
        }

        public static bool Set(Sprite obj, int _duration, int _radius = 1, Action<Sprite, Sprite> cb = null)
        {
            lock (Generator.Random)
            {
                var id = Generator.GenerateNumber();

                return (Traps.TryAdd(id, new Trap()
                {
                    Radius = _radius,
                    Duration = _duration,
                    CurrentMapId = obj.CurrentMapId,
                    Location = new Position(obj.Position.X, obj.Position.Y),
                    Owner = obj,
                    Tripped = cb,
                    Serial = id,
                }));
            }
        }

        public static bool Activate(Trap trap, Sprite target)
        {
            trap.Tripped?.Invoke(trap.Owner, target);
            return RemoveTrap(trap);
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (_Ticks > Duration)
            {
                RemoveTrap(this);
                _Ticks = 0;
            }

            _Ticks++;
        }

        public static bool RemoveTrap(Trap traptoRemove)
        {
            lock (Traps)
            {
                if (Traps.TryRemove(traptoRemove.Serial, out var trap))
                {
                    traptoRemove = null;
                    return true;
                }
            }

            return false;
        }
    }
}
