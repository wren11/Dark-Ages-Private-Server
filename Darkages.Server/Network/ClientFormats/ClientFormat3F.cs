namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3F : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x3F;

        public int Index { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Index = reader.ReadInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}