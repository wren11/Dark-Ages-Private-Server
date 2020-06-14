#region

using Ionic.Zlib;
using System;
using System.IO;

#endregion

namespace Darkages.Compression
{
    public static class CompressionProvider
    {
        public static byte[] Deflate(byte[] buffer)
        {
            using (var iStream = new MemoryStream(buffer))
            {
                using (var oStream = new MemoryStream())
                {
                    using (var zStream = new ZlibStream(oStream, CompressionMode.Compress))
                    {
                        CopyStream(iStream, zStream);
                        return oStream.ToArray();
                    }
                }
            }
        }

        public static byte[] Inflate(byte[] buffer)
        {
            using (var iStream = new MemoryStream(buffer))
            using (var oStream = new MemoryStream())
            using (var zStream = new ZlibStream(oStream, CompressionMode.Decompress))
            {
                try
                {
                    CopyStream(iStream, zStream);
                    return oStream.ToArray();
                }
                catch (Exception e)
                {
                    ServerContext.Error(e);
                    return null;
                }
            }
        }

        private static void CopyStream(Stream src, Stream dst)
        {
            var buffer = new byte[4096];
            int length;

            while ((length = src.Read(buffer, 0, buffer.Length)) > 0) dst.Write(buffer, 0, length);

            dst.Flush();
        }
    }
}