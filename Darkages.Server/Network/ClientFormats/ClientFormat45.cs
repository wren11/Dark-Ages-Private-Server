using System;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat45 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x45;

        public DateTime Ping { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Ping = DateTime.UtcNow;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}