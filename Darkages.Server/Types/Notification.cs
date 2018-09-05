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
using Darkages.Common;
using Darkages.Compression;
using Darkages.IO;
using System.IO;

namespace Darkages.Types
{
    public class Notification : CompressableObject
    {
        public byte[] Data => DeflatedData;

        public ushort Size => (ushort)DeflatedData.Length;

        public uint Hash { get; private set; }

        public static Notification FromFile(string filename)
        {
            var result          = new Notification();
            var message         = File.ReadAllText(filename);

            result.InflatedData = message.ToByteArray();
            result.Hash         = Crc32Provider.ComputeChecksum(result.InflatedData);
            result.Compress();

            ServerContext.GlobalMessage = message;

            return result;
        }
    }
}
