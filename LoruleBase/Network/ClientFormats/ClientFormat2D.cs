namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2D : NetworkFormat
    {
        public ClientFormat2D()
        {
            Secured = true;
            Command = 0x2D;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}