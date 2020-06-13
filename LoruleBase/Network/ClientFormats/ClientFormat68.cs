namespace Darkages.Network.ClientFormats
{
    public class ClientFormat68 : NetworkFormat
    {
        public ClientFormat68()
        {
            Secured = true;
            Command = 0x68;
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