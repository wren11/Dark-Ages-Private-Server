namespace Darkages.Network.ClientFormats
{
    public class ClientFormat13 : NetworkFormat
    {
        public ClientFormat13()
        {
            Secured = true;
            Command = 0x13;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}