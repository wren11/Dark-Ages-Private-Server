namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0C : NetworkFormat
    {
        public byte Direction;

        public int Serial;
        public short X;
        public short Y;

        public ServerFormat0C()
        {
            Secured = true;
            Command = 0x0C;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((uint) Serial);
            writer.Write((ushort) X);
            writer.Write((ushort) Y);
            writer.Write(Direction);
            writer.Write((byte) 0x00);
        }
    }
}