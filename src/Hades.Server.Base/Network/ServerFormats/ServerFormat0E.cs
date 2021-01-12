namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0E : NetworkFormat
    {
        public int Serial;

        public ServerFormat0E(int serial) : this()
        {
            Serial = serial;
        }

        public ServerFormat0E()
        {
            Secured = true;
            Command = 0x0E;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Serial);
        }
    }
}