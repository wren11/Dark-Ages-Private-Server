namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2A : NetworkFormat
    {
        public ClientFormat2A()
        {
            Secured = true;
            Command = 0x2A;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}