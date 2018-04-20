namespace Darkages.Network.ClientFormats
{
    public class ClientFormat02 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x02;

        public string AislingUsername { get; set; }
        public string AislingPassword { get; set; }

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