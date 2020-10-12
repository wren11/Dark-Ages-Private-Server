using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages
{
    public class TileGrid : ObjectManager
    {
        private readonly Area _map;
        private readonly int _x, _y;

        public TileGrid(Area map, int x, int y)
        {
            _map = map;
            _x = x;
            _y = y;
        }

        public List<Sprite> Sprites
        {
            get
            {
                return GetObjects(_map, o => o.X == _x && o.Y == _y && o.Alive,
                    Get.Monsters | Get.Mundanes | Get.Aislings).ToList();
            }
        }

        public bool IsPassable(Sprite sprite, bool isAisling)
        {
            var length = 0;

            lock (Sprites)
            {
                foreach (var obj in Sprites)
                {
                    if (obj.Serial == sprite.Serial)
                    {
                        if (!isAisling)
                            continue;

                        return true;
                    }

                    if (obj.X == sprite.X && obj.Y == sprite.Y) continue;

                    if (!(obj is Monster) && !(obj is Aisling) && !(obj is Mundane))
                        continue;

                    length++;
                }

                if (!isAisling)
                    return length == 0;

                var updates = 0;

                foreach (var s in Sprites)
                {
                    if (s is Aisling)
                        continue;

                    s.Update();
                    updates++;
                }

                if (updates > 0) (sprite as Aisling)?.Client.Refresh();
            }

            return length == 0;
        }
    }
}