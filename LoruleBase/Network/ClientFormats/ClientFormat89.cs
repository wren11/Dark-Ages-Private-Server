namespace Darkages.Network.ClientFormats
{
    public class ClientFormat89 : NetworkFormat
    {
        public ClientFormat89()
        {
            Secured = true;
            Command = 0x89;
        }

        public ushort DisplayMask { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            DisplayMask = reader.ReadUInt16();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}