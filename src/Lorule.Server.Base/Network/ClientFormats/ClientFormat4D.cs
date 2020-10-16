namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4D : NetworkFormat
    {
        public ClientFormat4D()
        {
            Secured = true;
            Command = 0x4D;
        }

        public byte Lines { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Lines = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}