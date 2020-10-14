namespace Darkages.Network.ServerFormats
{
    public class ServerFormat31 : NetworkFormat
    {
        public ServerFormat31()
        {
            Secured = true;
            Command = 0x31;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}