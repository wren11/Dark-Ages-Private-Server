namespace Darkages.Network.ClientFormats
{
    public class ClientFormat19 : NetworkFormat
    {
        public ClientFormat19()
        {
            Secured = true;
            Command = 0x19;
        }

        public string Message { get; set; }
        public string Name { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Name = reader.ReadStringA();
            Message = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}