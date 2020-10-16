namespace Darkages.Network.ClientFormats
{
    public class ClientFormat18 : NetworkFormat
    {
        public ClientFormat18()
        {
            Secured = true;
            Command = 0x18;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}