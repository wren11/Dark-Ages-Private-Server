namespace Darkages.Network.ServerFormats
{
    public class ServerFormat48 : NetworkFormat
    {
        public ServerFormat48()
        {
            Secured = true;
            Command = 0x48;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(byte.MinValue);
        }
    }
}