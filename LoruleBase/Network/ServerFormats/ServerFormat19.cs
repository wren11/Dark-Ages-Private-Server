namespace Darkages.Network.ServerFormats
{
    public class ServerFormat19 : NetworkFormat
    {
        public short Number;

        public ServerFormat19()
        {
            Secured = true;
            Command = 0x19;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x00);
            writer.Write((ushort)Number);
        }
    }
}