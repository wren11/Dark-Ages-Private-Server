namespace Darkages.Network.ClientFormats
{
    public class ClientFormat02 : NetworkFormat
    {
        public string AislingPassword;

        public string AislingUsername;

        public ClientFormat02()
        {
            Secured = true;
            Command = 0x02;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            AislingUsername = reader.ReadStringA();
            AislingPassword = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}