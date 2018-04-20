namespace Darkages.Network.ServerFormats
{
    public class ServerFormat4C : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4C;

        public byte Data => 0x01;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Data);
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
        }
    }
}