namespace Darkages.Network.ServerFormats
{
    public class ServerFormat38 : NetworkFormat
    {
        public ServerFormat38(byte slot)
        {
            Slot = slot;
        }

        public override bool Secured => true;

        public override byte Command => 0x38;

        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter packet)
        {
            packet.Write(Slot);
        }
    }
}