namespace Darkages.Network.ClientFormats
{
    public class ClientFormat08 : NetworkFormat
    {
        public ClientFormat08()
        {
            Secured = true;
            Command = 0x08;
        }

        public int ItemAmount { get; set; }
        public byte ItemSlot { get; set; }
        public short Unknown { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            ItemSlot = reader.ReadByte();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            ItemAmount = reader.ReadInt32();

            if (reader.GetCanRead())
                Unknown = reader.ReadInt16();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}