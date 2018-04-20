namespace Darkages.Network.ClientFormats
{
    public class ClientFormat13 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x13;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}