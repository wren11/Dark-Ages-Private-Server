namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3C : NetworkFormat
    {
        public byte[] Data;

        public ushort Line;

        public ServerFormat3C()
        {
            Secured = true;
            Command = 0x3C;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Line);
            writer.Write(Data);
        }
    }
}