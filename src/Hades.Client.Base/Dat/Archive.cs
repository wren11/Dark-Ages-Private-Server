using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lorule.Client.Base.Dat
{
    public class Archive : IArchive
    {
        private string _location;

        private readonly Dictionary<string, ISet<ArchivedItem>> _cachedItemCollection
            = new Dictionary<string, ISet<ArchivedItem>>();

        public Archive(string location)
        {
            _location = location;
        }

        public Archive() : this(string.Empty)
        {

        }

        public async Task Load(string archiveName,
            bool save = false,
            string root = "Archives",
            string outputDirectory = "")
        {
            if (archiveName == null) throw new ArgumentNullException(nameof(archiveName));
            var collection = new HashSet<ArchivedItem>();
            var path = Path.Combine(_location, root, archiveName);

            await foreach (var entry in UnpackArchive(path))
                if (entry != null)
                {
                    if (save)
                        await entry.Save(outputDirectory);

                    collection.Add(entry);
                }

            _cachedItemCollection[archiveName] = collection;
        }


        public IEnumerable<ArchivedItem> SearchArchive(string extension, string stringPattern, string archiveName)
        {
            if (stringPattern == null)
                throw new ArgumentNullException(nameof(stringPattern));
            if (!_cachedItemCollection.ContainsKey(archiveName))
                yield break;

            foreach (var item in _cachedItemCollection[archiveName])
            {
                if (item.Name.EndsWith(extension) && item.Name.Contains(stringPattern))
                {
                    yield return item;
                }
            }
        }

        public ArchivedItem Get(string name, string archiveName)
        {
            if (archiveName == null) throw new ArgumentNullException(nameof(archiveName));
            if (_cachedItemCollection.ContainsKey(archiveName))
                return _cachedItemCollection[archiveName].FirstOrDefault(archivedItem =>
                    archivedItem.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return null;
        }

        public class ArchiveLookupTableEntry
        {
            public readonly int MetaIndex;

            public string EntryName { get; set; }

            public int StartofData  { get; set; }

            public int EndOfData    { get; set; }

            public byte[] RawBytes { get; set; }

            public ArchiveLookupTableEntry(byte[] rawBytes, int metaIndex)
            {
                MetaIndex = metaIndex;
                RawBytes = new List<byte>(rawBytes).ToArray();
            }
        }

        public void PackArchive(string unpackedDirectory, string outputFileName)
        {
            var lookupTable = new List<ArchiveLookupTableEntry>();
            var i = 0;

            foreach (var file in Directory.EnumerateFiles(unpackedDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                if (file == null)
                    continue;

                if (Path.GetExtension(file).Equals(".meta"))
                    continue;

                var entry = new ArchiveLookupTableEntry(File.ReadAllBytes(file), i)
                {
                    EntryName = Path.GetFileName(file).PadRight(13, '\0')
                };

                lookupTable.Add(entry);
                i++;
            }

            lookupTable = lookupTable.OrderBy(lookupTableEntry => lookupTableEntry.EntryName).ToList();

            var start = 17 * (lookupTable.Count + 1) + 4;

            for (var index = 0; index < lookupTable.Count; index++)
            {
                if (index == 0)
                {
                    lookupTable[index].StartofData = start;
                    lookupTable[index].EndOfData   = start + lookupTable[index].RawBytes.Length;
                }
                else
                {
                    lookupTable[index].StartofData = lookupTable[index - 1].EndOfData;
                    lookupTable[index].EndOfData = lookupTable[index].StartofData + lookupTable[index].RawBytes.Length;
                }
            }

            using var br = new BinaryWriter(File.OpenWrite(outputFileName));

            br.Write((uint)lookupTable.Count + 1);
            foreach (var data in lookupTable.OrderBy(n => n.EntryName))
            {
                br.Write(data.StartofData);
                br.Write(Encoding.UTF8.GetBytes(data.EntryName));
                br.Write(data.EndOfData);
                br.BaseStream.Position -= 4;
            }

            br.Seek(start, SeekOrigin.Begin);
            foreach (var data in lookupTable.OrderBy(n => n.EntryName))
            {
                br.Write(data.RawBytes.ToArray());
            }
            br.Write(new byte[13]);
        }

        public async IAsyncEnumerable<ArchivedItem> UnpackArchive(string fileName)
        {
            const int entryLength = 13;

            static string GetItemName(string name)
            {
                var @null = name.IndexOf('\0');
                return @null == -1 ? name : name.Substring(0, @null);
            }

            static async Task<byte[]> Extract(Stream stream, int itemStart, int itemSize)
            {
                if (stream == null) throw new ArgumentNullException(nameof(stream));
                var buffer = new byte[itemSize];
                stream.Seek(itemStart, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }

            using var br = new BinaryReader(File.OpenRead(fileName));
            var itemCount = br.ReadUInt32();

            for (var i = 0; i < itemCount - 1; i++)
            {
                var itemStart = br.ReadInt32();
                var itemName  = GetItemName(Encoding.ASCII.GetString(br.ReadBytes(entryLength)));
                var itemEnd   = br.ReadInt32();
                var itemSize  = itemEnd - itemStart;
                var resumeStreamPosition = br.BaseStream.Position - 4;

                yield return new ArchivedItem(itemName, 
                    Path.GetFileNameWithoutExtension(fileName),
                    await Extract(br.BaseStream, itemStart, itemSize), i);

                br.BaseStream.Seek(resumeStreamPosition, SeekOrigin.Begin);
            }
        }

        public void SetLocation(string location)
        {
            _location = location;
        }
    }
}
