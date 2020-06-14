namespace Darkages.Network.ServerFormats
{
    public class ServerFormat4C : NetworkFormat
    {
        public ServerFormat4C()
        {
            Secured = true;
            Command = 0x4C;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x01);
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
        }
    }
}