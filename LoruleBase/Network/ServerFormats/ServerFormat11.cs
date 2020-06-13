namespace Darkages.Network.ServerFormats
{
    public class ServerFormat11 : NetworkFormat
    {
        public byte Direction;

        public int Serial;

        public ServerFormat11()
        {
            Secured = true;
            Command = 0x11;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Serial);
            writer.Write(Direction);
        }
    }
}