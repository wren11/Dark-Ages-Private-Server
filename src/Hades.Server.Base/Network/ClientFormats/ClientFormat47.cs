namespace Darkages.Network.ClientFormats
{
    public class ClientFormat47 : NetworkFormat
    {
        public ClientFormat47()
        {
            Secured = true;
            Command = 0x47;
        }

        public byte Stat { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Stat = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}