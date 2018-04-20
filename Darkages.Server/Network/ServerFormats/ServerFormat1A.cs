namespace Darkages.Network.ServerFormats
{
    public class ServerFormat1A : NetworkFormat
    {
        public ServerFormat1A()
        {
        }

        public ServerFormat1A(int serial, byte number, short speed)
        {
            Serial = serial;
            Number = number;
            Speed = speed;
        }

        public override bool Secured => true;

        public override byte Command => 0x1A;

        public int Serial { get; set; }
        public byte Number { get; set; }
        public short Speed { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Serial);
            writer.Write(Number);
            writer.Write(Speed);
            writer.Write(byte.MaxValue);
        }
    }
}