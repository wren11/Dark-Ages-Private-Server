#region

using System;
using System.Collections.Concurrent;
using Darkages.Common;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    public class Trap
    {
        public static ConcurrentDictionary<int, Trap> Traps = new ConcurrentDictionary<int, Trap>();

        private int _ticks;

        public Trap()
        {
            _ticks = 0;
        }

        public int CurrentMapId { get; set; }
        public int Duration { get; set; }
        public Position Location { get; set; }
        [JsonIgnore] public Sprite Owner { get; set; }
        public int Radius { get; set; }
        public int Serial { get; set; }
        public Item TrapItem { get; private set; }
        public Action<Sprite, Sprite> Tripped { get; set; }

        public static bool Activate(Trap trap, Sprite target)
        {
            trap.Tripped?.Invoke(trap.Owner, target);
            return RemoveTrap(trap);
        }

        public static bool RemoveTrap(Trap traptoRemove)
        {
            lock (Traps)
            {
                if (Traps.TryRemove(traptoRemove.Serial, out var trap))
                {
                    trap.TrapItem?.Remove();
                    return true;
                }
            }

            return false;
        }

        public static bool Set(Sprite obj, int duration, int radius = 1, Action<Sprite, Sprite> cb = null)
        {
            var itemTemplate = new ItemTemplate
            {
                Name = "A Hidden Trap",
                Image = 500,
                DisplayImage = 0x8000 + 500,
                Flags = ItemFlags.Trap
            };

            var pos = obj.Position;

            if (obj is Aisling aisling)
            {
                if (!aisling.Client.IsMoving)
                    pos = aisling.LastPosition;
                else
                    pos = aisling.Position;
            }

            var ts = Item.Create(obj, itemTemplate, true);
            ts.Release(obj, pos);

            lock (Generator.Random)
            {
                var id = Generator.GenerateNumber();

                return Traps.TryAdd(id, new Trap
                {
                    Radius = radius,
                    Duration = duration,
                    CurrentMapId = obj.CurrentMapId,
                    Location = pos,
                    Owner = obj,
                    Tripped = cb,
                    Serial = id,
                    TrapItem = ts
                });
            }
        }

        public void Update()
        {
            if (_ticks > Duration)
            {
                RemoveTrap(this);
                _ticks = 0;
            }

            _ticks++;
        }
    }
}