namespace Darkages.Network.ServerFormats
{
    public class ServerFormat18 : NetworkFormat
    {
        public ServerFormat18(byte slot)
        {
            Slot = slot;
        }

        public override bool Secured => true;

        public override byte Command => 0x18;

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