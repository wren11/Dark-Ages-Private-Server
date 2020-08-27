#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    public class ClientVersion : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x00;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((ushort) 718);
            writer.Write(0x4C);
            writer.Write(0x4B);
            writer.Write(0x00);
        }
    }
}