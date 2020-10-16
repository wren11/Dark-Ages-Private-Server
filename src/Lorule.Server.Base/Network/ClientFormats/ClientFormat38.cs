namespace Darkages.Network.ClientFormats
{
    public class ClientFormat38 : NetworkFormat
    {
        public ClientFormat38()
        {
            Secured = true;
            Command = 0x38;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}