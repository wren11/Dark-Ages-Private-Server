using Lorule.Client.Base.Dat;
using Lorule.Client.Base.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hades.Utils.TileAMerge
{
    public class TileMerger
    {
        private readonly IArchive _archiveService;

        public TileMerger(IArchive archiveService)
        {
            _archiveService = archiveService;
        }

        public async Task Combine(string first, string second, string target, string outputFileName)
        {
            async Task LoadPairs()
            {
                await _archiveService.Load(first);
                await _archiveService.Load(second);
            }

            await LoadPairs();

            var firstTileset  = _archiveService.Get(target, first);
            var secondTileset = _archiveService.Get(target, second);
            var newTileSet = CombineTiles(firstTileset, secondTileset);

            TileCollection.Save(outputFileName, newTileSet);
            {
                await Task.CompletedTask;
            }


            static IReadOnlyList<Tile> CombineTiles(ArchivedItem archivedItem, ArchivedItem secondTileset1)
            {
                var tiles = new List<Tile>();
                {
                    var (tiles1, tiles2) = (new TileCollection(archivedItem).Load(),
                        new TileCollection(secondTileset1).Load());

                    var newSeoTiles = tiles2.Skip(tiles1.Count).ToArray();
                    {
                        tiles.AddRange(tiles1);
                        tiles.AddRange(newSeoTiles);
                    }
                }

                return tiles;
            }
        }
    }
}
