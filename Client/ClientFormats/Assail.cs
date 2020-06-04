using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darkages.Network;

namespace DAClient.ClientFormats
{
    public class Assail : NetworkFormat
    {
        public override byte Command => 0x13;
        public override bool Secured => true;

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x01);
        }
    }
}
