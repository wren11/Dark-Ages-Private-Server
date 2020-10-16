namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3E : NetworkFormat
    {
        public ClientFormat3E()
        {
            Secured = true;
            Command = 0x3E;
        }

        public byte Index { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}