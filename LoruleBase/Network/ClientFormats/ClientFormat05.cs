namespace Darkages.Network.ClientFormats
{
    public class ClientFormat05 : NetworkFormat
    {
        public ClientFormat05()
        {
            Secured = true;
            Command = 0x05;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}