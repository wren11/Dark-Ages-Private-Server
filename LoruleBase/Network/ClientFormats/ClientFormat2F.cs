namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2F : NetworkFormat
    {
        public ClientFormat2F()
        {
            Secured = true;
            Command = 0x2F;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}