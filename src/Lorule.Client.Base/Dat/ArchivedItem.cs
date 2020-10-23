using ServiceStack;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lorule.Client.Base.Dat
{
    public class ArchivedItem
    {
        private readonly string _archiveName;

        public string Name { get; }
        public int Index { get; }
        public byte[] Data { get; }

        public ArchivedItem(string name, string archiveName, byte[] data, int index)
        {
            Name = name;
            Index = index;

            _archiveName = archiveName;

            Data = new List<byte>(data).ToArray();
        }

        public async Task Save(string directory)
        {
            var outputPath = Path.Combine(directory, _archiveName);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            using var stream = File.OpenWrite(Path.Combine(outputPath, Name));
            await new MemoryStream(Data).WriteToAsync(stream, Encoding.UTF8, CancellationToken.None);
        }
    }
}