namespace Darkages.Network.ClientFormats
{
    public class ClientFormat1C : NetworkFormat
    {
        public ClientFormat1C()
        {
            Secured = true;
            Command = 0x1C;
        }

        public byte Index { get; private set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}