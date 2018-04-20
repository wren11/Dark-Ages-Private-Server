namespace Darkages.Network.ClientFormats
{
    public class ClientFormat4E : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x4E;

        public string Message { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Message = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}