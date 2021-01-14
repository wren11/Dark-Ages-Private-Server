using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lorule.Client.Base.Dat;

namespace Lorule.Content.Editor.Dat
{
    public class PaletteTable
    {
        public int Palette { get; set; }
        public (int,int) PaletteRange { get; set; }

        public PaletteTable(int min, int max, int palette)
        {
            PaletteRange = (min, max);
            Palette = palette;
        }

        public static async Task<List<PaletteTable>> FromArchive(
            IEnumerable<ArchivedItem> tablesFound,
            string pattern,
            Action<List<PaletteTable>> additionalPalettes = null)
        {
            var results = new List<PaletteTable>();
            var extendedPalettes = new List<PaletteTable>();

            foreach (var item in tablesFound)
            {
                var name = Path.GetFileNameWithoutExtension(item.Name).Substring(pattern.Length);

                if (name.ToLower().Contains("ani"))
                {
                    continue;
                }

                using var br = new StreamReader(new MemoryStream(item.Data));
                while (!br.EndOfStream)
                {
                    var line = Regex.Replace(await br.ReadLineAsync() ?? string.Empty, @"\s+", "|");

                    if (line == string.Empty)
                        continue;

                    var segments = line.Split('|');
                    var newPaletteTable = new PaletteTable(0, 0, 0);

                    switch (segments.Length)
                    {
                        case 3:
                        {
                            newPaletteTable.PaletteRange = (Convert.ToInt32(segments[0]), Convert.ToInt32(segments[1]));
                            newPaletteTable.Palette = Convert.ToInt32(segments[2]);
                            break;
                        }
                        case 2:
                        {
                            var min = Convert.ToInt32(segments[0]);
                            newPaletteTable.PaletteRange = (min - 1, min);
                            newPaletteTable.Palette = Convert.ToInt32(segments[1]);
                            break;
                        }
                    }

                    if (!int.TryParse(name, out _))
                        results.Add(newPaletteTable);
                    else
                        extendedPalettes.Add(newPaletteTable);
                }
            }

            if (extendedPalettes.Any())
                additionalPalettes?.Invoke(extendedPalettes);

            return results;
        }
    }

    public interface IPaletteLookup
    {
        List<PaletteTable> GetBackgroundPalettesTables(bool extended = false);
        List<Palette> GetBackgroundPalettes();
        (int, Palette) GetBackgroundPaletteIndex(int index);
    }

    public interface IPaletteCollection : IPaletteLookup
    {
        void LoadPalettes(string archiveName, string pattern);
        void LoadTables(string archiveName, string pattern);
    }

    public class PaletteCollection : IPaletteCollection
    {
        public static readonly (string,string) BackgroundPalettes =  ("seo\\seo.dat", "mpt");

        private readonly IArchive _archiveService;

        public Dictionary<(string, string), List<Palette>> Palettes = new Dictionary<(string, string), List<Palette>>();
        public Dictionary<(string, string), List<PaletteTable>> PaletteTables = new Dictionary<(string, string), List<PaletteTable>>();
        public Dictionary<(string, string), List<PaletteTable>> ExtendedPaletteTables = new Dictionary<(string, string), List<PaletteTable>>();

        public PaletteCollection(IArchive archiveService, ILogger<PaletteCollection> logger)
        {
            _archiveService = archiveService;
            logger.LogInformation("PaletteCollection loaded.");
        }

        public List<PaletteTable> GetBackgroundPalettesTables(bool extended = false)
            => extended ? ExtendedPaletteTables[BackgroundPalettes] : PaletteTables[BackgroundPalettes];

        public List<Palette> GetBackgroundPalettes() 
            => Palettes[BackgroundPalettes];

        public (int, Palette) GetBackgroundPaletteIndex(int index)
            => GetPaletteIndex(index, BackgroundPalettes);

        private (int, Palette) GetPaletteIndex(int index, (string,string) key)
        {
            var paletteIndex = 0;


            if (PaletteTables.ContainsKey(key))
                foreach (var entry in PaletteTables[key]
                    .Where(entry => index >= entry.PaletteRange.Item1 && index <= entry.PaletteRange.Item2))
                {
                    paletteIndex = entry.Palette;
                }

            //if (ExtendedPaletteTables.ContainsKey(key))
            //    foreach (var o in ExtendedPaletteTables[key]
            //        .Where(o => index >= o.PaletteRange.Item1 && index <= o.PaletteRange.Item2))
            //    {
            //        paletteIndex = o.Palette;
            //    }

            return (paletteIndex, Palettes[key][paletteIndex]);
        }

        public async void LoadPalettes(string archiveName, string pattern)
        {
            var palettesFound = _archiveService.SearchArchive(".pal", pattern, archiveName);
            Palettes[(archiveName, pattern)] = Palette.FromArchive(palettesFound);
            await Task.CompletedTask;
        }

        public async void LoadTables(string archiveName, string pattern)
        {
            var tablesFound = _archiveService.SearchArchive(".tbl", pattern, archiveName);
            var archivedItems = tablesFound as ArchivedItem[] ?? tablesFound.ToArray();

            if (!archivedItems.Any())
                return;

            PaletteTables[(archiveName, pattern)] = await PaletteTable.FromArchive(archivedItems, pattern,
                ext => { ExtendedPaletteTables[(archiveName, pattern)] = ext; }); 
        }
    }

    public class Palette
    {
        private const int TableSize = 256;

        public Color[] Colors = new Color[TableSize];

        public Color this[int index] => Colors[index];

        public static List<Palette> FromArchive(IEnumerable<ArchivedItem> palettesFound)
        {
            var result = new List<Palette>();
            foreach (var item in palettesFound)
            {
                using var br = new BinaryReader(new MemoryStream(item.Data));
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                var palette = new Palette();

                for (var i = 0; i < TableSize; i++)
                {
                    palette.Colors[i] = 
                        Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
                }

                result.Add(palette);
            }

            return result;
        }

        public static Palette FromFile(string fileName)
        {
            using var br = new BinaryReader(File.OpenRead(fileName));
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            var palette = new Palette();

            for (var i = 0; i < TableSize; i++)
            {
                palette.Colors[i] =
                    Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
            }

            return palette;
        }
    }
}
