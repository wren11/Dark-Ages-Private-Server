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
        private readonly string _location;
        private readonly Dictionary<string, ISet<ArchivedItem>> _cachedItemCollection = new Dictionary<string, ISet<ArchivedItem>>();

        public Archive(string location)
        {
            _location = location;
        }

        public async Task Load(string archiveName,
            bool save = false,
            string root = "Archives",
            string outputDirectory = "")
        {
            if (archiveName == null) throw new ArgumentNullException(nameof(archiveName));
            var collection = new HashSet<ArchivedItem>();

            await foreach (var entry in Open(Path.Combine(_location, root, archiveName)))
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


        private async IAsyncEnumerable<ArchivedItem> Open(string fileName)
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
    }
}
