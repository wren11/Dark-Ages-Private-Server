namespace Darkages.Network.ClientFormats
{
    public class ClientFormat11 : NetworkFormat
    {
        public ClientFormat11()
        {
            Secured = true;
            Command = 0x11;
        }

        public byte Direction { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Direction = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}