namespace Darkages.Network.ClientFormats
{
    public class ClientFormat00 : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x00;

        public int Version { get; set; }
        public byte UnknownA { get; set; }
        public byte UnknownB { get; set; }


        public override void Serialize(NetworkPacketReader reader)
        {
            Version = reader.ReadUInt16();
            UnknownA = reader.ReadByte();
            UnknownB = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}