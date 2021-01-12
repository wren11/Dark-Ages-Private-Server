namespace Darkages.Network.ClientFormats
{
    public class ClientFormat00 : NetworkFormat
    {
        public byte UnknownA;
        public byte UnknownB;
        public int Version;

        public ClientFormat00()
        {
            Secured = false;
            Command = 0x00;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Version = reader.ReadUInt16();
            UnknownA = reader.ReadByte();
            UnknownB = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}