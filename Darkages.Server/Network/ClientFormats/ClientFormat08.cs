namespace Darkages.Network.ClientFormats
{
    public class ClientFormat08 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x08;

        public byte ItemSlot { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public int ItemAmount { get; set; }
        public short Unknown { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            ItemSlot = reader.ReadByte();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            ItemAmount = reader.ReadInt32();

            if (reader.CanRead)
                Unknown = reader.ReadInt16();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}