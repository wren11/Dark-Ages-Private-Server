using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Types
{
    public class Trap : ObjectManager
    {
        //Script Callbacks
        public Action<Sprite> Activated { get; set; }
        public Action<Sprite> Toggle { get; set; }
        public Action<Sprite, Sprite> Tripped { get; set; }

        public Position Location { get; set; }
        public Sprite Creator { get; set; }
        public int Serial { get; set; }
        public int CurrentMapId { get; set; }
        public double EffectRadius { get; set; }
        public int Duration { get; set; }

        private int Interval;

        public bool Update(TimeSpan elapsed)
        {
            HandleTimer();
            HandleUpdate();

            return true;
        }

        private void HandleUpdate()
        {

            var affected_check = GetObjects(
                    i => i.Serial != Creator.Serial && i.CurrentMapId == CurrentMapId
                    && i.Position.DistanceFrom(Location) <= EffectRadius,
                    Get.Aislings | Get.Monsters | Get.Mundanes
                );

            CheckTriggerState(affected_check);
        }

        private void HandleTimer()
        {
            Creator?.Show(Scope.Self, new ServerFormat29(1, Location.X, Location.Y));

            if (Interval++ >= Duration)
            {
                Trap.Dispose(this);
                Interval = 0;
            }
        }

        private void CheckTriggerState(IEnumerable<Sprite> affected_check)
        {
            var count = 0;

            foreach (var affected in affected_check)
            {
                Tripped?.Invoke(Creator, affected);
                count++;
            }

            if (count > 0)
            {
                Trap.Dispose(this);
            }
        }

        public static ConcurrentDictionary<int, Trap>
            TrapCache = new ConcurrentDictionary<int, Trap>();

        public static bool CreateTrap(Sprite sprite, Trap trap)
        {
            trap.Interval = 0;

            lock (Generator.Random)
            {
                trap.Serial = Generator.GenerateNumber();
            }

            trap.Creator       = sprite;
            trap.CurrentMapId  = sprite.CurrentMapId;
            trap.Location      = new Position { X = (ushort)sprite.X, Y = (ushort)sprite.Y };

            lock (Trap.TrapCache)
            {
                if (TrapCache.TryAdd(trap.Serial, trap))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveTraps(Aisling aisling)
        {
            return aisling.Traps.All(i => Trap.Dispose(i));          
        }

        public static bool UpdateAll(TimeSpan elapsedTime)
        {
            return Trap.TrapCache.All(i => i.Value.Update(elapsedTime));
        }

        public static bool Dispose(Trap trap)
        {
            lock (Trap.TrapCache)
            {
                try
                {
                    return TrapCache
                        .TryRemove(trap.Serial, out var removed);
                }
                finally
                {
                    trap = null;
                }
            }
        }
    }
}
