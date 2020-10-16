namespace Darkages.Network.ClientFormats
{
    public class ClientFormat32 : NetworkFormat
    {
        public ClientFormat32()
        {
            Secured = true;
            Command = 0x32;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}