namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3F : NetworkFormat
    {
        public int Index;

        public ClientFormat3F()
        {
            Secured = true;
            Command = 0x3F;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Index = reader.ReadInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}