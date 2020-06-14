namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4A : NetworkFormat
    {
        public ClientFormat4A()
        {
            Secured = true;
            Command = 0x4A;
        }

        public int Gold { get; set; }
        public uint Id { get; set; }
        public byte ItemSlot { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Id = reader.ReadUInt32();

            if (Type == 0x01 && reader.GetCanRead())
                ItemSlot = reader.ReadByte();

            if (Type == 0x03 && reader.GetCanRead()) Gold = reader.ReadInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}