namespace Darkages.Network.ClientFormats
{
    public class ClientFormat66 : NetworkFormat
    {
        public ClientFormat66()
        {
            Secured = true;
            Command = 0x66;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}