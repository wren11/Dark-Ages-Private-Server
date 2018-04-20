using System.IO;
using zlib;

namespace Darkages.Compression
{
    public static class CompressionProvider
    {
        private static void CopyStream(Stream src, Stream dst)
        {
            var buffer = new byte[4096];
            int length;

            while ((length = src.Read(buffer, 0, buffer.Length)) > 0) dst.Write(buffer, 0, length);

            dst.Flush();
        }

        public static byte[] Deflate(byte[] buffer)
        {
            using (var iStream = new MemoryStream(buffer))
            using (var oStream = new MemoryStream())
            using (var zStream = new ZOutputStream(oStream, -1))
            {
                try
                {
                    CopyStream(iStream, zStream);

                    zStream.finish();

                    return oStream.ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        public static byte[] Inflate(byte[] buffer)
        {
            using (var iStream = new MemoryStream(buffer))
            using (var oStream = new MemoryStream())
            using (var zStream = new ZOutputStream(oStream))
            {
                try
                {
                    CopyStream(iStream, zStream);

                    zStream.finish();

                    return oStream.ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}