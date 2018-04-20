using System;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat19 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x19;

        public string Name { get; set; }

        public string Message { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Name = reader.ReadStringA();
            Message = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
