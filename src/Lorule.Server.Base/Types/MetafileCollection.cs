#region

using System.Collections.Generic;
using Darkages.Network;

#endregion

namespace Darkages.Types
{
    public class MetafileCollection : List<Metafile>, IFormattable
    {
        public MetafileCollection(int capacity)
            : base(capacity)
        {
        }

        public MetafileCollection(IEnumerable<Metafile> collection)
            : base(collection)
        {
        }

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(
                (ushort) Count);

            foreach (var metafile in this)
            {
                writer.WriteStringA(
                    metafile.Name);
                writer.Write(
                    metafile.Hash);
            }
        }
    }
}