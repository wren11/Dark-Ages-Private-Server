namespace Darkages.Network.ClientFormats
{
    public class ClientFormat47 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x47;

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