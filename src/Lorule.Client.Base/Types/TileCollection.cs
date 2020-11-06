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
            get
            {
                if (index >= 0 && _tiles.Count > index)
                    return _tiles[index];

                return null;
            }
            set
            {
                if (index >= 0 && _tiles.Count > index)
                    _tiles[index] = value;
            }
        }


        public TileCollection(ArchivedItem baseTileSet)
        {
            _baseTileSet = baseTileSet;
        }

        public List<Tile> Load()
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

            return new List<Tile>(_tiles);
        }


        public static void Save(string fileName, IReadOnlyList<Tile> tiles)
        {
            using var writer = new BinaryWriter(File.OpenWrite(fileName));

            foreach (var file in tiles)
            {
                writer.Write(file.Data);
            }
        }
    }
}
