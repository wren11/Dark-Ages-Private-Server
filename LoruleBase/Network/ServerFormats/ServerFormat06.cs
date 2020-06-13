namespace Darkages.Network.ServerFormats
{
    public class ServerFormat06 : NetworkFormat
    {
        public ServerFormat06()
        {
            Secured = true;
            Command = 0x06;
        }


        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}