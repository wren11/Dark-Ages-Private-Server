namespace Darkages.Network.ServerFormats
{
    public class ServerFormat31 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x31;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}