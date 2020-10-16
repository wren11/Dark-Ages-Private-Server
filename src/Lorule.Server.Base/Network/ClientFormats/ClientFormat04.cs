namespace Darkages.Network.ClientFormats
{
    public class ClientFormat04 : NetworkFormat
    {
        public ClientFormat04()
        {
            Secured = true;
            Command = 0x04;
        }

        public byte Gender { get; set; }
        public byte HairColor { get; set; }
        public byte HairStyle { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            HairStyle = reader.ReadByte();
            Gender = reader.ReadByte();
            HairColor = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}