namespace Darkages.Network.ServerFormats
{
    public class ServerFormat73 : NetworkFormat
    {
        public ServerFormat73()
        {
            Secured = true;
            Command = 0x73;
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