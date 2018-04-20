namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x0B;

        public byte Direction { get; set; }
        public ushort LastX { get; set; }
        public ushort LastY { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Direction);
            writer.Write(LastX);
            writer.Write(LastY);
            writer.Write((ushort)0x0B);
            writer.Write((ushort)0x0B);
            writer.Write((byte)0x01);
        }
    }
}