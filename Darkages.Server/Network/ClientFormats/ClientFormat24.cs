namespace Darkages.Network.ClientFormats
{
    public class ClientFormat24 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x24;

        public int GoldAmount { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Unknown { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            GoldAmount = reader.ReadInt32();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();

            if (reader.CanRead)
                Unknown = reader.ReadInt16();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}