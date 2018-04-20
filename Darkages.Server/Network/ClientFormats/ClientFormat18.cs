namespace Darkages.Network.ClientFormats
{
    public class ClientFormat18 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x18;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}