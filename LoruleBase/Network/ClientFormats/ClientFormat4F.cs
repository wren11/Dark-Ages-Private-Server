namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4F : NetworkFormat
    {
        public ClientFormat4F()
        {
            Secured = true;
            Command = 0x4F;
        }

        public ushort Count { get; set; }
        public byte[] Image { get; set; }
        public string Words { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}