namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x0B;

        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}