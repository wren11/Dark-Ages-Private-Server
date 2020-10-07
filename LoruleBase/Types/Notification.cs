#region

using System.IO;
using Darkages.Common;
using Darkages.Compression;
using Darkages.IO;

#endregion

namespace Darkages.Types
{
    public class Notification : CompressableObject
    {
        public byte[] Data => DeflatedData;

        public uint Hash { get; private set; }
        public ushort Size => (ushort) DeflatedData.Length;

        public static Notification FromFile(string filename)
        {
            var result = new Notification();
            var message = File.ReadAllText(filename);

            result.InflatedData = message.ToByteArray();
            result.Hash = Crc32Provider.ComputeChecksum(result.InflatedData);
            result.Compress();

            ServerContext.GlobalMessage = message;

            return result;
        }
    }
}