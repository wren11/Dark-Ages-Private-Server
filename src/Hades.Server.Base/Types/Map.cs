using System.Collections.Generic;
using System.Text.Json.Serialization;
using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages
{
    public class Map : ObjectManager
    {
        public ushort Cols { get; set; }
        public string ContentName { get; set; }
        public MapFlags Flags { get; set; }
        public int Id { get; set; }
        public int Music { get; set; }
        public string Name { get; set; }
        public ushort Rows { get; set; }
        public List<Position> Blocks { get; set; }
        public string ScriptKey { get; set; }

        [JsonIgnore]
        public int ID => Id;

        public Map()
        {
            Blocks = new List<Position>();
        }
    }
}