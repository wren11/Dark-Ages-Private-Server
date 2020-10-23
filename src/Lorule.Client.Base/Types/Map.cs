using System.Collections.Generic;
using System.IO;

namespace Lorule.Client.Base.Types
{
    public class Map
    {
        public static IEnumerable<MapTile> LoadMapTiles(string fileName)
        {
            using var reader = new BinaryReader(File.OpenRead(fileName));
            for (var i = 0; i < reader.BaseStream.Length / 6; i++)
            {
                yield return new MapTile(
                    reader.ReadUInt16(), 
                    reader.ReadUInt16(), 
                    reader.ReadUInt16());
            }
        }
    }
}
