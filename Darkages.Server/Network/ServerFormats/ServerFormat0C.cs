namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0C : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x0C;

        public int Serial { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public byte Direction { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((uint)Serial);
            writer.Write((ushort)X);
            writer.Write((ushort)Y);
            writer.Write(Direction);
            writer.Write((byte)0x00);
        }
    }
}