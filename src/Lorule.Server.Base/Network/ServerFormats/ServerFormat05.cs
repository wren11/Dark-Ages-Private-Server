namespace Darkages.Network.ServerFormats
{
    public class ServerFormat05 : NetworkFormat
    {
        public Aisling Aisling;

        public ServerFormat05()
        {
            Secured = true;
            Command = 0x05;
        }

        public ServerFormat05(Aisling aisling) : this()
        {
            Aisling = aisling;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Aisling.Serial);
            writer.Write(new byte[]
            {
                0x02,
                0x00,
                0x01,
                0x00,
                0x00,
                0x00
            });
        }
    }
}