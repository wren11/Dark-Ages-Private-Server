namespace Darkages.Network.ServerFormats
{
    public class ServerFormat1A : NetworkFormat
    {
        public byte Number;

        public int Serial;
        public short Speed;

        public ServerFormat1A()
        {
            Secured = true;
            Command = 0x1A;
        }


        public ServerFormat1A(int serial, byte number, short speed) : this()
        {
            Serial = serial;
            Number = number;
            Speed = speed;
        }

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