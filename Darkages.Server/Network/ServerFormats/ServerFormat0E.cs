namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0E : NetworkFormat
    {
        public ServerFormat0E(int serial)
        {
            Serial = serial;
        }

        public override bool Secured => true;

        public override byte Command => 0x0E;

        public int Serial { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Serial);
        }
    }
}