#region

using System.Collections.ObjectModel;
using System.IO;
using Darkages.Compression;
using Darkages.IO;
using Darkages.Network;

#endregion

namespace Darkages.Types
{
    public class Metafile : CompressableObject, IFormattable
    {
        public Metafile()
        {
            Nodes = new Collection<MetafileNode>();
        }

        public Collection<MetafileNode> Nodes { get; set; }
        public uint Hash { get; set; }
        public string Name { get; set; }

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.WriteStringA(Name);
            writer.Write(Hash);
            writer.Write(
                (ushort) DeflatedData.Length);
            writer.Write(DeflatedData);
        }

        public override void Load(MemoryStream stream)
        {
            using (var reader = new BufferReader(stream))
            {
                int length = reader.ReadUInt16();

                for (var i = 0; i < length; i++)
                {
                    var node = new MetafileNode(reader.ReadStringA());
                    var atomSize = reader.ReadUInt16();

                    for (var j = 0; j < atomSize; j++)
                        node.Atoms.Add(
                            reader.ReadStringB());

                    Nodes.Add(node);
                }
            }

            Hash = Crc32Provider.ComputeChecksum(InflatedData);
            Name = Path.GetFileName(Filename);
        }

        public override void Save(MemoryStream stream)
        {
            using (var writer = new BufferWriter(stream))
            {
                writer.Write(
                    (ushort) Nodes.Count);

                foreach (var node in Nodes)
                {
                    writer.WriteStringA(node.Name);
                    writer.Write(
                        (ushort) node.Atoms.Count);

                    foreach (var atom in node.Atoms) writer.WriteStringB(atom);
                }
            }
        }
    }
}