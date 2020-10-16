#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    public class CreateAisling : NetworkFormat
    {
        public byte Gender { get; set; }
        public byte HairColor { get; set; }
        public byte HairStyle { get; set; }

        public override bool Secured => true;
        public override byte Command => 0x04;

        public override void Serialize(NetworkPacketReader reader)
        {
            HairStyle = reader.ReadByte();
            Gender = reader.ReadByte();
            HairColor = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte) 2);
            writer.Write((byte) 0);
            writer.Write((byte) 1);
        }
    }
}