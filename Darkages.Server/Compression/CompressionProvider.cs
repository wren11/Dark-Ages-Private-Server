///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
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
            {
                using (var oStream = new MemoryStream())
                {
                    using (var zStream = new ZOutputStream(oStream, -1))
                    {

                        CopyStream(iStream, zStream);

                        zStream.finish();

                        return oStream.ToArray();

                    }
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
