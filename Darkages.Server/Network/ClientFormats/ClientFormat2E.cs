namespace Darkages.Network.ClientFormats
{
    public class ClientFormat2E : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x2E;

        public byte Type { get; set; }
        public string Name { get; set; }
        public bool ShowOnMap { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            if (Type == 0x02)
                Name = reader.ReadStringA();

            if (Type == 0x08)
            {
                Name = reader.ReadStringA();
                ShowOnMap = reader.ReadBool();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}