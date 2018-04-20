namespace Darkages.Network.ServerFormats
{
    public class ServerFormat73 : NetworkFormat
    {
        public override bool Secured => true;
        public override byte Command => 0x73;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(byte.MinValue);
        }
    }
}