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
using Darkages.Compression;
using Darkages.IO;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Darkages.Types
{
    public class MServerTable : CompressableObject
    {
        public MServerTable()
        {
            Servers = new Collection<MServer>();
        }

        public Collection<MServer> Servers { get; set; }

        [XmlIgnore] public ushort Size => (ushort)DeflatedData.Length;

        [XmlIgnore] public byte[] Data => DeflatedData;

        [XmlIgnore] public uint Hash { get; set; }

        public static MServerTable FromFile(string filename)
        {
            MServerTable result = null;

            if (File.Exists(filename))
            {
                using (var stream = File.OpenRead(filename))
                {
                    result = new XmlSerializer(typeof(MServerTable)).Deserialize(stream) as MServerTable;
                }

                using (var stream = new MemoryStream())
                {
                    result.Save(stream);
                    result.InflatedData = stream.ToArray();
                }

                result.Hash = Crc32Provider.ComputeChecksum(result.InflatedData);
                result.Compress();
            }

            return result;
        }

        public override void Load(MemoryStream stream)
        {
            using (var reader = new BufferReader(stream))
            {
                var count = reader.ReadByte();

                for (var i = 0; i < count; i++)
                {
                    var server = new MServer
                    {
                        Guid = reader.ReadByte(),
                        Address = reader.ReadIPAddress(),
                        Port = reader.ReadUInt16()
                    };

                    var text = reader.ReadString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    server.Name = text[0];
                    server.Description = text[1];

                    var id = reader.ReadByte();

                    Servers.Add(server);
                }
            }
        }

        public override void Save(MemoryStream stream)
        {
            using (var writer = new BufferWriter(stream))
            {
                writer.Write(
                    (byte)Servers.Count);

                foreach (var server in Servers)
                {
                    writer.Write(server.Guid);
                    writer.Write(server.Address);
                    writer.Write(server.Port);
                    writer.Write(server.Name + ";" + server.Description);
                    writer.Write(server.ID);
                }
            }
        }
    }
}
