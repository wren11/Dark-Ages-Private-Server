using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Lorule.Client.Base.Dat
{
    public class ArchivedItem
    {
        [JsonIgnore]
        private readonly string _archiveName;

        public string Name { get; }
        public int Index { get; }

        [JsonIgnore]
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
            await WriteToAsync(new MemoryStream(Data), stream, CancellationToken.None);
        }


        public static async Task WriteToAsync(MemoryStream stream, Stream output,  CancellationToken token)
        {
            if (stream.TryGetBuffer(out var buffer))
            {
                await output.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, token);
                return;
            }

            var bytes = stream.ToArray();
            await output.WriteAsync(bytes, 0, bytes.Length, token);
        }
    }
}