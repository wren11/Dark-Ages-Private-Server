namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4A : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4A;

        public uint Id { get; set; }
        public byte Type { get; set; }
        public byte ItemSlot { get; set; }
        public int Gold { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Id = reader.ReadUInt32();

            if (Type == 0x01 && reader.CanRead)
                ItemSlot = reader.ReadByte();

            if (Type == 0x03 && reader.CanRead)
            {
                Gold = reader.ReadInt32();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
