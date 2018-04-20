namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2D : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x2D;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}