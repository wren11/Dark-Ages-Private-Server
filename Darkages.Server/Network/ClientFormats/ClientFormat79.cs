using Darkages.Types;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat79 : NetworkFormat
    {
        public override bool Secured => true;
        public override byte Command => 0x79;
        public ActivityStatus Status { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Status = (ActivityStatus)reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}