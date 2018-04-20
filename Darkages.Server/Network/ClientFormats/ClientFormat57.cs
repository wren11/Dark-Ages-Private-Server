namespace Darkages.Network.ClientFormats
{
    public class ClientFormat57 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x57;

        public byte Type { get; set; }
        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            if (reader.CanRead)
                Slot = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}