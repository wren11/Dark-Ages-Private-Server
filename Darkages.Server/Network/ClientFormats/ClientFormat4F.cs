namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4F : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4F;

        public ushort Count { get; set; }
        public byte[] Image { get; set; }
        public string Words { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Count = reader.ReadUInt16();
            Image = reader.ReadBytes(reader.ReadUInt16());
            Words = reader.ReadStringB();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}