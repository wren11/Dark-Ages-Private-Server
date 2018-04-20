namespace Darkages.Network.ServerFormats
{
    public class ServerFormat4B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4B;

        public byte Type { get; set; }
        public uint Serial { get; set; }
        public byte ItemSlot { get; set; }

        public ServerFormat4B(uint serial, byte type, byte itemSlot = 0)
        {
            Type = type;
            Serial = serial;
            ItemSlot = itemSlot;
        }

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
