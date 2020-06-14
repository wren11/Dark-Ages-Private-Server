namespace Darkages.Network.ServerFormats
{
    public class ServerFormat4B : NetworkFormat
    {
        public ServerFormat4B()
        {
            Secured = true;
            Command = 0x4B;
        }

        public ServerFormat4B(uint serial, byte type, byte itemSlot = 0) : this()
        {
            Type = type;
            Serial = serial;
            ItemSlot = itemSlot;
        }

        public byte ItemSlot { get; set; }
        public uint Serial { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (Type == 0)
            {
                writer.Write((ushort)0x06);
                writer.Write((byte)0x4A);
                writer.Write((byte)0x00);
                writer.Write(Serial);
                writer.Write((byte)0x00);
            }

            if (Type == 1)
            {
                writer.Write((ushort)0x07);
                writer.Write((byte)0x4A);
                writer.Write((byte)0x01);
                writer.Write(Serial);
                writer.Write(ItemSlot);
                writer.Write((byte)0x00);
            }
        }
    }
}