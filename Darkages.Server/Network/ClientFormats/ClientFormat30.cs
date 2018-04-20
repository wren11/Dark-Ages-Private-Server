using Darkages.Types;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat30 : NetworkFormat
    {
        public override bool Secured => true;
        public override byte Command => 0x30;

        public Pane PaneType { get; set; }
        public byte MovingFrom { get; set; }
        public byte MovingTo { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            PaneType = (Pane)reader.ReadByte();
            MovingFrom = reader.ReadByte();
            MovingTo = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}