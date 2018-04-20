namespace Darkages.Network.ClientFormats
{
    public class ClientFormat66 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x66;

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
