namespace Darkages.Network.ClientFormats
{
    public class ClientFormat04 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x04;

        public byte Gender { get; set; }
        public byte HairStyle { get; set; }
        public byte HairColor { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            HairStyle = reader.ReadByte();
            Gender = reader.ReadByte();
            HairColor = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}