using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;

namespace Lorule.Content.Editor.Dat
{
    public class ArchivedItem
    {
        private readonly string _archiveName;
        private readonly MemoryStream _dataStream;

        public string Name { get; }
        public int Index { get; }

        public ArchivedItem(string name, string archiveName, MemoryStream data, int index)
        {
            Name = name;
            Index = index;

            _dataStream = data;
            _archiveName = archiveName;
        }

        public async Task Save(string directory)
        {
            var outputPath = Path.Combine(directory, _archiveName);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            using var stream = File.OpenWrite(Path.Combine(outputPath, Name));
            await _dataStream.WriteToAsync(stream, Encoding.UTF8, CancellationToken.None);
        }
    }
}