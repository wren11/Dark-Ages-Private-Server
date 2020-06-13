namespace Darkages.Network.ClientFormats
{
    public class ClientFormat1D : NetworkFormat
    {
        public byte Number;

        public ClientFormat1D()
        {
            Secured = true;
            Command = 0x1D;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Number = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}