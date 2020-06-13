namespace Darkages.Network.ServerFormats
{
    public class ServerFormat20 : NetworkFormat
    {
        public byte Shade;

        public byte Unknown = 0x01;

        public ServerFormat20()
        {
            Secured = true;
            Command = 0x20;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Shade);
            writer.Write(Unknown);
        }
    }
}