using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darkages.Network;

namespace DAClient.ClientFormats
{
    public class ClientVersion : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x00;

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((ushort)718);
            writer.Write(0x4C);
            writer.Write(0x4B);
            writer.Write(0x00);
        }
    }
}
