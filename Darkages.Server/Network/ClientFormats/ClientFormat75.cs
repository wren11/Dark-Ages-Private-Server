namespace Darkages.Network.ClientFormats
{
    public class ClientFormat75 : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x75;

        public long Tick { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Tick = (long)(reader.ReadByte() >> 4) - 0x15;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}