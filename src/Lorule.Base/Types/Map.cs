using System.Collections.Generic;
using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages
{
    public class Map : ObjectManager
    {
        public ushort Cols { get; set; }
        public string ContentName { get; set; }
        public MapFlags Flags { get; set; }
        public int ID { get; set; }
        public int Music { get; set; }
        public string Name { get; set; }
        public ushort Rows { get; set; }
        public List<Position> Blocks { get; set; }

        public Map()
        {
            Blocks = new List<Position>();
        }
    }
}