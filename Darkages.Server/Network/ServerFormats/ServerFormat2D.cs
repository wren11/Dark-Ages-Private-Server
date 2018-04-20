namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2D : NetworkFormat
    {
        public ServerFormat2D(byte slot)
        {
            Slot = slot;
        }

        public override bool Secured => true;

        public override byte Command => 0x2D;

        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Slot);
        }
    }
}