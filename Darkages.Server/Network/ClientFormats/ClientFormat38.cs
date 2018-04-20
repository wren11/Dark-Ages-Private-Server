namespace Darkages.Network.ClientFormats
{
    public class ClientFormat38 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x38;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}