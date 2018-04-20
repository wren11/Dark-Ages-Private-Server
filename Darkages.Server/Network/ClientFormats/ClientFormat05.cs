namespace Darkages.Network.ClientFormats
{
    public class ClientFormat05 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x05;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}