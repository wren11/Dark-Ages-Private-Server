namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2F : NetworkFormat
    {
        public override bool Secured => true;
        public override byte Command => 0x2F;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}