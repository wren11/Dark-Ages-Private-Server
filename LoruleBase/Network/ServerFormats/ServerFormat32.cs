namespace Darkages.Network.ServerFormats
{
    public class ServerFormat32 : NetworkFormat
    {
        public ServerFormat32()
        {
            Secured = true;
            Command = 0x32;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x00);
        }
    }
}