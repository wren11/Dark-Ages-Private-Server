namespace Darkages.Network.ClientFormats
{
    public class ClientFormat1B : NetworkFormat
    {
        public int Index;

        public ClientFormat1B()
        {
            Secured = true;
            Command = 0x1B;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}