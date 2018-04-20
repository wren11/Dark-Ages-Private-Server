using System;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x3B;


        public DateTime Ping { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            Ping = DateTime.UtcNow;

            writer.Write(0x0001);
        }
    }
}