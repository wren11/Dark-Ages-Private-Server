using Lorule.Client.Base.Dat;
using System.Collections.Generic;
using System.IO;

namespace Lorule.Client.Base.Types
{
    public class TileCollection
    {
        private readonly ArchivedItem _baseTileSet;
        private readonly List<Tile> _tiles = new List<Tile>();

        public Tile this[int index]
        {
            get => _tiles[index];
            set => _tiles[index] = value;
        }

        public TileCollection(ArchivedItem baseTileSet)
        {
            _baseTileSet = baseTileSet;
        }

        public void Load()
        {
            const int tileSize = 1512;

            using var reader = new BinaryReader(new MemoryStream(_baseTileSet.Data));
            var tileCount = (int) (reader.BaseStream.Length / tileSize);

            for (var i = 0; i < tileCount; i++)
            {
                var tile = new Tile
                {
                    Name = $"tile_{i + 1}",
                    Data = reader.ReadBytes(tileSize)
                };

                _tiles.Add(tile);
            }
        }
    }
}
