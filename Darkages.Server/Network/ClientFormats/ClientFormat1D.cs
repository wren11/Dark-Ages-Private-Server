namespace Darkages.Network.ClientFormats
{
    public class ClientFormat1D : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x1D;

        public byte Number { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Number = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}