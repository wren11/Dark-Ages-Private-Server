namespace Darkages.Network.ServerFormats
{
    public class ServerFormat49 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x49;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(byte.MinValue);
        }
    }
}