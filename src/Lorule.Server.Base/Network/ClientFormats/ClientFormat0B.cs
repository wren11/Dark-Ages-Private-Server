namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0B : NetworkFormat
    {
        public ClientFormat0B()
        {
            Secured = true;
            Command = 0x0B;
        }

        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}