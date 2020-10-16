#region

using System.IO;
using ComponentAce.Compression.Libs.zlib;

#endregion

namespace Darkages.Compression
{
    public static class CompressionProvider
    {
        public static byte[] Deflate(byte[] buffer)
        {
            var iStream = new MemoryStream(buffer);
            var oStream = new MemoryStream();
            var zStream = new ZOutputStream(oStream, -1);

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
            finally
            {
                zStream.Close();
                oStream.Close();
                iStream.Close();
            }
        }

        public static byte[] Inflate(byte[] buffer)
        {
            var iStream = new MemoryStream(buffer);
            var oStream = new MemoryStream();
            var zStream = new ZOutputStream(oStream);

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
            finally
            {
                zStream.Close();
                oStream.Close();
                iStream.Close();
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