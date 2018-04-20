namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4B;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}