using Darkages.Security;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat10 : NetworkFormat
    {
        public override byte Command => 0x10;

        public override bool Secured => false;

        public SecurityParameters Parameters { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Parameters = reader.ReadObject<SecurityParameters>();
            Name = reader.ReadStringA();
            Id = reader.ReadInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}