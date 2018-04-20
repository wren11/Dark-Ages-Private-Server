namespace Darkages.Network.ClientFormats
{
    public class ClientFormat44 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x44;

        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Slot = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}