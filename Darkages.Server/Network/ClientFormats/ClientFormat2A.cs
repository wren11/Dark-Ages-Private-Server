namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2A : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x2A;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}
